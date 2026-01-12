using Avalonia.Controls;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.PS5.Tools.WebSrv;

public partial class PS5webMANBrowser : Window
{

    private readonly AvaloniaCefBrowser WebMANWebSrvView = new() { Address = "about:blank" };
    public string WebMANWebSrvAddress = "";

    public PS5webMANBrowser()
    {
        InitializeComponent();

        WebMANWebSrvView.Initialized += WebMANWebSrvView_Initialized;
        var browserWrapper = this.FindControl<Decorator>("WebMANWebSrvViewWrapper");
        browserWrapper!.Child = WebMANWebSrvView;
    }

    private void WebMANWebSrvView_Initialized(object? sender, System.EventArgs e)
    {
        WebMANWebSrvView.Address = WebMANWebSrvAddress;
    }

}