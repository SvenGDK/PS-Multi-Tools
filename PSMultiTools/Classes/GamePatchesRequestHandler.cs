using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace PSMultiTools.Classes
{
    internal class GamePatchesRequestHandler : RequestHandler
    {
        // There's unfortunately no other way to download the _sc.pkg correctly unless we ignore certificate errors
        protected override bool OnCertificateError(CefBrowser browser, CefErrorCode certError, string requestUrl, CefSslInfo sslInfo, CefCallback callback)
        {
            callback.Continue();
            return true;
        }
    }
}
