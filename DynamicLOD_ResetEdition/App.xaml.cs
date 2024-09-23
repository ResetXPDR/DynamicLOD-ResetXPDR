﻿using H.NotifyIcon;
using Serilog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
namespace DynamicLOD_ResetEdition
{
    public partial class App : Application
    {
        private ServiceModel Model;
        private ServiceController Controller;

        private TaskbarIcon notifyIcon;

        public static new App Current => Application.Current as App;
        public static string ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DynamicLOD_ResetEdition\DynamicLOD_ResetEdition.config";
        public static string AppDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DynamicLOD_ResetEdition\bin";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Process.GetProcessesByName("DynamicLOD_ResetEdition").Length > 1)
            {
                MessageBox.Show("DynamicLOD_ResetEdition is already running!", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            if (Process.GetProcessesByName("DynamicLOD").Length > 0)
            {
                MessageBox.Show("A pre-ResetEdition version of DynamicLOD is already running!", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            if (Process.GetProcessesByName("MSFS2020_AutoFPS").Length > 0)
            {
                MessageBox.Show("MSFS2020_AutoFPS is already running!", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            if (Process.GetProcessesByName("SmoothFlight").Length > 0)
            {
                MessageBox.Show("SmoothFlight is already running!", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }


            Directory.SetCurrentDirectory(AppDir);

            if (!File.Exists(ConfigFile))
            {
                string ConfigFileDefault = Directory.GetCurrentDirectory() + @"\DynamicLOD_ResetEdition.config";
                if (!File.Exists(ConfigFileDefault))
                {
                    MessageBox.Show("No Configuration File found! Closing ...", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
                else
                {
                    File.Copy(ConfigFileDefault, ConfigFile);
                }
            }

            Model = new();
            InitLog();
            InitSystray();

            Controller = new(Model);
            Task.Run(Controller.Run);

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += OnTick;
            timer.Start();

            MainWindow = new MainWindow(notifyIcon.DataContext as NotifyIconViewModel, Model);
            if (Model.OpenWindow && Model.windowIsVisible)
                MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Model != null)
            {
                Model.CancellationRequested = true;
                if (Model.DefaultSettingsRead && Model.IsSessionRunning)
                {
                    Logger.Log(LogLevel.Information, "App:OnExit", $"Resetting LODs to {Model.DefaultTLOD} / {Model.DefaultOLOD} and VR {Model.DefaultTLOD_VR} / {Model.DefaultOLOD_VR}");
                    Model.MemoryAccess.SetTLOD_PC(Model.DefaultTLOD);
                    Model.MemoryAccess.SetTLOD_VR(Model.DefaultTLOD_VR);
                    Model.MemoryAccess.SetOLOD_PC(Model.DefaultOLOD);
                    Model.MemoryAccess.SetOLOD_VR(Model.DefaultOLOD_VR);
                    Logger.Log(LogLevel.Information, "App:OnExit", $"Resetting cloud quality to {ServiceModel.CloudQualityText(Model.DefaultCloudQ)}  / VR  {ServiceModel.CloudQualityText(Model.DefaultCloudQ_VR)}");
                    Model.MemoryAccess.SetCloudQ(Model.DefaultCloudQ);
                    Model.MemoryAccess.SetCloudQ_VR(Model.DefaultCloudQ_VR);
                }
            }
            notifyIcon?.Dispose();
            base.OnExit(e);

            Logger.Log(LogLevel.Information, "App:OnExit", "DynamicLOD_ResetEdition exiting ...");
        }

        protected void OnTick(object sender, EventArgs e)
        {
            if ((MainWindow.Visibility == Visibility.Visible && !Model.windowIsVisible) || (MainWindow.Visibility == Visibility.Hidden && Model.windowIsVisible)) Model.SetSetting("windowIsVisible", Model.windowIsVisible ? "false" : "true");
            if (Model.ServiceExited)
            {
                Current.Shutdown();
            }
        }

        protected void InitLog()
        {
            string logFilePath = @"..\log\" + Model.GetSetting("logFilePath", "DynamicLOD_ResetEdition.log");
            string logLevel = Model.GetSetting("logLevel", "Debug"); ;
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration().WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3,
                                                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message} {NewLine}{Exception}");
            if (logLevel == "Warning")
                loggerConfiguration.MinimumLevel.Warning();
            else if (logLevel == "Debug")
                loggerConfiguration.MinimumLevel.Debug();
            else if (logLevel == "Verbose")
                loggerConfiguration.MinimumLevel.Verbose();
            else
                loggerConfiguration.MinimumLevel.Information();
            Log.Logger = loggerConfiguration.CreateLogger();
            Log.Information($"-----------------------------------------------------------------------");
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            assemblyVersion = assemblyVersion[0..assemblyVersion.LastIndexOf('.')];
            Logger.Log(LogLevel.Information, "App:InitLog", $"DynamicLOD_ResetEdition v{assemblyVersion}{(ServiceModel.TestVersion ? ServiceModel.TestVariant : "")} started! Log Level: {logLevel} Log File: {logFilePath}");
        }

        protected void InitSystray()
        {
            Logger.Log(LogLevel.Information, "App:InitSystray", $"Creating SysTray Icon ...");
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            notifyIcon.Icon = GetIcon("icon.ico");
            notifyIcon.ForceCreate(false);
        }

        public static Icon GetIcon(string filename)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DynamicLOD_ResetEdition.{filename}");
            return new Icon(stream);
        }
    }
}
