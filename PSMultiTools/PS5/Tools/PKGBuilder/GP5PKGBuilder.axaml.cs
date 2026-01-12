using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using IniParser;
using IniParser.Model;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using static PSMultiTools.Classes.PS5ManifestClass;
using static PSMultiTools.Classes.PS5ParamClass;
using static PSMultiTools.Classes.Structures;

namespace PSMultiTools.PS5.Tools.PKGBuilder;

public partial class GP5PKGBuilder : Window
{

    private PKGBuilderProject CurrentProject;
    private string CurrentProjectName = "";

    public string PKGBuilderOuput = "";
    public string SavedBGPath = "";
    public string SavedIconPath = "";

    private readonly DispatcherTimer ClockTimer = new() { Interval = new TimeSpan(0, 1, 0) };

    public GP5PKGBuilder()
    {
        InitializeComponent();

        Loaded += GP5PKGBuilder_Loaded;

        ClockTimer.Tick += ClockTimer_Tick;

        GamesTextBlock.PointerPressed += GamesTextBlock_PointerPressed;
        MediaTextBlock.PointerPressed += MediaTextBlock_PointerPressed;
        MainIconImageRectangle.PointerPressed += MainIconImageRectangle_PointerPressed;
        BackgroundImage.PointerPressed += BackgroundImage_PointerPressed;
        TitleTextBlock.PointerPressed += TitleTextBlock_PointerPressed;

        EnableHTTPCacheCheckBox.IsCheckedChanged += EnableHTTPCacheCheckBox_Checked;
        TwinTurboCheckBox.IsCheckedChanged += TwinTurboCheckBox_Checked;
    }

