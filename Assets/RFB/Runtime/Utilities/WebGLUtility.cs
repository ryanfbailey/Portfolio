#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_ENABLED
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if WEB_ENABLED
using System.Runtime.InteropServices;
#endif

namespace RFB.Utilities
{
    public static class WebGLUtility
    {
#if WEB_ENABLED
        // Check for mobile device
        [DllImport("__Internal")]
        private static extern bool IsMobileDevice();

        // Open URL in new tab
        [DllImport("__Internal")]
        private static extern void OpenURLInNewTab(string url);
#endif

        // Check if web is mobile
        public static bool IsWebMobile()
        {
#if WEB_ENABLED
            return IsMobileDevice();
#else
            return false;
#endif
        }

        // Open URL
        public static void OpenWebURL(string url)
        {
#if WEB_ENABLED
            // On mobile, this fails
            if (AppManager.instance.debugPlatform == AppPlatform.Desktop)
            {
                OpenURLInNewTab(url);
            }
#endif

            // Open URL
            Application.OpenURL(url);
        }
    }
}
