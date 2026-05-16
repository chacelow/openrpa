using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenRPA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        public static System.Windows.Forms.Form _splashForm;
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nL, int nT, int nR, int nB, int nWE, int nHE);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
        [STAThread]
        public static void Main()
        {
            // Show native WinForms splash instantly (before WPF assemblies load)
            var splashThread = new System.Threading.Thread(() =>
            {
                _splashForm = new System.Windows.Forms.Form()
                {
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                    StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                    Size = new System.Drawing.Size(420, 340),
                    TopMost = true,
                    ShowInTaskbar = false,
                    BackColor = System.Drawing.Color.White
                };
                var logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenRPA-logo.png");
                var logo = new System.Windows.Forms.PictureBox()
                {
                    Image = System.Drawing.Image.FromFile(logoPath),
                    SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom,
                    Size = new System.Drawing.Size(100, 100),
                    Location = new System.Drawing.Point(160, 30)
                };
                var title = new System.Windows.Forms.Label()
                {
                    Text = "OpenRPA",
                    Font = new System.Drawing.Font("Segoe UI", 24, System.Drawing.FontStyle.Regular),
                    ForeColor = System.Drawing.Color.FromArgb(45, 45, 45),
                    AutoSize = true,
                    Location = new System.Drawing.Point(130, 140)
                };
                var status = new System.Windows.Forms.Label()
                {
                    Text = "◌ 正在启动...",
                    Font = new System.Drawing.Font("Segoe UI", 10),
                    ForeColor = System.Drawing.Color.FromArgb(100, 100, 100),
                    AutoSize = true,
                    Location = new System.Drawing.Point(140, 200),
                    Name = "StatusLabel"
                };
                _splashForm.Controls.Add(logo);
                _splashForm.Controls.Add(title);
                _splashForm.Controls.Add(status);
                _splashForm.Load += (s, ev) => {
                    var rgn = CreateRoundRectRgn(0, 0, _splashForm.Width + 1, _splashForm.Height + 1, 16, 16);
                    SetWindowRgn(_splashForm.Handle, rgn, true);
                };
                _splashForm.Show();
                // Spin animation timer
                var spinChars = new[] { "◌", "◐", "◓", "◑", "◒" };
                int spinIdx = 0;
                var spinTimer = new System.Windows.Forms.Timer { Interval = 150 };
                spinTimer.Tick += (s, ev) => { spinIdx = (spinIdx + 1) % spinChars.Length; status.Text = spinChars[spinIdx] + " " + GetCurrentStatus(); };
                spinTimer.Start();
                System.Windows.Forms.Application.Run();
            });
            splashThread.SetApartmentState(System.Threading.ApartmentState.STA);
            splashThread.Start();

            if (SingleInstance<App>.InitializeAsFirstInstance("OpenRPA"))
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                // AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceHandler;
                try
                {
                    var args = Environment.GetCommandLineArgs();
                    CommandLineParser parser = new CommandLineParser();
                    // parser.Parse(string.Join(" ", args), true);
                    var options = parser.Parse(args, true);
                    if (options.ContainsKey("workingdir"))
                    {
                        var filepath = options["workingdir"].ToString();
                        if (System.IO.Directory.Exists(filepath))
                        {
                            Log.ResetLogPath(filepath);
                        }
                        else
                        {
                            MessageBox.Show("Path not found " + filepath);
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                }
                var application = new App();
                application.InitializeComponent();
                application.Run();
                // application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Log.Function("MainWindow", "CurrentDomain_UnhandledException");
            try
            {
                Exception ex = (Exception)args.ExceptionObject;
                Log.Error(ex.ToString());
                Log.Error("MyHandler caught : " + ex.Message);
                Log.Error("Runtime terminating: {0}", (args.IsTerminating).ToString());
            }
            catch (Exception)
            {
            }
        }
        public static System.Windows.Forms.NotifyIcon notifyIcon { get; set; } = new System.Windows.Forms.NotifyIcon();
        public App()
        {
            if (!string.IsNullOrEmpty(Config.local.culture))
            {
                try
                {
                    var cultur = System.Globalization.CultureInfo.GetCultureInfo(Config.local.culture);
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultur;
                    System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultur;
                    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultur;
                    ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
                    foreach (object obj in currentThreads)
                    {
                        try
                        {
                            Thread t = obj as Thread;
                            if (t != null)
                            {
                                t.CurrentUICulture = cultur;
                                t.CurrentCulture = cultur;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }


                }
                catch (Exception)
                {
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSameFolder);
            try
            {
                var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/open_rpa.ico")).Stream;
                notifyIcon.Icon = new System.Drawing.Icon(iconStream);
                notifyIcon.Visible = false;
                //notifyIcon.ShowBalloonTip(5000, "Title", "Text", System.Windows.Forms.ToolTipIcon.Info);
                notifyIcon.Click += nIcon_Click;
                notifyIcon.DoubleClick += nIcon_Click;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(System.IO.Path.Combine(target.FullName, file.Name));
            }
        }
        static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string assemblyPath = "";
            if (args != null && !string.IsNullOrEmpty(args.Name)) assemblyPath = args.Name;
            try
            {
                assemblyPath = new AssemblyName(args.Name).Name + ".dll";
            }
            catch (Exception)
            {
            }
            try
            {
                if (args.Name.StartsWith("CefSharp"))
                {
                    string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                    string archSpecificPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                           Environment.Is64BitProcess ? "x64" : "x86",
                                                           assemblyName);

                    return File.Exists(archSpecificPath)
                               ? Assembly.LoadFile(archSpecificPath)
                               : null;
                }
                string folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                assemblyPath = System.IO.Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
                if (System.IO.File.Exists(assemblyPath)) return Assembly.LoadFrom(assemblyPath);

                folderPath = Interfaces.Extensions.PluginsDirectory;
                assemblyPath = System.IO.Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
                if (System.IO.File.Exists(assemblyPath)) return Assembly.LoadFrom(assemblyPath);

                folderPath = Path.Combine(Interfaces.Extensions.ProjectsDirectory, "extensions");
                assemblyPath = System.IO.Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
                if (System.IO.File.Exists(assemblyPath)) return Assembly.LoadFrom(assemblyPath);

                folderPath = System.IO.Path.GetTempPath();
                assemblyPath = System.IO.Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
                if (System.IO.File.Exists(assemblyPath)) return Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                Log.Error(assemblyPath);
                Log.Error(ex.ToString());
            }
            return null;
        }
        void nIcon_Click(object sender, EventArgs e)
        {
            GenericTools.Restore();
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (notifyIcon != null)
            {
                if (notifyIcon.Icon != null) notifyIcon.Icon.Dispose();
                notifyIcon.Dispose();
            }
        }
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            nIcon_Click(null, null);
            RobotInstance.instance.ParseCommandLineArgs(args);
            return true;
        }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                AutomationHelper.syncContext = System.Threading.SynchronizationContext.Current;
                System.Threading.Thread.CurrentThread.Name = "UIThread";
                if (!Config.local.isagent)
                {
                    StartupUri = new Uri("/OpenRPA;component/MainWindow.xaml", UriKind.Relative);
                    notifyIcon.Visible = false;
                }
                else
                {
                    StartupUri = new Uri("/OpenRPA;component/AgentWindow.xaml", UriKind.Relative);
                    notifyIcon.Visible = true;
                }
                if (Config.local.files_pending_deletion.Length > 0)
                {
                    bool sucess = true;
                    foreach (var f in Config.local.files_pending_deletion)
                    {
                        try
                        {
                            if (System.IO.File.Exists(f)) System.IO.File.Delete(f);
                        }
                        catch (Exception ex)
                        {
                            sucess = false;
                            Log.Error(ex.ToString());
                        }
                    }
                    if (sucess)
                    {
                        Config.local.files_pending_deletion = new string[] { };
                        Config.Save();
                    }
                }

                if (Config.local.restore_dependencies_on_startup)
                {
                    Log.Debug("Package restore on startup enabled -> cleaning existing extensions.");
                    var extensionsPath = Path.Combine(Interfaces.Extensions.ProjectsDirectory, "extensions");
                    if (Directory.Exists(extensionsPath))
                    {
                        foreach (var file in Directory.GetFiles(extensionsPath))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Could not clean extension: " + ex.ToString());
                            }
                        }
                    }
                }

                RobotInstance.instance.Status += App_Status;
                Input.InputDriver.Instance.initCancelKey(Config.local.cancelkey);
                SetSplashStatus("加载插件...", "");
                Plugins.LoadPlugins(RobotInstance.instance, Interfaces.Extensions.PluginsDirectory, false);
                SetSplashStatus("初始化...", "");
                RobotInstance.instance.Initialize();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Console.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message);
            }

            await Task.Run(async () =>
            {
                try
                {
                    // if (Config.local.showloadingscreen) splash.BusyContent = "loading plugins";
                    // Plugins.LoadPlugins(RobotInstance.instance, Interfaces.Extensions.ProjectsDirectory);
                    // if (Config.local.showloadingscreen) splash.BusyContent = "Initialize main window";
                    await RobotInstance.instance.init();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show(ex.Message);
                }
            });
        }
        private static string _splashStatus = "正在启动...";
        private static string GetCurrentStatus() => _splashStatus;

        public static void SetSplashStatus(string status, string module)
        {
            _splashStatus = status;
            if (_splashForm != null && _splashForm.IsHandleCreated)
                _splashForm.Invoke((Action)(() => {
                    foreach (System.Windows.Forms.Control c in _splashForm.Controls)
                        if (c is System.Windows.Forms.Label l && l.Name == "StatusLabel")
                            l.Text = l.Text.Substring(0, 2) + status;
                }));
        }

        private void App_Status(string message)
        {
            try
            {
                Log.Debug(message);
                SetSplashStatus(message, "");
            }
            catch (Exception) { }
        }
    }
}
