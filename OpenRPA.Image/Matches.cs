using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA.Image
{
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Structure;
    using Emgu.CV.Util;
    using OpenRPA.Interfaces;
    using System.Drawing;
    static class Matches
    {
        // https://stackoverflow.com/questions/8218997/how-to-detect-the-sun-from-the-space-sky-in-opencv/8221251#8221251
        // https://stackoverflow.com/questions/30867391/how-to-call-opencvs-matchtemplate-method-from-c-sharp


        public static Bitmap DrawRectangle(Bitmap Source, Rectangle rect)
        {
            var img = Source.ToImage<Bgr, byte>();
            img.Draw(rect, new Bgr(0, 0, 255), 2);
            return img.ToBitmap();
        }

        public static Rectangle FindMatch(Bitmap Source, Bitmap Template, double Threshold)
        {
            return FindMatch(Source.ToImage<Bgr, byte>(), Template.ToImage<Bgr, byte>(), Threshold);
        }
        public static Rectangle FindMatch(Image<Bgr, byte> Source, Image<Bgr, byte> Template, double Threshold)
        {
            try
            {
                using (Image<Gray, float> result = Source.MatchTemplate(Template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    // between 0.75 and 0.95 would be good.
                    if (maxValues[0] > Threshold)
                    {
                        Rectangle match = new Rectangle(maxLocations[0], Template.Size);
                        return match;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
            return Rectangle.Empty;
        }

        public static Rectangle[] FindMatchesMultiScale(Bitmap Source, Bitmap Template, double Threshold, int maxResults, bool asGray)
        {
            // Try 1:1 first
            var result = FindMatches(Source, Template, Threshold, maxResults, asGray);
            if (result.Length > 0) return result;

            // Multi-scale fallback: 0.6x ~ 2.0x
            double[] scales = { 0.6, 0.75, 0.9, 1.1, 1.3, 1.5, 2.0 };
            double bestScore = 0;
            Rectangle bestRect = Rectangle.Empty;
            double bestScale = 1.0;

            using (var srcToMatch = Source.ToImage<Bgr, byte>())
            {
                foreach (var scale in scales)
                {
                    int sw = (int)(Template.Width * scale);
                    int sh = (int)(Template.Height * scale);
                    if (sw < 5 || sh < 5) continue;
                    if (sw > Source.Width || sh > Source.Height) continue;

                    using (var scaledTpl = new Bitmap(Template, new System.Drawing.Size(sw, sh)))
                    using (var tplToMatch = scaledTpl.ToImage<Bgr, byte>())
                    using (var matchResult = srcToMatch.MatchTemplate(tplToMatch, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minV, maxV;
                        Point[] minL, maxL;
                        matchResult.MinMax(out minV, out maxV, out minL, out maxL);
                        if (maxV[0] > bestScore)
                        {
                            bestScore = maxV[0];
                            bestRect = new Rectangle(maxL[0], new System.Drawing.Size(sw, sh));
                            bestScale = scale;
                        }
                    }
                }
            }

            if (bestScore >= Threshold && bestRect.Width > 0)
            {
                Log.Information(string.Format("FindMatchesMultiScale: found at scale={0:F2}x score={1:P1}", bestScale, bestScore));
                return new Rectangle[] { bestRect };
            }
            Log.Information(string.Format("FindMatchesMultiScale: no match above threshold, best={0:P1} scale={1:F2}x", bestScore, bestScale));
            return new Rectangle[] { };
        }

        public static Rectangle[] FindMatches(Bitmap Source, Bitmap Template, double Threshold, int maxResults, bool asGray)
        {
            try
            {
                if(Template == null || Source == null) return new Rectangle[] { };
                if (Template.Width > Source.Width) throw new ArgumentException("Template is wider than the source");
                if (Template.Height > Source.Height) throw new ArgumentException("Template is higher than the source");
                if (asGray)
                {
                    using (var source = Source.ToImage<Gray, byte>())
                    {
                        using (var template = Template.ToImage<Gray, byte>())
                        {
                            //Interfaces.Image.Util.SaveImageStamped(source.Bitmap, "c:\\temp", "FindMatches-source");
                            //Interfaces.Image.Util.SaveImageStamped(template.Bitmap, "c:\\temp", "FindMatches-template");
                            //rpaactivities.image.util.saveImage(source, "FindMatches-source");
                            //rpaactivities.image.util.saveImage(template, "FindMatches-template");
                            var result = FindMatches(source, template, Threshold, maxResults);
                            //image.util.showImage(template);
                            return result;
                        }
                    }
                }
                else
                {
                    using (var source = Source.ToImage<Bgr, byte>())
                    {
                        using (var template = Template.ToImage<Bgr, byte>())
                        {
                            //Interfaces.Image.Util.SaveImageStamped(source.Bitmap, "c:\\temp", "FindMatches-source");
                            //Interfaces.Image.Util.SaveImageStamped(template.Bitmap, "c:\\temp", "FindMatches-template");
                            //rpaactivities.image.util.saveImage(source, "FindMatches-source");
                            //rpaactivities.image.util.saveImage(template, "FindMatches-template");
                            var result = FindMatches(source, template, Threshold, maxResults);
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
            return new Rectangle[] { };
        }
        private static Rectangle[] FindMatches<TColor, TDepth>(Image<TColor, TDepth> Source, Image<TColor, TDepth> Template, double Threshold, int maxResults, bool inverted = false)
        where TColor : struct, IColor
        where TDepth : new()
        {
            var result = new List<Rectangle>();
            double bestScore = 0;
            using (var Matches = Source.MatchTemplate(Template, TemplateMatchingType.CcoeffNormed))
            //using (var Matches = Source.MatchTemplate(Template, TemplateMatchingType.CcorrNormed))
            {
                try
                {
                    int matchcount = 0;
                    for (int y = 0; y < Matches.Data.GetLength(0); y++)
                    {
                        for (int x = 0; x < Matches.Data.GetLength(1); x++)
                        {
                            Double certain = Matches.Data[y, x, 0];
                            if (certain > bestScore) bestScore = certain;
                            if (Matches.Data[y, x, 0] >= Threshold) //Check if its a valid match
                            {
                                matchcount++;
                                bool canadd = true;
                                if (matchcount > maxResults) canadd = false;
                                if (canadd)
                                {
                                    var rect = new Rectangle(new Point(x, y), new Size(Template.Width, Template.Height));
                                    foreach (var _r in result.ToList())
                                    {
                                        if ((_r.Contains(rect) || _r.IntersectsWith(rect)) && !_r.Equals(rect))
                                        {
                                            canadd = false;
                                        }
                                    }
                                    if (canadd) result.Add(rect);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }

            }
            if (result.Count > 0)
            {
                Log.Information(string.Format("FindMatches: {0} result(s) threshold={1:F2} best={2:F4}", result.Count, Threshold, bestScore));
                //if (imageDump)
                //{
                //    using (var _b = Source.ToBitmap())
                //    using (var saveimage = new Image<Emgu.CV.Structure.Bgr, Byte>(_b))
                //    {
                //        foreach (var match in result)
                //        {
                //            saveimage.Draw(match, new Bgr(System.Drawing.Color.Red), 2);
                //        }
                //        rpaactivities.image.util.saveImage(saveimage, "FindMatches-hits");
                //    }
                //}
            }

            if (result.Count == 0 && inverted == false)
            {
                Log.Information(string.Format("FindMatches: 0 results, threshold={0:F2} best={1:F4}", Threshold, bestScore));
                //using (var invertedimage = Template.Not())
                //{
                //    return FindMatches(Source, invertedimage, Threshold, maxResults, true);
                //}
            }
            return result.ToArray();
        }
        //public static Highlighter[] HighlightMatches(Rectangle[] Matches)
        //{
        //    var result = new List<Highlighter>();
        //    foreach (var m in Matches)
        //    {
        //        result.Add(new Highlighter(m, System.Drawing.Color.Red));
        //    }
        //    return result.ToArray();
        //}
        //public static void HighlightMatches(Rectangle[] Matches, TimeSpan Duration)
        //{
        //    foreach (var Match in Matches)
        //    {
        //        Task.Factory.StartNew(() =>
        //        {
        //            var h2 = new Highlighter(Match, System.Drawing.Color.Red);
        //            System.Threading.Thread.Sleep(Duration);
        //            h2.remove();
        //        });
        //    }
        //}
        //public static void HighlightMatch(Rectangle Match, bool Blocking, Color Color, TimeSpan Duration)
        //{
        //    if (!Blocking)
        //    {
        //        Task.Factory.StartNew(() =>
        //        {
        //            var h2 = new Highlighter(Match, System.Drawing.Color.Red);
        //            System.Threading.Thread.Sleep(Duration);
        //            h2.remove();
        //            System.Windows.Forms.Application.DoEvents();
        //        });
        //        return;
        //    }
        //    var h = new Highlighter(Match, System.Drawing.Color.Red);
        //    System.Threading.Thread.Sleep(Duration);
        //    h.remove();
        //    System.Windows.Forms.Application.DoEvents();
        //}






        public static Rectangle[] FindFeatureMatches(Bitmap Source, Bitmap Template, double Threshold, int maxResults)
        {
            return FindCvMatches(Source, Template, Threshold, maxResults, ImageMatchMode.SIFT);
        }

        public static Rectangle[] FindCvMatches(Bitmap source, Bitmap templ, double threshold, int maxResults, ImageMatchMode mode)
        {
            var result = new List<Rectangle>();
            try
            {
                var srcImg = source.ToImage<Bgr, byte>();
                var tplImg = templ.ToImage<Bgr, byte>();

                Emgu.CV.Features2D.Feature2D detector;
                if (mode == ImageMatchMode.AKAZE) detector = new Emgu.CV.Features2D.AKAZE();
                else if (mode == ImageMatchMode.KAZE) detector = new Emgu.CV.Features2D.KAZE();
                else if (mode == ImageMatchMode.ORB) detector = new Emgu.CV.Features2D.ORB();
                else detector = new Emgu.CV.Features2D.SIFT();

                var mkp = new VectorOfKeyPoint(); var okp = new VectorOfKeyPoint();
                var mdl = new Mat(); var odl = new Mat();
                detector.DetectAndCompute(tplImg.Mat, null, mkp, mdl, false);
                detector.DetectAndCompute(srcImg.Mat, null, okp, odl, false);

                int n0 = mkp.Size, n1 = okp.Size;
                Log.Information(string.Format("FindCvMatches: {0} detected {1}/{2} keypoints (tpl/src)", mode, n0, n1));
                if (n0 < 4 || n1 < 4) return result.ToArray();

                // Choose matcher based on descriptor type
                Emgu.CV.Features2D.DescriptorMatcher matcher;
                if (mode == ImageMatchMode.ORB || mode == ImageMatchMode.AKAZE)
                    matcher = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.Hamming, false);
                else
                    matcher = new Emgu.CV.Features2D.FlannBasedMatcher(new Emgu.CV.Flann.KdTreeIndexParams(), new Emgu.CV.Flann.SearchParams());

                var matches = new VectorOfVectorOfDMatch();
                using (matcher)
                {
                    matcher.Add(mdl);
                    matcher.KnnMatch(odl, matches, 2, null);
                }

                // Lowe's ratio test
                var goodPts1 = new List<PointF>(); var goodPts2 = new List<PointF>();
                double rt = 0.75; // ratio test threshold
                for (int i = 0; i < matches.Size; i++)
                {
                    var m = matches[i].ToArray();
                    if (m.Length < 2) continue;
                    if (m[0].Distance < rt * m[1].Distance)
                    {
                        if (m[0].TrainIdx < n0 && m[0].QueryIdx < n1)
                        {
                            goodPts1.Add(mkp[m[0].TrainIdx].Point);
                            goodPts2.Add(okp[m[0].QueryIdx].Point);
                        }
                    }
                }

                Log.Information(string.Format("FindCvMatches: {0} good matches after ratio test", goodPts1.Count));
                SaveDebugViz(source, templ, goodPts1, goodPts2, null, null);

                if (goodPts1.Count < 4) return result.ToArray();

                var mask = new Mat();
                var H = CvInvoke.FindHomography(goodPts1.ToArray(), goodPts2.ToArray(), RobustEstimationAlgorithm.Ransac, 8.0, mask);
                if (H == null || H.IsEmpty) return result.ToArray();

                int inliers = CvInvoke.CountNonZero(mask);
                double conf = (double)inliers / goodPts1.Count;
                Log.Information(string.Format("FindCvMatches: raw={0} ransac={1} confidence={2:P1}", goodPts1.Count, inliers, conf));

                if (conf < threshold) return result.ToArray();

                var corners = new PointF[] { new PointF(0, 0), new PointF(templ.Width, 0), new PointF(templ.Width, templ.Height), new PointF(0, templ.Height) };
                var warped = CvInvoke.PerspectiveTransform(corners, H);
                SaveDebugViz(source, templ, goodPts1, goodPts2, H, warped);

                float minX = warped.Min(p => p.X), maxX = warped.Max(p => p.X);
                float minY = warped.Min(p => p.Y), maxY = warped.Max(p => p.Y);
                int rx = Math.Max(0, (int)minX), ry = Math.Max(0, (int)minY);
                int rw = Math.Min(source.Width - rx, (int)(maxX - minX));
                int rh = Math.Min(source.Height - ry, (int)(maxY - minY));
                if (rw > 0 && rh > 0) result.Add(new Rectangle(rx, ry, rw, rh));
            }
            catch (Exception ex) { Log.Error("FindCvMatches: " + ex.Message); }
            return result.ToArray();
        }

        private static void SaveDebugViz(Bitmap src, Bitmap tpl, List<PointF> pts1, List<PointF> pts2, Mat homography, PointF[] warped)
        {
            try
            {
                var path = System.IO.Path.Combine(Interfaces.Extensions.MyPictures, "ImageMatchDebug");
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                var ts = DateTime.Now.ToString("HHmmss");

                using (var img = src.ToImage<Bgr, byte>())
                {
                    foreach (var p in pts2) img.Draw(new CircleF(p, 2), new Bgr(0, 255, 0), 2);
                    if (warped != null)
                    {
                        var poly = new Point[] { new Point((int)warped[0].X, (int)warped[0].Y), new Point((int)warped[1].X, (int)warped[1].Y),
                            new Point((int)warped[2].X, (int)warped[2].Y), new Point((int)warped[3].X, (int)warped[3].Y) };
                        img.Draw(poly, new Bgr(0, 255, 255), 3);
                        // Warped template overlay
                        if (homography != null && !homography.IsEmpty)
                        {
                            using (var warpedSrc = new Mat())
                            using (var tplImg = tpl.ToImage<Bgr, byte>())
                            {
                                CvInvoke.WarpPerspective(tplImg, warpedSrc, homography, new System.Drawing.Size(src.Width, src.Height));
                                CvInvoke.AddWeighted(img, 0.5, warpedSrc, 0.5, 0, img);
                            }
                        }
                    }
                    CvInvoke.Imwrite(System.IO.Path.Combine(path, ts + "_onnx_src.jpg"), img);
                }
                using (var img = tpl.ToImage<Bgr, byte>())
                {
                    foreach (var p in pts1) img.Draw(new CircleF(p, 2), new Bgr(0, 255, 0), 2);
                    CvInvoke.Imwrite(System.IO.Path.Combine(path, ts + "_onnx_tpl.jpg"), img);
                }
            }
            catch { }
        }

        private static void SaveDebugImage(Mat img, string name)
        {
            try
            {
                var path = System.IO.Path.Combine(Interfaces.Extensions.MyPictures, "ImageMatchDebug");
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                var file = System.IO.Path.Combine(path, DateTime.Now.ToString("HHmmss") + "_" + name + ".jpg");
                CvInvoke.Imwrite(file, img);
                Log.Debug("Debug image saved: " + file);
            }
            catch { }
        }

    }
}