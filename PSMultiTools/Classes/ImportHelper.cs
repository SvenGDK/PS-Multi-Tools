using System.Runtime.InteropServices;

namespace PSMultiTools.Classes
{
    public static class ImportHelper
    {
        public struct Native
        {
            /// <summary>
            /// Initializes the X threading system
            /// </summary>
            /// <remarks>X11 only</remarks>
            /// <returns>non-zero on success, zero on failure</returns>
            [DllImport("libX11", CallingConvention = CallingConvention.Cdecl)]
            public static extern int XInitThreads();
        }
    }
}
