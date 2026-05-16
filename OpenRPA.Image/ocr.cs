using Emgu.CV;
using Emgu.CV.CvEnum;
using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace OpenRPA.Image
{
    public class ocr
    {
        public static void TesseractDownloadLangFile(string folder, string lang)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string dest = Path.Combine(folder, String.Format("{0}.traineddata", lang));
            if (!File.Exists(dest))
                using (System.Net.WebClient webclient = new System.Net.WebClient())
                {
                    var Expect100Continue = System.Net.ServicePointManager.Expect100Continue;
                    var SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol;
                    System.Net.ServicePointManager.Expect100Continue = true;
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Ssl3;
                    string source = string.Format("https://github.com/tesseract-ocr/tessdata/raw/main/{0}.traineddata", lang);
                    System.Diagnostics.Trace.WriteLine(string.Format("Downloading file from '{0}' to '{1}'", source, dest));
                    webclient.DownloadFile(source, dest);
                    System.Diagnostics.Trace.WriteLine(string.Format("Download completed"));
                    System.Net.ServicePointManager.Expect100Continue = Expect100Continue;
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocol;
                }
        }

        private static Pix MatToPix(Mat mat)
        {
            using (var bitmap = mat.ToBitmap())
            {
                // Tesseract PixConverter works with Bitmap
                return Pix.LoadFromMemory(RawBitmapToBytes(bitmap));
            }
        }

        private static byte[] RawBitmapToBytes(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static string OcrImage(TesseractEngine engine, Mat image)
        {
            Mat processed = null;
            Mat imgGrey = null;
            Mat imgThresholded = null;
            try
            {
                processed = new Mat();
                imgGrey = new Mat();
                imgThresholded = new Mat();

                if (image.NumberOfChannels == 1)
                    CvInvoke.CvtColor(image, processed, ColorConversion.Gray2Bgr);
                else
                    image.CopyTo(processed);

                // Try color first
                string result = Recognize(engine, processed);
                if (!string.IsNullOrWhiteSpace(result)) return result;

                // Try grayscale
                CvInvoke.CvtColor(image, imgGrey, ColorConversion.Bgr2Gray);
                result = Recognize(engine, imgGrey);
                if (!string.IsNullOrWhiteSpace(result)) return result;

                // Try thresholded
                CvInvoke.Threshold(imgGrey, imgThresholded, 65, 255, ThresholdType.Binary);
                result = Recognize(engine, imgThresholded);
                return result ?? "";
            }
            finally
            {
                processed?.Dispose();
                imgGrey?.Dispose();
                imgThresholded?.Dispose();
            }
        }

        private static string Recognize(TesseractEngine engine, Mat image)
        {
            using (var pix = MatToPix(image))
            using (var page = engine.Process(pix))
            {
                return page.GetText().TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        public static ImageElement[] OcrImage2(TesseractEngine engine, Mat image, string wordlimit, bool casesensitive)
        {
            using (var pix = MatToPix(image))
            using (var page = engine.Process(pix))
            {
                var result = new List<ImageElement>();
                var wordresult = new List<ImageElement>();

                using (var iter = page.GetIterator())
                {
                    iter.Begin();
                    do
                    {
                        var wordText = iter.GetText(PageIteratorLevel.Word);
                        float conf = iter.GetConfidence(PageIteratorLevel.Word);
                        Tesseract.Rect rect;
                        iter.TryGetBoundingBox(PageIteratorLevel.Word, out rect);

                        // Build result entry for each word
                        if (!string.IsNullOrEmpty(wordText) && wordText != " " && wordText != "\n" && wordText != "\r")
                        {
                            var res = new ImageElement(Rectangle.Empty);
                            res.Text = wordText.Trim();
                            res.Confidence = conf;
                            res.Rectangle = new System.Drawing.Rectangle(rect.X1, rect.Y1, rect.Width, rect.Height);
                            result.Add(res);

                            // Word limit search
                            if (!string.IsNullOrEmpty(wordlimit))
                            {
                                if (casesensitive
                                    ? wordText.Trim() == wordlimit
                                    : wordText.Trim().ToLower() == wordlimit.ToLower())
                                {
                                    wordresult.Add(res);
                                }
                            }
                        }

                    } while (iter.Next(PageIteratorLevel.Word));
                }

                if (!string.IsNullOrEmpty(wordlimit)) return wordresult.ToArray();
                return result.ToArray();
            }
        }
    }
}
