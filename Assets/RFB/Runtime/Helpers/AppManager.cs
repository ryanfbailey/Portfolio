using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RFB.Utilities
{
    // App platform
    public enum AppPlatform
    {
        Unknown,
        Desktop,
        Tablet,
        Mobile
    }

    // App Manager
    public class AppManager : Singleton<AppManager>
    {
#if UNITY_EDITOR
        [Header("Debug Settings")]
        public AppPlatform debugPlatform = AppPlatform.Desktop;
#endif

        // Tablet min size in inches
        public float tabletMinSize = 6.5f;

        // Platform set
        public AppPlatform platform { get; private set; }
        public static event Action<AppPlatform> onPlatformSet;

        // Loading
        public bool isLoading { get; private set; }
        public float loadProgress { get; private set; }
        public static event Func<IEnumerator> performLoad;
        public static event Action onLoadComplete;

        // On awake, get scripts
        protected override void Awake()
        {
            base.Awake();
            platform = AppPlatform.Unknown;
            isLoading = true;
        }

        // Load content
        protected IEnumerator Start()
        {
            // Ensure loading
            isLoading = true;

            // Refresh platform
            RefreshPlatform();

            // Wait for all loads
            if (performLoad != null)
            {
                foreach (Func<IEnumerator> del in performLoad.GetInvocationList())
                {
                    yield return StartCoroutine(del());
                }
            }

            // Done loading
            isLoading = false;

            // Load complete
            if (onLoadComplete != null)
            {
                onLoadComplete();
            }
        }

        // Refresh platform
        public void RefreshPlatform()
        {
            // Determine platform
            AppPlatform newPlatform = AppPlatform.Desktop;

#if UNITY_IOS || UNITY_ANDROID
            // Mobile platforms
            newPlatform = AppPlatform.Mobile
#elif UNITY_WEBGL
            // Use plugin to check platform
            if (WebGLUtility.IsWebMobile())
            {
                newPlatform = AppPlatform.Mobile;
            }
#endif

            // Check for tablet
            if (newPlatform == AppPlatform.Mobile && Screen.resolutions != null && Screen.resolutions.Length > 0)
            {
                // Set cross size
                Resolution resolution = Screen.resolutions[0];
                float dpi = Screen.dpi;
                float screenWidth = resolution.width / dpi;
                float screenHeight = resolution.height / dpi;
                float crossSize = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
                LogUtility.LogStatic("Screen Cross", crossSize.ToString("0.0") + "in");
                if (crossSize > tabletMinSize)
                {
                    newPlatform = AppPlatform.Tablet;
                }
            }


#if UNITY_EDITOR
            // Use debug platform
            if (debugPlatform != AppPlatform.Unknown)
            {
                newPlatform = debugPlatform;
            }
#endif

            // Platform set
            if (platform != newPlatform)
            {
                // Apply
                platform = newPlatform;
                LogUtility.LogStatic("Platform", platform.ToString());

                // Platform update
                if (onPlatformSet != null)
                {
                    onPlatformSet(platform);
                }
            }
        }
    }
}
