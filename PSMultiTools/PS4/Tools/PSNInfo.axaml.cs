using Avalonia.Controls;
using Avalonia.Threading;
using IronSoftware.Drawing;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using System;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.PS4.Tools;

public partial class PSNInfo : Window
{
    public string CurrentGameContentID = string.Empty;
    public AvaloniaCefBrowser PSNBrowser = new() { Address = "about:blank" };

    public PSNInfo()
    {
        InitializeComponent();

        PSNBrowser.Initialized += PSNBrowser_Initialized;
        PSNBrowser.LoadEnd += PSNBrowser_LoadEnd;
        var browserWrapper = this.FindControl<Decorator>("WebViewWrapper");
        browserWrapper!.Child = PSNBrowser;
    }

    private async void PSNBrowser_LoadEnd(object sender, Xilium.CefGlue.Common.Events.LoadEndEventArgs e)
    {
        if (e.HttpStatusCode == 200)
        {
            var StoreJS = @"return (function() { var el = document.getElementById('mfe-jsonld-tags'); if (!el) return ''; return (el.innerHTML || '').trim(); })(); ";
            string StoreInfoResultJSON = await PSNBrowser.EvaluateJavaScript<string>(StoreJS);
            StoreInfoResultJSON = StoreInfoResultJSON.Trim();
            var StoreInfo = JsonConvert.DeserializeObject<StorePageInfos>(StoreInfoResultJSON);

            if (StoreInfo != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {

                    if (!string.IsNullOrEmpty(StoreInfo.Name))
                        GameTitleTextBlock.Text = StoreInfo.Name;

                    if (!string.IsNullOrEmpty(StoreInfo.Description))
                        DescriptionTextBlock.Text = StoreInfo.Description;

                    if (!string.IsNullOrEmpty(StoreInfo.Category))
                        CategoryTextBlock.Text = StoreInfo.Category;

                    if (!string.IsNullOrEmpty(StoreInfo.Sku))
                        GameCodeTextBlock.Text = StoreInfo.Sku;

                    if (!string.IsNullOrEmpty(StoreInfo.Image))
                    {
                        try
                        {
                            var uri = new Uri(StoreInfo.Image, UriKind.RelativeOrAbsolute);
                            var bmp = AnyBitmap.FromUri(uri);
                            GameImage.Source = Utils.AnyBitmapToIImage(bmp);
                        }
                        catch (Exception imgEx)
                        {
                            Console.WriteLine($"Failed to load image: {imgEx.Message}");
                        }
                    }
                });
            }
        }
    }

    private void PSNBrowser_Initialized(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentGameContentID))
        {
            PSNBrowser.Address = "https://store.playstation.com/en-us/product/" + CurrentGameContentID;
        }
    }

}