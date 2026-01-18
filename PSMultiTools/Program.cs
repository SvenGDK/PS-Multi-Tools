using Avalonia;
using System;
using System.IO;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

namespace PSMultiTools
{
    internal class Program
    {
        public static string CachePath = Path.Combine(Path.GetTempPath(), "CefGlue_" + Guid.NewGuid().ToString().Replace("-", null));

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanupOnProcessExit();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                CleanupOnProcessExit();
                Environment.Exit(0);
            };

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
            .UsePlatformDetect() // Change to UseX11() for FreeBSD and publish self-contained
            .UseSkia()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings
            {
                RootCachePath = CachePath,
                WindowlessRenderingEnabled = false
            }));

        private static void CleanupOnProcessExit()
        {
            try
            {
                CefRuntime.Shutdown();
            }
            catch { }

            try
            {
                var dirInfo = new DirectoryInfo(CachePath);
                if (dirInfo.Exists)
                    dirInfo.Delete(true);
            }
            catch { }
        }
    }
}
