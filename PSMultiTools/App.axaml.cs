using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace PSMultiTools
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Args != null && desktop.Args.Length > 0)
                {
                    string[] args = desktop.Args;

                    // Single argument
                    if (args.Length == 1)
                    {

                        var firstArg = desktop.Args[0].Trim('"');
                        if (firstArg.StartsWith("file://", System.StringComparison.OrdinalIgnoreCase)) { try { firstArg = new System.Uri(firstArg).LocalPath; } catch { } } // Get file path in case of Uri

                        var FileExtension = System.IO.Path.GetExtension(firstArg)?.ToLowerInvariant();
                        var FileName = System.IO.Path.GetFileName(firstArg)?.ToLowerInvariant();

                        switch (FileExtension)
                        {
                            case ".pkg":
                                desktop.MainWindow = new MultiPlatformTools.PKGInfo() { SelectedPKG = args[0] };
                                break;
                            case ".json":
                                if (FileName == "param.json")
                                {
                                    desktop.MainWindow = new PS5.Tools.Editors.PS5ParamEditor() { ReceivedParamFile = args[0] };
                                }
                                else if (FileName == "manifest.json")
                                {
                                    desktop.MainWindow = new PS5.Tools.Editors.PS5ManifestEditor() { ReceivedManifestFile = args[0] };
                                }
                                break;
                            case ".elf" or ".bin":
                                desktop.MainWindow = new PS5.Tools.PS5Sender { ReceivedPayload = args[0] };
                                break;
                        }
                    }
                    else
                    {
                        // More to do
                    }
                }
                else
                {
                    desktop.MainWindow = new MainWindow();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}