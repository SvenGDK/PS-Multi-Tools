using Avalonia.Controls;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.PS3.Tools;

public partial class PS3webMANBrowser : Window
{

    private readonly AvaloniaCefBrowser WebMANWebView = new() { Address = "about:blank" };
    public string WebMANAddress = "";

    public PS3webMANBrowser()
    {
        InitializeComponent();

        WebMANWebView.Initialized += WebMANWebView_Initialized;
        var browserWrapper = this.FindControl<Decorator>("WebMANWebViewWrapper");
        browserWrapper!.Child = WebMANWebView;
    }

    private void WebMANWebView_Initialized(object? sender, System.EventArgs e)
    {
        WebMANWebView.Address = WebMANAddress;
    }

}