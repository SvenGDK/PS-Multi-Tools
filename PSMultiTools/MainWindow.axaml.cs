using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.PS1;
using PSMultiTools.PS2;
using PSMultiTools.PS3;
using PSMultiTools.PS4;
using PSMultiTools.PS5;
using PSMultiTools.PSP;
using PSMultiTools.PSV;

namespace PSMultiTools
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PS1Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS1Library = new PS1Library() { ShowActivated = true };
            NewPS1Library.Show();
        }

        private void PS2Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS2Library = new PS2Library() { ShowActivated = true };
            NewPS2Library.Show();
        }

        private async void PSXImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            //if (Utils.IsRunningAsAdministratorOrRoot())
            //{
            //    var NewPSXProjectManager = new PSXMainWindow() { ShowActivated = true };
            //    NewPSXProjectManager.Show();
            //}
            //else
            //{
            //    var box = MessageBoxManager.GetMessageBoxStandard("Elevation required", "This feature requires Administrator/Root permissions." + Environment.NewLine + "Do you want to restart PS Multi Tools as Administrator/Root ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            //    var boxresult = await box.ShowWindowDialogAsync(this);
            //    if (boxresult == ButtonResult.Yes)
            //    {
            //        Utils.RunAsAdministrator();
            //    }
            //}
        }

        private void PS3Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS3Library = new PS3Library() { ShowActivated = true };
            NewPS3Library.Show();
        }

        private void PS4Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS4Library = new PS4Library() { ShowActivated = true };
            NewPS4Library.Show();
        }

        private void PS5Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS5Library = new PS5Library() { ShowActivated = true };
            NewPS5Library.Show();
        }

        private void PSPImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPSPLibrary = new PSPLibrary() { ShowActivated = true };
            NewPSPLibrary.Show();
        }
        private void PSVImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPSVLibrary = new PSVLibrary() { ShowActivated = true };
            NewPSVLibrary.Show();
        }

        private void PS1TextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS1Library = new PS1Library() { ShowActivated = true };
            NewPS1Library.Show();
        }

        private void PS2TextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS2Library = new PS2Library() { ShowActivated = true };
            NewPS2Library.Show();
        }

        private async void PSXTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            //if (Utils.IsRunningAsAdministratorOrRoot())
            //{
            //    var NewPSXProjectManager = new PSXMainWindow() { ShowActivated = true };
            //    NewPSXProjectManager.Show();
            //}
            //else
            //{
            //    var box = MessageBoxManager.GetMessageBoxStandard("Elevation required", "This feature requires Administrator/Root permissions." + Environment.NewLine + "Do you want to restart PS Multi Tools as Administrator/Root ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            //    var boxresult = await box.ShowWindowDialogAsync(this);
            //    if (boxresult == ButtonResult.Yes)
            //    {
            //        Utils.RunAsAdministrator();
            //    }
            //}
        }

        private void PS3TextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS3Library = new PS3Library() { ShowActivated = true };
            NewPS3Library.Show();
        }

        private void PS4TextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS4Library = new PS4Library() { ShowActivated = true };
            NewPS4Library.Show();
        }

        private void PS5TextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPS5Library = new PS5Library() { ShowActivated = true };
            NewPS5Library.Show();
        }

        private void PSPTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPSPLibrary = new PSPLibrary() { ShowActivated = true };
            NewPSPLibrary.Show();
        }

        private void PSVTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var NewPSVLibrary = new PSVLibrary() { ShowActivated = true };
            NewPSVLibrary.Show();
        }

        private async void UpdateImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (await Utils.IsPSMultiToolsUpdateAvailable())
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Update found", "An update is available, do you want to download it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Yes)
                {
                    Utils.DownloadAndExecuteUpdater();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No update found", "PS Multi Tools is up to date!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }

        private async void UpdateTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (await Utils.IsPSMultiToolsUpdateAvailable())
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Update found", "An update is available, do you want to download it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Yes)
                {
                    Utils.DownloadAndExecuteUpdater();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No update found", "PS Multi Tools is up to date!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }

    }
}