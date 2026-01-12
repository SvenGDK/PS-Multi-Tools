using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ImageMagick;
using IronSoftware.Drawing;
using LibVLCSharp.Shared;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using SixLabors.Fonts;
using System;
using System.IO;
using System.Threading.Tasks;
using static PSMultiTools.Classes.Structures;

namespace PSMultiTools.PS5.Tools;

public partial class PS5AssetsBrowser : Window
{

    private LibVLC NewLibVLCPlayer = new();
    private MediaPlayer MediaPlayer { get; }
    private SyncWindow NewLoadingWindow = new() { Title = "Loading asset files", ShowActivated = true };

    private ContextMenu PlayerContextMenu = new();
    private readonly MenuItem ShowHideAssetsMenuItem = new() { Header = "Show assets browser", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Assets-icon.png"))) } };
    private MenuItem PlayPauseMenuItem = new() { Header = "Pause", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/pause.png"))) } };
    private readonly MenuItem StopMenuItem = new() { Header = "Stop playblack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) } };

    public string SelectedDirectory = "";
    public int FilesCount = 0;

    public PS5AssetsBrowser()
    {
        // Init libX11 on FreeBSD for LibVLC, on Linux it shouldn't be necessary
        if (OperatingSystem.IsFreeBSD())
        {
            try
            {
                var InitSuccess = ImportHelper.Native.XInitThreads();
                if (InitSuccess != IntPtr.Zero)
                {
                    Close();
                }
            }
            catch { } //libX11 should be installed before when using FreeBSD
        }

        InitializeComponent();
        MediaPlayer = new MediaPlayer(NewLibVLCPlayer);

        Loaded += PS5AssetsBrowser_Loaded;

        ShowHideAssetsMenuItem.Click += ShowHideAssetsMenuItem_Click;
        PlayPauseMenuItem.Click += PlayPauseMenuItem_Click;
        StopMenuItem.Click += StopMenuItem_Click;

        AssetFilesListBox.PointerPressed += AssetFilesListView_PointerPressed;
    }

    private async void PS5AssetsBrowser_Loaded(object? sender, RoutedEventArgs e)
    {
        PlayerContextMenu.Items.Add(ShowHideAssetsMenuItem);
        PlayerContextMenu.Items.Add(PlayPauseMenuItem);
        PlayerContextMenu.Items.Add(StopMenuItem);

        AssetPlayer.ContextMenu = PlayerContextMenu;

        AssetFilesListBox.Transitions = [new DoubleTransition { Property = WidthProperty, Duration = TimeSpan.FromMilliseconds(400), Easing = new CubicEaseOut() }];
        AssetPlayer.Transitions = [new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(400), Easing = new CubicEaseOut() }];

        if (SelectedDirectory != null)
        {
            // Load playable assets
            FilesCount = 0;
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.mp4", SearchOption.AllDirectories).Length;
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.webm", SearchOption.AllDirectories).Length;
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.ogg", SearchOption.AllDirectories).Length;
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.png", SearchOption.AllDirectories).Length;
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.jpg", SearchOption.AllDirectories).Length;
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.ttf", SearchOption.AllDirectories).Length;

            NewLoadingWindow = new SyncWindow() { Title = "Loading asset files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = FilesCount;
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + FilesCount.ToString();
            NewLoadingWindow.Show();

            await LoadFiles();
            NewLoadingWindow.Close();
        }
    }

    private void ShowHideAssetsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (ShowHideAssetsMenuItem.Header!.ToString() == "Show assets browser")
        {
            AssetFilesListBox.Width = 350;
            AssetPlayer.Margin = new Thickness(350, 0, 0, 0);
            ShowHideAssetsMenuItem.Header = "Hide assets browser";
        }
        else
        {
            AssetFilesListBox.Width = 0;
            AssetPlayer.Margin = new Thickness(0, 0, 0, 0);
            ShowHideAssetsMenuItem.Header = "Show assets browser";
        }
    }

    private void PlayPauseMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (MediaPlayer.Media is not null)
        {
            if (PlayPauseMenuItem.Header!.ToString() == "Pause")
            {
                MediaPlayer.Pause();
                PlayPauseMenuItem.Header = "Play";
                PlayPauseMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
            }
            else
            {
                MediaPlayer.Play();
                PlayPauseMenuItem.Header = "Pause";
                PlayPauseMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/pause.png"))) };
            }
        }
    }

    private void AssetFilesListView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (AssetFilesListBox.SelectedItem != null)
            {
                AssetListViewItem SelectedAsset = (AssetListViewItem)AssetFilesListBox.SelectedItem;

                if (SelectedAsset.Type == AssetType.Video)
                {
                    AssetPlayer.IsVisible = true;
                    ImageViewer.IsVisible = false;
                    ImageViewer.Source = null;
                    FontPreviewTextBlock.IsVisible = false;
                    PlayPauseMenuItem.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/pause.png")));

                    // Set source & play
                    using var media = new Media(NewLibVLCPlayer, new Uri(SelectedAsset.AssetFilePath, UriKind.RelativeOrAbsolute));
                    MediaPlayer.Play(media);

                    // Hide file browser and expand player
                    AssetFilesListBox.Width = 0;
                    AssetPlayer.Margin = new Thickness(0, 0, 0, 0);
                }
                else if (SelectedAsset.Type == AssetType.Audio)
                {
                    AssetPlayer.IsVisible = true;
                    ImageViewer.IsVisible = false;
                    ImageViewer.Source = null;
                    FontPreviewTextBlock.IsVisible = false;
                    PlayPauseMenuItem.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/pause.png")));

                    using var media = new Media(NewLibVLCPlayer, new Uri(SelectedAsset.AssetFilePath, UriKind.RelativeOrAbsolute));
                    MediaPlayer.Play(media);
                }
                else if (SelectedAsset.Type == AssetType.Image)
                {
                    // Check if media is set and hide it to display the image
                    if (MediaPlayer.Media != null)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Media = null;
                    }

                    AssetPlayer.IsVisible = false;
                    FontPreviewTextBlock.IsVisible = false;
                    PlayPauseMenuItem.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png")));

                    if (Path.GetExtension(SelectedAsset.AssetFilePath) == ".png")
                    {
                        ImageViewer.IsVisible = true;
                        ImageViewer.Source = Utils.AnyBitmapToIImage(AnyBitmap.FromFile(SelectedAsset.AssetFilePath));
                    }
                    else if (Path.GetExtension(SelectedAsset.AssetFilePath) == ".jpg")
                    {
                        ImageViewer.IsVisible = true;
                        ImageViewer.Source = Utils.AnyBitmapToIImage(AnyBitmap.FromFile(SelectedAsset.AssetFilePath));
                    }
                    else if (Path.GetExtension(SelectedAsset.AssetFilePath) == ".dds")
                    {
                        using var NewPNGImage = new MagickImage(SelectedAsset.AssetFilePath);
                        NewPNGImage.SetCompression(CompressionMethod.NoCompression);
                        NewPNGImage.Format = MagickFormat.Png;

                        using var ms = new MemoryStream();
                        NewPNGImage.Write(ms, MagickFormat.Png);
                        ms.Position = 0;
                        var NewAnyBitmap = AnyBitmap.FromStream(ms);
                        ImageViewer.Source = Utils.AnyBitmapToIImage(NewAnyBitmap);
                        ms.Dispose();
                    }
                }

                else if (SelectedAsset.Type == AssetType.Font)
                {
                    if (MediaPlayer.Media != null)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Media = null;
                    }

                    AssetPlayer.IsVisible = false;
                    ImageViewer.IsVisible = false;
                    ImageViewer.Source = null;
                    PlayPauseMenuItem.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png")));

                    var collection = new FontCollection();
                    FontFamily NewFontFamily = collection.Add(SelectedAsset.AssetFilePath);
                    var InternalName = NewFontFamily.Name;
                    var FilePathAsUri = new Uri(SelectedAsset.AssetFilePath).AbsoluteUri;
                    var NewAvaloniaFontFamily = new Avalonia.Media.FontFamily(FilePathAsUri + "#" + InternalName);

                    FontPreviewTextBlock.FontFamily = NewAvaloniaFontFamily;
                    FontPreviewTextBlock.IsVisible = true;
                }

            }
        }
    }

    private async Task LoadFiles()
    {
        await Task.Run(() =>
        {
            foreach (var MP4File in Directory.GetFiles(SelectedDirectory, "*.mp4", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = MP4File, AssetFileName = Path.GetFileName(MP4File), Type = AssetType.Video };
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Movie-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Movie-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }

            foreach (var WEBMFile in Directory.GetFiles(SelectedDirectory, "*.webm", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = WEBMFile, AssetFileName = Path.GetFileName(WEBMFile), Type = AssetType.Video };
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Movie-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Movie-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }

            foreach (var OGGFile in Directory.GetFiles(SelectedDirectory, "*.ogg", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = OGGFile, AssetFileName = Path.GetFileName(OGGFile), Type = AssetType.Audio };

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Music-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Music-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }

            foreach (var PNGFile in Directory.GetFiles(SelectedDirectory, "*.png", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = PNGFile, AssetFileName = Path.GetFileName(PNGFile), Type = AssetType.Image };
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Image-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Image-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }

            foreach (var JPGFile in Directory.GetFiles(SelectedDirectory, "*.jpg", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = JPGFile, AssetFileName = Path.GetFileName(JPGFile), Type = AssetType.Image };
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Image-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Image-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }

            foreach (var DDSFile in Directory.GetFiles(SelectedDirectory, "*.dds", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = DDSFile, AssetFileName = Path.GetFileName(DDSFile), Type = AssetType.Image };
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Image-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Image-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }

            foreach (var FontFile in Directory.GetFiles(SelectedDirectory, "*.ttf", SearchOption.AllDirectories))
            {
                var NewAssetFile = new AssetListViewItem() { AssetFilePath = FontFile, AssetFileName = Path.GetFileName(FontFile), Type = AssetType.Font };
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Font-File.png")));
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                        AssetFilesListBox.Items.Add(NewAssetFile);
                    });
                }
                else
                {
                    NewAssetFile.Icon = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/Font-File.png")));
                    NewLoadingWindow.LoadProgressBar.Value += 1d;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FilesCount.ToString();
                    AssetFilesListBox.Items.Add(NewAssetFile);
                }
            }
        });
    }

    private void StopMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (MediaPlayer.Media != null)
        {
            MediaPlayer.Stop();
        }

        // Show file browser and minimize player
        AssetFilesListBox.Width = 350;
        AssetPlayer.Margin = new Thickness(350, 0, 0, 0);
    }

}