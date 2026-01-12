using Avalonia.Controls;
using Avalonia.Threading;
using PSMultiTools.Classes;
using System;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.PS5.Tools.GamePatches;

public partial class PS5GamePatchSelector : Window
{

    public string CurrentGameID = "";
    public AvaloniaCefBrowser ContentWebView = new();

    public PS5GamePatchSelector()
    {
        InitializeComponent();

        Loaded += PS5GamePatchSelector_Loaded;

        ContentWebView.LoadEnd += ContentWebView_LoadEnd;
        this.FindControl<Decorator>("GamePatchesWebViewWrapper")!.Child = ContentWebView;
        ContentWebView.DownloadHandler = new GamePatchesDownloadHandler();
        ContentWebView.RequestHandler = new GamePatchesRequestHandler();
    }

    private void PS5GamePatchSelector_Loaded(object? sender, EventArgs e)
    {
        LoadingTextBlock.Text = "Loading game patches for" + Environment.NewLine + Environment.NewLine + CurrentGameID + Environment.NewLine + Environment.NewLine + "Please wait ...";
    }

    private void ContentWebView_LoadEnd(object sender, Xilium.CefGlue.Common.Events.LoadEndEventArgs e)
    {
        if (e.HttpStatusCode == 200)
        {

            Dispatcher.UIThread.Invoke(() => LoadingTextBlock.IsVisible = false);
            Dispatcher.UIThread.Invoke(() => GamePatchesWebViewWrapper.IsVisible = true);

            // Remove parts of the site
            string JS = "document.getElementsByClassName('navbar navbar-expand-lg bd-navbar sticky-top')[0].style.display='none';document.getElementsByClassName('py-2')[0].style.display='none';document.getElementsByClassName('py-4')[0].style.display='none';document.getElementsByClassName('ms-2 fw-normal')[0].style.display='none';document.getElementsByClassName('nav-link flex-fill share-icon')[0-x].style.display='none';";
            string AdditionalJS = "var sharebuttons = document.getElementsByClassName('nav-link flex-fill share-icon');for (let sharebutton of sharebuttons) { sharebutton.style.display='none'; };";

            if (ContentWebView.Address != null && ContentWebView.Address.StartsWith("https://prosperopatches.com/"))
            {
                ContentWebView.ExecuteJavaScript(JS);
                ContentWebView.ExecuteJavaScript(AdditionalJS);
            }
        }
    }

}