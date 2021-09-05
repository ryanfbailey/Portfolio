#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_ENABLED
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
            if (AppManager.instance.platform == AppPlatform.Desktop)
            {
                OpenURLInNewTab(url);
                return;
            }
#endif

            // Open URL
            Application.OpenURL(url);
        }

        // Get web parameters
        public static Dictionary<string, string> GetWebParameters()
        {
            // Dictionary
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Start with URL
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url))
            {
                return parameters;
            }

            // Remove beginning
            int begin = url.IndexOf("://");
            if (begin != -1)
            {
                url = url.Substring(begin + 3);
            }

//#if WEB_ENABLED
            // Remove before ?
            begin = url.IndexOf("?");
            if (begin != -1)
            {
                url = url.Substring(begin + 1);
            }
//#endif
            // Split by &
            string[] sections = url.Split('&');
            if (sections != null)
            {
                foreach (string section in sections)
                {
                    // Get equal sign
                    int mid = section.IndexOf("=");
                    if (mid == -1)
                    {
                        continue;
                    }
                    // Get key
                    string key = section.Substring(0, mid);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    // Get & clean value
                    string val = section.Substring(mid + 1);
                    val = UnityWebRequest.UnEscapeURL(val);
                    // Apply
                    parameters[key] = val;
                }
            }

            // Return
            return parameters;
        }
    }
}