    private async void GP5PKGBuilder_Loaded(object? sender, RoutedEventArgs e)
    {
        GamesTextBlock.Transitions = [new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = new CubicEaseOut() }];
        MediaTextBlock.Transitions = [new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = new CubicEaseOut() }];
        MainIconImageRectangle.Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(500), Easing = new CubicEaseOut() }];
        BackgroundImage.Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(500), Easing = new CubicEaseOut() }];

        ClockTextBlock.Text = DateTime.Now.ToString("HH:mm");
        ClockTimer.Start();

        if (!OperatingSystem.IsWindows())
        {
            //  Check if a wine prefix exists
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c")))
            {
                var WineNotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine installation not complete", "A wine prefix will be created, please close the Wine Configuration Tool when the initialization finished.", ButtonEnum.Ok);
                await WineNotInstalledMessage.ShowAsync();

                // Check if winetricks is updated if previously installed
                Process BashProcess = new();
                BashProcess.StartInfo.FileName = OperatingSystem.IsLinux() ? "/bin/bash" : "/bin/sh";
                BashProcess.StartInfo.Arguments = $"-c \"winecfg\"";
                BashProcess.StartInfo.RedirectStandardOutput = true;
                BashProcess.StartInfo.RedirectStandardError = true;
                BashProcess.StartInfo.UseShellExecute = false;
                BashProcess.StartInfo.CreateNoWindow = false;
                BashProcess.Start();
                BashProcess.WaitForExit();
            }

            // Check if wine prefix is 64bit
            if (!Directory.Exists(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "windows", "syswow64"))))
            {
                var Wine64NotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine prefix mismatch", "Current default wine prefix is 32bit only, please change to 64bit mode before continuing.", ButtonEnum.Ok);
                await Wine64NotInstalledMessage.ShowAsync();
            }
        }
    }

    private async void CreateNewProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder to save your project" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {

            var NewInputDialog = new InputDialog() { Title = "Title required" };
            NewInputDialog.NewValueTextBox.Text = "No Title";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a title for your new PKG project :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewProjectTitle = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewProjectTitle))
            {

                // Create a new project
                var NewProject = new PKGBuilderProject()
                {
                    ProjectBackground = null,
                    ProjectCategory = "Game",
                    ProjectIcon = null,
                    ProjectSoundtrack = "",
                    ProjectTitle = NewProjectTitle,
                    ProjectURL = "",
                    ProjectPath = FBDResult,
                    GP5Created = false
                };

                // Create the sce_sys directory
                Directory.CreateDirectory(Path.Combine(FBDResult, "sce_sys"));

                // Create a new param.json file inside the sce_sys directory
                CreateParam(Path.Combine(FBDResult, "sce_sys", "param.json"));
                ChangeParam(Path.Combine(FBDResult, "sce_sys", "param.json"), "Title", NewProjectTitle);

                // Create a new manifest.json file
                CreateManifest(Path.Combine(FBDResult, "manifest.json"));

                // Save project configuration file
                var ProjectConfigParser = new FileIniDataParser();
                IniData ProjectConfigData = new();

                ProjectConfigData["PKGProject"]["Title"] = NewProjectTitle;
                ProjectConfigData["PKGProject"]["Background"] = "";
                ProjectConfigData["PKGProject"]["Category"] = "Game";
                ProjectConfigData["PKGProject"]["Icon"] = "";
                ProjectConfigData["PKGProject"]["Soundtrack"] = "";
                ProjectConfigData["PKGProject"]["URL"] = "";
                ProjectConfigData["PKGProject"]["Path"] = FBDResult;
                ProjectConfigData["PKGProject"]["GP5Created"] = "False";

                ProjectConfigParser.WriteFile(Path.Combine(FBDResult, NewProjectTitle + ".ini"), ProjectConfigData);

                // Enable controls
                CurrentProjectTextBlock.Text = Path.Combine(FBDResult, NewProjectTitle + ".ini");
                SaveProjectButton.IsEnabled = true;
                FinalizeButton.IsEnabled = true;
                SetBackgroundButton.IsEnabled = true;
                SetSoundtrackButton.IsEnabled = true;
                SetURLButton.IsEnabled = true;
                SetContentIDButton.IsEnabled = true;
                SetTitleIDButton.IsEnabled = true;
                SetVersionButton.IsEnabled = true;
                GamesTextBlock.IsEnabled = true;
                MediaTextBlock.IsEnabled = true;
                TitleTextBlock.IsEnabled = true;
                MainIconImageRectangle.IsEnabled = true;
                BackgroundImage.IsEnabled = true;
                SetReqSysVersionButton.IsEnabled = true;
                EnableHTTPCacheCheckBox.IsEnabled = true;
                TwinTurboCheckBox.IsEnabled = true;

                CurrentProject = NewProject;
                TitleTextBlock.Text = NewProjectTitle;
            }

        }
    }

    private async void LoadProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var iniFileFilter = new FileDialogFilter
        {
            Name = "INI Project File",
            Extensions = ["ini"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a saved PKG project", Filters = { iniFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {

            CurrentProjectTextBlock.Text = OFDResult[0];

            var ProjectInfos = new PKGBuilderProject();
            var ProjectConfigParser = new FileIniDataParser();
            IniData ProjectConfigData = ProjectConfigParser.ReadFile(OFDResult[0]);

            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["Title"]))
            {
                ProjectInfos.ProjectTitle = ProjectConfigData["PKGProject"]["Title"];
                TitleTextBlock.Text = ProjectConfigData["PKGProject"]["Title"];
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["Background"]))
            {
                var TempBitmapImage = AnyBitmap.FromFile(ProjectConfigData["PKGProject"]["Background"]);
                ProjectInfos.ProjectBackground = TempBitmapImage;
                BackgroundImage.Source = Utils.AnyBitmapToIImage(TempBitmapImage);
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["Category"]))
            {
                ProjectInfos.ProjectCategory = ProjectConfigData["PKGProject"]["Category"];
                if (!(ProjectConfigData["PKGProject"]["Category"] == "Game"))
                {
                    MediaTextBlock.FontWeight = FontWeight.Bold;
                    GamesTextBlock.FontWeight = FontWeight.Regular;

                    if (MediaTextBlock.Margin.Left == 175d)
                    {
                        MediaTextBlock.Margin = new Thickness(55, 305, 0, 0);
                        GamesTextBlock.Margin = new Thickness(175, 305, 0, 0);
                    }

                    PlayButtonTextBlock.Text = "Launch";
                }
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["Icon"]))
            {
                var TempBitmapImage = AnyBitmap.FromFile(ProjectConfigData["PKGProject"]["Icon"]);
                ProjectInfos.ProjectIcon = TempBitmapImage;

                MainIconImageRectangle.Fill = Utils.AnyBitmapToImageBrush(TempBitmapImage);
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["Soundtrack"]))
            {
                ProjectInfos.ProjectSoundtrack = ProjectConfigData["PKGProject"]["Soundtrack"];
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["URL"]))
            {
                ProjectInfos.ProjectURL = ProjectConfigData["PKGProject"]["URL"];
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["Path"]))
            {
                ProjectInfos.ProjectPath = ProjectConfigData["PKGProject"]["Path"];
            }
            if (!string.IsNullOrEmpty(ProjectConfigData["PKGProject"]["GP5Created"]))
            {
                if (ProjectConfigData["PKGProject"]["GP5Created"] == "True")
                {
                    if (File.Exists(ProjectInfos.ProjectPath + @"\" + ProjectInfos.ProjectTitle + ".gp5"))
                    {
                        ProjectInfos.GP5Created = true;
                        BuildProjectButton.IsEnabled = true;
                    }
                }
                else
                {
                    ProjectInfos.GP5Created = false;
                }
            }

            if (!string.IsNullOrEmpty(ProjectInfos.ProjectPath))
            {
                SaveProjectButton.IsEnabled = true;
                FinalizeButton.IsEnabled = true;
                SetBackgroundButton.IsEnabled = true;
                SetSoundtrackButton.IsEnabled = true;
                SetURLButton.IsEnabled = true;
                SetContentIDButton.IsEnabled = true;
                SetTitleIDButton.IsEnabled = true;
                SetVersionButton.IsEnabled = true;
                GamesTextBlock.IsEnabled = true;
                MediaTextBlock.IsEnabled = true;
                TitleTextBlock.IsEnabled = true;
                MainIconImageRectangle.IsEnabled = true;
                BackgroundImage.IsEnabled = true;
                SetReqSysVersionButton.IsEnabled = true;
                EnableHTTPCacheCheckBox.IsEnabled = true;
                TwinTurboCheckBox.IsEnabled = true;

                CurrentProject = ProjectInfos;
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error reading project INI", "Wrong INI format.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            }

        }
    }

    private async void SaveProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var FBD = new OpenFolderDialog() { Title = "Select a save path for your project" };
            var FBDResult = await FBD.ShowAsync(this);
            if (FBDResult != null)
            {
                // Save project settings
                var ProjectConfigParser = new FileIniDataParser();
                IniData ProjectConfigData = ProjectConfigParser.ReadFile(Path.Combine(FBDResult, CurrentProject.ProjectTitle + ".ini"));

                ProjectConfigData["PKGProject"]["Title"] = CurrentProject.ProjectTitle;
                ProjectConfigData["PKGProject"]["Background"] = SavedBGPath;
                ProjectConfigData["PKGProject"]["Category"] = CurrentProject.ProjectCategory;
                ProjectConfigData["PKGProject"]["Icon"] = SavedIconPath;
                ProjectConfigData["PKGProject"]["Soundtrack"] = CurrentProject.ProjectSoundtrack;
                ProjectConfigData["PKGProject"]["URL"] = CurrentProject.ProjectURL;
                ProjectConfigData["PKGProject"]["Path"] = FBDResult;
                ProjectConfigData["PKGProject"]["GP5Created"] = "False";

                ProjectConfigParser.WriteFile(Path.Combine(FBDResult, CurrentProject.ProjectTitle + ".ini"), ProjectConfigData);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    #region PointerPressedEvents

    private async void TitleTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var NewInputDialog = new InputDialog() { Title = "Set Title" };
            NewInputDialog.NewValueTextBox.Text = "No Title";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new title :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewTitle = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewTitle))
            {
                // Save changes in param.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
                {
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "Title", NewValue: NewTitle);
                }
                else
                {
                    CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "Title", NewValue: NewTitle);
                }

                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "ApplicationName", NewValue: NewTitle);
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "ApplicationName", NewValue: NewTitle);
                }

                TitleTextBlock.Text = NewTitle;
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Title changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void BackgroundImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new background image", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                // Show new background
                Dispatcher.UIThread.Invoke(() =>
                {
                    var TempBitmapImage = AnyBitmap.FromFile(OFDResult[0]);
                    CurrentProject.ProjectBackground = TempBitmapImage;

                    BackgroundImage.Opacity = 0;
                    BackgroundImage.Source = Utils.AnyBitmapToIImage(TempBitmapImage);
                    BackgroundImage.Opacity = 1;
                });

                // Copy file to project folder
                File.Copy(OFDResult[0], Path.Combine(CurrentProject.ProjectPath, "sce_sys", "pic0.png"), true);

                // Save project configuration file with new path
                if (!string.IsNullOrEmpty(CurrentProjectTextBlock.Text))
                {
                    var ProjectConfigParser = new FileIniDataParser();
                    IniData ProjectConfigData = ProjectConfigParser.ReadFile(CurrentProjectTextBlock.Text);
                    ProjectConfigData["PKGProject"]["Background"] = Path.Combine(CurrentProject.ProjectPath, "sce_sys", "pic0.png");
                    ProjectConfigParser.WriteFile(CurrentProjectTextBlock.Text, ProjectConfigData);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Background changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void MainIconImageRectangle_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new icon", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                // Show new icon
                Dispatcher.UIThread.Invoke(() =>
                {
                    var TempBitmapImage = AnyBitmap.FromFile(OFDResult[0]);
                    CurrentProject.ProjectIcon = TempBitmapImage;
                    MainIconImageRectangle.Opacity = 0;
                    MainIconImageRectangle.Fill = Utils.AnyBitmapToImageBrush(TempBitmapImage);
                    MainIconImageRectangle.Opacity = 1;
                });

                // Copy file to project folder
                File.Copy(OFDResult[0], Path.Combine(CurrentProject.ProjectPath, "sce_sys", "icon0.png"), true);

                // Save project configuration file with new path
                if (!string.IsNullOrEmpty(CurrentProjectTextBlock.Text))
                {
                    var ProjectConfigParser = new FileIniDataParser();
                    IniData ProjectConfigData = ProjectConfigParser.ReadFile(CurrentProjectTextBlock.Text);
                    ProjectConfigData["PKGProject"]["Icon"] = Path.Combine(CurrentProject.ProjectPath, "sce_sys", "icon0.png");
                    ProjectConfigParser.WriteFile(CurrentProjectTextBlock.Text, ProjectConfigData);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Icon changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void MediaTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
            {
                ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "Category", NewValue: "Media");
            }
            else
            {
                CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "Category", NewValue: "Media");
            }

            MediaTextBlock.FontWeight = FontWeight.Bold;
            GamesTextBlock.FontWeight = FontWeight.Regular;

            if (MediaTextBlock.Margin.Left == 180)
            {
                MediaTextBlock.Margin = new Thickness(60, 365, 0, 0);
                GamesTextBlock.Margin = new Thickness(180, 365, 0, 0);
            }
            else
            {
                MediaTextBlock.Margin = new Thickness(180, 365, 0, 0);
                GamesTextBlock.Margin = new Thickness(60, 365, 0, 0);
            }

            PlayButtonTextBlock.Text = "Launch";

            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Category changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowDialogAsync(this);
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void GamesTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
            {
                ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "Category", NewValue: "Game");
            }
            else
            {
                CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "Category", NewValue: "Game");
            }

            MediaTextBlock.FontWeight = FontWeight.Regular;
            GamesTextBlock.FontWeight = FontWeight.Bold;

            if (GamesTextBlock.Margin.Left == 60)
            {
                GamesTextBlock.Margin = new Thickness(180, 365, 0, 0);
                MediaTextBlock.Margin = new Thickness(60, 365, 0, 0);
            }
            else
            {
                GamesTextBlock.Margin = new Thickness(60, 365, 0, 0);
                MediaTextBlock.Margin = new Thickness(180, 365, 0, 0);
            }

            PlayButtonTextBlock.Text = "Play Game";

            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Category changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowDialogAsync(this);
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    #endregion

    #region ButtonClicks

    private async void SetBackgroundButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new background image", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                // Show new background
                Dispatcher.UIThread.Invoke(() =>
                {
                    var TempBitmapImage = AnyBitmap.FromFile(OFDResult[0]);
                    CurrentProject.ProjectBackground = TempBitmapImage;

                    BackgroundImage.Opacity = 0;
                    BackgroundImage.Source = Utils.AnyBitmapToIImage(TempBitmapImage);
                    BackgroundImage.Opacity = 1;
                });

                // Copy file to project folder
                File.Copy(OFDResult[0], Path.Combine(CurrentProject.ProjectPath, "sce_sys", "pic0.png"), true);

                // Save project configuration file with new path
                if (!string.IsNullOrEmpty(CurrentProjectTextBlock.Text))
                {
                    var ProjectConfigParser = new FileIniDataParser();
                    IniData ProjectConfigData = ProjectConfigParser.ReadFile(CurrentProjectTextBlock.Text);
                    ProjectConfigData["PKGProject"]["Background"] = Path.Combine(CurrentProject.ProjectPath, "sce_sys", "pic0.png");
                    ProjectConfigParser.WriteFile(CurrentProjectTextBlock.Text, ProjectConfigData);
                }

                SavedBGPath = OFDResult[0];

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Background changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void SetSoundtrackButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var at9FileFilter = new FileDialogFilter
            {
                Name = "AT9 File",
                Extensions = ["at9"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new sountrack", Filters = { at9FileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                // Copy file to project folder
                File.Copy(OFDResult[0], Path.Combine(CurrentProject.ProjectPath, "sce_sys", "snd0.at9"), true);

                // Save project configuration file
                if (!string.IsNullOrEmpty(CurrentProjectTextBlock.Text))
                {
                    var ProjectConfigParser = new FileIniDataParser();
                    IniData ProjectConfigData = ProjectConfigParser.ReadFile(CurrentProjectTextBlock.Text);
                    ProjectConfigData["PKGProject"]["Soundtrack"] = Path.Combine(CurrentProject.ProjectPath, "sce_sys", "snd0.at9");
                    ProjectConfigParser.WriteFile(CurrentProjectTextBlock.Text, ProjectConfigData);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Soundtrack changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void SetURLButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var NewInputDialog = new InputDialog() { Title = "Set URL" };
            NewInputDialog.NewValueTextBox.Text = "";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter an action or website URL :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewURL = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewURL))
            {

                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
                {
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "URL", NewValue: NewURL);
                }
                else
                {
                    CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "URL", NewValue: NewURL);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "URL changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void SetContentIDButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var NewInputDialog = new InputDialog() { Title = "Set Content ID" };
            NewInputDialog.NewValueTextBox.Text = "IV9999-NPXS12345_00-XXXXXXXXXXXXXXXX";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new Content ID :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewContentID = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewContentID))
            {

                // Save changes in param.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
                {
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "ContentID", NewValue: NewContentID);
                }
                else
                {
                    CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "ContentID", NewValue: NewContentID);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Content ID changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void SetTitleIDButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var NewInputDialog = new InputDialog() { Title = "Set Title ID" };
            NewInputDialog.NewValueTextBox.Text = "NPXS12345";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new Title ID :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewTitleID = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewTitleID))
            {

                // Save changes in param.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
                {
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "TitleID", NewValue: NewTitleID);
                }
                else
                {
                    CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "TitleID", NewValue: NewTitleID);
                }

                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "TitleID", NewValue: NewTitleID);
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "TitleID", NewValue: NewTitleID);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Title ID changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void SetVersionButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var NewInputDialog = new InputDialog() { Title = "Set Version" };
            NewInputDialog.NewValueTextBox.Text = "01.000.000";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new version like the default value :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewVersion = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewVersion))
            {

                string[] SplittedValues = NewVersion.Split('.');
                string ApplicationMajorVersion = "";
                string ApplicationMinorVersion = "";
                string ApplicationPatchVersion = "";

                if (SplittedValues[0].StartsWith("0"))
                {
                    ApplicationMajorVersion = SplittedValues[0][1..];
                }
                if (SplittedValues[0] == "00")
                {
                    ApplicationMajorVersion = SplittedValues[0].Replace("00", "0");
                }
                if (string.IsNullOrEmpty(ApplicationMajorVersion))
                {
                    ApplicationMajorVersion = SplittedValues[0];
                }

                if (SplittedValues[1] == "000")
                {
                    ApplicationMinorVersion = SplittedValues[1].Replace("000", "0");
                }
                if (SplittedValues[1].Contains("00"))
                {
                    ApplicationMinorVersion = SplittedValues[1].Replace("00", "");
                }
                if (string.IsNullOrEmpty(ApplicationMinorVersion))
                {
                    ApplicationMinorVersion = SplittedValues[1];
                }

                if (SplittedValues[2] == "000")
                {
                    ApplicationPatchVersion = SplittedValues[2].Replace("000", "0");
                }
                if (SplittedValues[2].Contains("00"))
                {
                    ApplicationPatchVersion = SplittedValues[2].Replace("00", "");
                }
                if (string.IsNullOrEmpty(ApplicationPatchVersion))
                {
                    ApplicationPatchVersion = SplittedValues[2];
                }

                string ApplicationVersion = ApplicationMajorVersion + "." + ApplicationMinorVersion + "." + ApplicationPatchVersion + "+000";
                string ReactNativePlaystationVersion = ApplicationMajorVersion + "." + ApplicationMinorVersion + "." + ApplicationPatchVersion + "-000.0";
                string MasterVersion = SplittedValues[0] + "." + SplittedValues[1][1..];

                // Save changes in param.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
                {
                    if (!string.IsNullOrEmpty(NewVersion) && !string.IsNullOrEmpty(MasterVersion))
                    {
                        ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "ContentVersion", NewValue: NewVersion);
                        ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "MasterVersion", NewValue: MasterVersion);
                    }
                }
                else
                {
                    CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));

                    if (!string.IsNullOrEmpty(NewVersion) && !string.IsNullOrEmpty(MasterVersion))
                    {
                        ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "ContentVersion", NewValue: NewVersion);
                        ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "MasterVersion", NewValue: MasterVersion);
                    }
                }

                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    if (!string.IsNullOrEmpty(ApplicationVersion))
                    {
                        ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "ApplicationVersion", NewValue: ApplicationVersion);
                        ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "ReactNativePlaystationVersion", NewValue: ReactNativePlaystationVersion);
                    }
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));

                    if (!string.IsNullOrEmpty(ApplicationVersion))
                    {
                        ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "ApplicationVersion", NewValue: ApplicationVersion);
                        ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "ReactNativePlaystationVersion", NewValue: ReactNativePlaystationVersion);
                    }
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Version changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void SetReqSysVersionButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var NewInputDialog = new InputDialog() { Title = "Set Required System Version" };
            NewInputDialog.NewValueTextBox.Text = "0x0000000000000000";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the required System Version :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string NewRequiredSystemVersion = await NewInputDialog.ShowDialog<string>(this);
            if (!string.IsNullOrEmpty(NewRequiredSystemVersion))
            {
                // Save changes in param.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json")))
                {
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "RequiredSystemSoftwareVersion", NewValue: NewRequiredSystemVersion);
                }
                else
                {
                    CreateParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"));
                    ChangeParam(Path.Combine(CurrentProject.ProjectPath, "sce_sys", "param.json"), "RequiredSystemSoftwareVersion", NewValue: NewRequiredSystemVersion);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Required System Version changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    #endregion

    private void EnableHTTPCacheCheckBox_Checked(object? sender, RoutedEventArgs e)
    {
        if (EnableHTTPCacheCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
            {
                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "EnableHttpCache", NewBoolValue: true);
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "EnableHttpCache", NewBoolValue: true);
                }
            }
            else
            {
                NoProjectLoadedMessage();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
            {
                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "EnableHttpCache", NewBoolValue: false);
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "EnableHttpCache", NewBoolValue: false);
                }
            }
            else
            {
                NoProjectLoadedMessage();
            }
        }
    }

    private void TwinTurboCheckBox_Checked(object? sender, RoutedEventArgs e)
    {
        if (EnableHTTPCacheCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
            {
                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "TwinTurbo", NewBoolValue: true);
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "TwinTurbo", NewBoolValue: true);
                }
            }
            else
            {
                NoProjectLoadedMessage();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(CurrentProject.ProjectPath))
            {
                // Save changes in manifest.json
                if (File.Exists(Path.Combine(CurrentProject.ProjectPath, "manifest.json")))
                {
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "TwinTurbo", NewBoolValue: false);
                }
                else
                {
                    CreateManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"));
                    ChangeManifest(Path.Combine(CurrentProject.ProjectPath, "manifest.json"), "TwinTurbo", NewBoolValue: false);
                }
            }
            else
            {
                NoProjectLoadedMessage();
            }
        }
    }

    private async void FinalizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath) && !string.IsNullOrEmpty(CurrentProjectTextBlock.Text))
        {
            if (await FinalizeProjectAsync(CurrentProjectTextBlock.Text) == true)
                await MessageBoxManager.GetMessageBoxStandard("Success", "Project finalized! You can build the PKG now.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info).ShowWindowDialogAsync(this);
        }
        else
        {
            NoProjectLoadedMessage();
        }
    }

    private async void BuildProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentProject.ProjectPath) && !string.IsNullOrEmpty(CurrentProjectTextBlock.Text))
        {
            var pkgFileFilter = new FileDialogFilter
            {
                Name = "PKG File",
                Extensions = ["pkg"]
            };
            var SFD = new SaveFileDialog() { Title = "Select a save path for the PKG file", DefaultExtension = ".pkg", Filters = { pkgFileFilter } };
            var SFDResult = await SFD.ShowAsync(this);
            if (SFDResult != null)
            {
                string PKGDestinationPath = SFDResult;
                string GP5ProjectPath = Path.Combine(CurrentProject.ProjectPath, CurrentProject.ProjectTitle + ".gp5");

                // Disable buttons
                SaveProjectButton.IsEnabled = false;
                FinalizeButton.IsEnabled = false;
                BuildProjectButton.IsEnabled = false;
                SetBackgroundButton.IsEnabled = false;
                SetSoundtrackButton.IsEnabled = false;
                SetURLButton.IsEnabled = false;
                SetContentIDButton.IsEnabled = false;
                SetTitleIDButton.IsEnabled = false;
                SetVersionButton.IsEnabled = false;
                SetReqSysVersionButton.IsEnabled = false;
                EnableHTTPCacheCheckBox.IsEnabled = false;
                TwinTurboCheckBox.IsEnabled = false;

                GamesTextBlock.IsEnabled = false;
                MediaTextBlock.IsEnabled = false;
                TitleTextBlock.IsEnabled = false;
                MainIconImageRectangle.IsEnabled = false;
                BackgroundImage.IsEnabled = false;

                BuildPKG(GP5ProjectPath, PKGDestinationPath);
            }

        }
    }

    private static void CreateParam(string DestinationPath)
    {
        var NewPS5Param = new PS5Param()
        {
            AgeLevel = new AgeLevel() { Default = 0, US = 0 },
            ApplicationCategoryType = 0,
            ApplicationDrmType = "free",
            Attribute = 0,
            Attribute2 = 0,
            Attribute3 = 0,
            ConceptId = "99999",
            ContentBadgeType = 0,
            ContentId = "IV9999-NPXS12345_00-XXXXXXXXXXXXXXXX",
            ContentVersion = "01.000.000",
            DownloadDataSize = 0,
            LocalizedParameters = new LocalizedParameters()
            {
                DefaultLanguage = "en-US",
                EnUS = new EnUS() { TitleName = "Title Name" },
                ArAE = new ArAE() { TitleName = "Title Name" },
                CsCZ = new CsCZ() { TitleName = "Title Name" },
                DaDK = new DaDK() { TitleName = "Title Name" },
                DeDE = new DeDE() { TitleName = "Title Name" },
                ElGR = new ElGR() { TitleName = "Title Name" },
                EnGB = new EnGB() { TitleName = "Title Name" },
                Es419 = new Es419() { TitleName = "Title Name" },
                EsES = new EsES() { TitleName = "Title Name" },
                FiFI = new FiFI() { TitleName = "Title Name" },
                FrCA = new FrCA() { TitleName = "Title Name" },
                FrFR = new FrFR() { TitleName = "Title Name" },
                HuHU = new HuHU() { TitleName = "Title Name" },
                IdID = new IdID() { TitleName = "Title Name" },
                ItIT = new ItIT() { TitleName = "Title Name" },
                JaJP = new JaJP() { TitleName = "Title Name" },
                KoKR = new KoKR() { TitleName = "Title Name" },
                NlNL = new NlNL() { TitleName = "Title Name" },
                NoNO = new NoNO() { TitleName = "Title Name" },
                PlPL = new PlPL() { TitleName = "Title Name" },
                PtBR = new PtBR() { TitleName = "Title Name" },
                PtPT = new PtPT() { TitleName = "Title Name" },
                RoRO = new RoRO() { TitleName = "Title Name" },
                RuRU = new RuRU() { TitleName = "Title Name" },
                SvSE = new SvSE() { TitleName = "Title Name" },
                ThTH = new ThTH() { TitleName = "Title Name" },
                TrTR = new TrTR() { TitleName = "Title Name" },
                ViVN = new ViVN() { TitleName = "Title Name" },
                ZhHans = new ZhHans() { TitleName = "Title Name" },
                ZhHant = new ZhHant() { TitleName = "Title Name" }
            },
            MasterVersion = "01.00",
            TitleId = "NPXS12345"
        };

        string RawDataJSON = JsonConvert.SerializeObject(NewPS5Param, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText(DestinationPath, RawDataJSON);
    }

    private async void ChangeParam(string DestinationPath, string Param, string NewValue)
    {
        string JSONData = File.ReadAllText(DestinationPath);
        try
        {
            // Read values
            var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);

            // Change value
            if (ParamData != null)
            {
                switch (Param ?? "")
                {
                    case "Category":
                        {
                            if (NewValue == "Media")
                            {
                                ParamData.ApplicationCategoryType = 65536;
                            }
                            else
                            {
                                ParamData.ApplicationCategoryType = 0;
                            }

                            break;
                        }
                    case "ContentID":
                        {
                            ParamData.ContentId = NewValue;
                            break;
                        }
                    case "ContentVersion":
                        {
                            ParamData.ContentVersion = NewValue;
                            break;
                        }
                    case "Title":
                        {
                            ParamData.LocalizedParameters = new LocalizedParameters()
                            {
                                DefaultLanguage = "en-US",
                                EnUS = new EnUS() { TitleName = NewValue },
                                ArAE = new ArAE() { TitleName = NewValue },
                                CsCZ = new CsCZ() { TitleName = NewValue },
                                DaDK = new DaDK() { TitleName = NewValue },
                                DeDE = new DeDE() { TitleName = NewValue },
                                ElGR = new ElGR() { TitleName = NewValue },
                                EnGB = new EnGB() { TitleName = NewValue },
                                Es419 = new Es419() { TitleName = NewValue },
                                EsES = new EsES() { TitleName = NewValue },
                                FiFI = new FiFI() { TitleName = NewValue },
                                FrCA = new FrCA() { TitleName = NewValue },
                                FrFR = new FrFR() { TitleName = NewValue },
                                HuHU = new HuHU() { TitleName = NewValue },
                                IdID = new IdID() { TitleName = NewValue },
                                ItIT = new ItIT() { TitleName = NewValue },
                                JaJP = new JaJP() { TitleName = NewValue },
                                KoKR = new KoKR() { TitleName = NewValue },
                                NlNL = new NlNL() { TitleName = NewValue },
                                NoNO = new NoNO() { TitleName = NewValue },
                                PlPL = new PlPL() { TitleName = NewValue },
                                PtBR = new PtBR() { TitleName = NewValue },
                                PtPT = new PtPT() { TitleName = NewValue },
                                RoRO = new RoRO() { TitleName = NewValue },
                                RuRU = new RuRU() { TitleName = NewValue },
                                SvSE = new SvSE() { TitleName = NewValue },
                                ThTH = new ThTH() { TitleName = NewValue },
                                TrTR = new TrTR() { TitleName = NewValue },
                                ViVN = new ViVN() { TitleName = NewValue },
                                ZhHans = new ZhHans() { TitleName = NewValue },
                                ZhHant = new ZhHant() { TitleName = NewValue }
                            };
                            break;
                        }
                    case "TitleID":
                        {
                            ParamData.TitleId = NewValue;
                            break;
                        }
                    case "MasterVersion":
                        {
                            ParamData.MasterVersion = NewValue;
                            break;
                        }
                    case "TitleId":
                        {
                            ParamData.TitleId = NewValue;
                            break;
                        }
                    case "URL":
                        {
                            ParamData.DeeplinkUri = NewValue;
                            break;
                        }
                    case "RequiredSystemSoftwareVersion":
                        {
                            ParamData.RequiredSystemSoftwareVersion = NewValue;
                            break;
                        }
                }

                // Write back
                string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                File.WriteAllText(DestinationPath, RawDataJSON);
            }
        }
        catch (JsonSerializationException)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private static void CreateManifest(string DestinationPath)
    {
        var NewPS5Manifest = new PS5Manifest()
        {
            applicationName = "No Title",
            applicationVersion = "0.0.0+000",
            bootAnimation = "default",
            commitHash = "",
            titleId = "NPXS12345",
            repositoryUrl = "",
            reactNativePlaystationVersion = "0.00.0-000.0",
            applicationData = new ApplicationData() { branchType = "release" },
            twinTurbo = false,
            enableHttpCache = false
        };

        string RawDataJSON = JsonConvert.SerializeObject(NewPS5Manifest, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText(DestinationPath, RawDataJSON);
    }

    private async void ChangeManifest(string DestinationPath, string Param, string NewValue = "", bool NewBoolValue = false)
    {
        string JSONData = File.ReadAllText(DestinationPath);
        try
        {
            // Read values
            var ManifestData = JsonConvert.DeserializeObject<PS5Manifest>(JSONData);

            // Set value
            if (ManifestData != null)
            {
                switch (Param ?? "")
                {
                    case "ApplicationName":
                        {
                            ManifestData.applicationName = NewValue;
                            break;
                        }
                    case "ApplicationVersion":
                        {
                            ManifestData.applicationVersion = NewValue;
                            break;
                        }
                    case "TitleID":
                        {
                            ManifestData.titleId = NewValue;
                            break;
                        }
                    case "ReactNativePlaystationVersion":
                        {
                            ManifestData.reactNativePlaystationVersion = NewValue;
                            break;
                        }
                    case "EnableHttpCache":
                        {
                            ManifestData.enableHttpCache = NewBoolValue;
                            break;
                        }
                    case "TwinTurbo":
                        {
                            ManifestData.twinTurbo = NewBoolValue;
                            break;
                        }
                }

                // Write back
                string RawDataJSON = JsonConvert.SerializeObject(ManifestData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                File.WriteAllText(DestinationPath, RawDataJSON);
            }
        }
        catch (JsonSerializationException)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the manifest.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    #region Finalize&Build

    private async Task<bool> FinalizeProjectAsync(string ProjectConfig)
    {
        if (!string.IsNullOrEmpty(ProjectConfig) && !string.IsNullOrEmpty(CurrentProject.ProjectPath))
        {
            var ProjectConfigParser = new FileIniDataParser();
            IniData ProjectConfigData = ProjectConfigParser.ReadFile(ProjectConfig);
            string GP5ProjectPath = Path.Combine(CurrentProject.ProjectPath, CurrentProject.ProjectTitle + ".gp5");

            if (!string.IsNullOrEmpty(CurrentProject.ProjectTitle))
            {
                try
                {
                    string WineCompatiblePath = $"c:\\{CurrentProject.ProjectTitle}"; // The pub tools take only Windows paths

                    // Create the gp5 project
                    var NewGP5Project = new XDocument(
new XElement("psproject", new XAttribute("fmt", "gp5"), new XAttribute("version", "1000"),
new XElement("volume",
new XElement("volume_type", "prospero_app"),
new XElement("package", new XAttribute("passcode", "00000000000000000000000000000000")),
new XElement("chunk_info", new XAttribute("chunk_count", "1"), new XAttribute("scenario_count", "1"),
new XElement("chunks",
new XElement("chunk", new XAttribute("id", "0"), new XAttribute("label", "Chunk #0"))),
new XElement("scenarios", new XAttribute("default_id", "0"),
new XElement("scenario", new XAttribute("id", "0"), new XAttribute("initial_chunk_count", "1"), new XAttribute("label", "Scenario #0"), new XAttribute("type", "playmode"), "0")))),
new XElement("global_exclude"),
new XElement("rootdir", new XAttribute("dir_exclude", "about"), new XAttribute("file_exclude", "*.gp5;*.esbak;keystone;*.dds;disc_info.dat;pfs-version.dat;ext_info.dat"), new XAttribute("src_path", OperatingSystem.IsWindows() ? CurrentProject.ProjectPath : WineCompatiblePath)))
);

                    NewGP5Project.Save(GP5ProjectPath);

                    CurrentProject.GP5Created = true;
                    BuildProjectButton.IsEnabled = true;

                    ProjectConfigData["PKGProject"]["GP5Created"] = "True";
                    ProjectConfigParser.WriteFile(ProjectConfig, ProjectConfigData);

                    CurrentProjectName = CurrentProject.ProjectTitle;

                    return true;
                }
                catch (Exception)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not create the gp5 project.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            NoProjectLoadedMessage();
            return false;
        }
    }

    private async void BuildPKG(string ProjectPath, string DestinationPath)
    {
        if (OperatingSystem.IsWindows())
        {
            Process PKGBuilder = new();
            PKGBuilder.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "prospero-pub-cmd.exe");
            PKGBuilder.StartInfo.Arguments = $"img_create --oformat nwonly \"{ProjectPath}\" \"{DestinationPath}\"";
            PKGBuilder.StartInfo.RedirectStandardOutput = true;
            PKGBuilder.StartInfo.UseShellExecute = false;
            PKGBuilder.StartInfo.CreateNoWindow = true;
            PKGBuilder.EnableRaisingEvents = true;

            PKGBuilder.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    PKGBuilderOuput += e.Data + "\r\n";
                }
            };

            PKGBuilder.Exited += async (s, e) =>
            {
                PKGBuilder.Dispose();

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        // Re-enable buttons
                        EnableButtons();

                        if (PKGBuilderOuput.Contains("Create image Process finished with warning(s)."))
                        {
                            FinishMessage(false);
                        }
                        else
                        {
                            FinishMessage(true);
                        }
                    });
                }
                else
                {
                    // Re-enable buttons
                    EnableButtons();

                    if (PKGBuilderOuput.Contains("Create image Process finished with warning(s)."))
                    {
                        FinishMessage(false);
                    }
                    else
                    {
                        FinishMessage(true);
                    }
                }
            };

            PKGBuilder.Start();
            PKGBuilder.BeginOutputReadLine();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("PKG Builder", "Building a PKG on Linux is supported. However, all files need to be copied first to the Wine's C: drive or it will not work." + Environment.NewLine +
                "Do you want to proceed ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);

            if (boxresult == ButtonResult.Yes)
            {
                // Copy PS5 tools to the wine C:\ drive
                string WinePubToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS5");
                if (!Directory.Exists(WinePubToolsPath))
                {
                    Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5"), WinePubToolsPath);
                }

                // Copy app to the wine C:\ drive
                string ProjectParentPath = Path.GetDirectoryName(ProjectPath)!;
                string DestinationCPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", CurrentProjectName);
                if (!Directory.Exists(DestinationCPath) && ProjectParentPath != null)
                {
                    Utils.CopyFilesRecursively(ProjectParentPath, DestinationCPath);
                }

                var box2 = MessageBoxManager.GetMessageBoxStandard("PKG Builder", "All files are ready & the PKG will will now be created.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box2.ShowWindowDialogAsync(this);

                // Start building
                string PUBCMD = $"wine \"c:\\PS5\\prospero-pub-cmd.exe\" img_create --oformat nwonly \"{ProjectPath}\" \"{DestinationPath}\"";
                var EscapedArgs = PUBCMD.Replace("\"", "\\\"");

                // DEBUG: output everywhere
                Console.WriteLine("Original: " + PUBCMD);
                Console.WriteLine("Escaped: " + EscapedArgs);
                Trace.WriteLine("Original: " + PUBCMD);
                Trace.WriteLine("Escaped: " + EscapedArgs);
                Debug.WriteLine("Original: " + PUBCMD);
                Debug.WriteLine("Escaped: " + EscapedArgs);

                Process PKGBuilder = new();
                PKGBuilder.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
                PKGBuilder.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
                PKGBuilder.StartInfo.RedirectStandardOutput = true;
                PKGBuilder.StartInfo.UseShellExecute = false;
                PKGBuilder.StartInfo.CreateNoWindow = true;
                PKGBuilder.EnableRaisingEvents = true;

                PKGBuilder.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        PKGBuilderOuput += e.Data + "\r\n";
                    }
                };

                PKGBuilder.Exited += async (s, e) =>
                {
                    PKGBuilder.Dispose();

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            // Re-enable buttons
                            EnableButtons();

                            if (PKGBuilderOuput.Contains("Create image Process finished with warning(s)."))
                            {
                                FinishMessage(false);
                            }
                            else
                            {
                                FinishMessage(true);
                            }
                        });
                    }
                    else
                    {
                        // Re-enable buttons
                        EnableButtons();

                        if (PKGBuilderOuput.Contains("Create image Process finished with warning(s)."))
                        {
                            FinishMessage(false);
                        }
                        else
                        {
                            FinishMessage(true);
                        }
                    }
                };

                PKGBuilder.Start();
                PKGBuilder.BeginOutputReadLine();
            }
        }
    }

    private void EnableButtons()
    {
        SaveProjectButton.IsEnabled = true;
        FinalizeButton.IsEnabled = true;
        BuildProjectButton.IsEnabled = true;
        SetBackgroundButton.IsEnabled = true;
        SetSoundtrackButton.IsEnabled = true;
        SetURLButton.IsEnabled = true;
        SetContentIDButton.IsEnabled = true;
        SetTitleIDButton.IsEnabled = true;
        SetVersionButton.IsEnabled = true;
        SetReqSysVersionButton.IsEnabled = true;
        EnableHTTPCacheCheckBox.IsEnabled = true;
        TwinTurboCheckBox.IsEnabled = true;

        GamesTextBlock.IsEnabled = true;
        MediaTextBlock.IsEnabled = true;
        TitleTextBlock.IsEnabled = true;
        MainIconImageRectangle.IsEnabled = true;
        BackgroundImage.IsEnabled = true;
    }

    #endregion

    private void ClockTimer_Tick(object? sender, EventArgs e)
    {
        ClockTextBlock.Text = DateTime.Now.ToString("HH:mm");
    }

    private async void NoProjectLoadedMessage()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Could not load list", "No data available. Please add TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        await box.ShowWindowDialogAsync(this);
    }

    private async void FinishMessage(bool failed)
    {
        if (!failed)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await box.ShowWindowDialogAsync(this);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error while creating the PKG :" + Environment.NewLine + PKGBuilderOuput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

}