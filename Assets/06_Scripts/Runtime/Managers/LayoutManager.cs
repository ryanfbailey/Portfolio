using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RFB.Utilities;

namespace RFB.Portfolio
{
    // Orientation
    public enum LayoutOrientation
    {
        Unknown,
        Portrait,
        Landscape
    }

    // Layout swatch
    [Serializable]
    public struct LayoutSwatch
    {
        public string swatchID;
        public Color swatchColor; 
    }
    // Label settings
    [Serializable]
    public struct LayoutLabelSettings
    {
        // ID
        public string labelID;

        // Color
        public string labelSwatchID;

        // Font
        public float labelFontSize;
        public FontStyles labelFontStyles;
        public TMP_FontAsset labelFont;
        public Material labelFontMaterial;
    }

    // Manager
    public class LayoutManager : Singleton<LayoutManager>
    {
        #region LIFECYCLE
        // On awake
        protected override void Awake()
        {
            base.Awake();

            // On change
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasScaler = canvas.GetComponent<CanvasScaler>();

            // Set platform
            PlatformSet(AppManager.instance.platform);
            AppManager.onPlatformSet += PlatformSet;
        }
        // Remove delegate
        protected override void OnDestroy()
        {
            base.OnDestroy();
            AppManager.onPlatformSet -= PlatformSet;
        }

        // Platform set, refresh scale
        private void PlatformSet(AppPlatform platform)
        {
            if (platform != AppPlatform.Unknown)
            {
                RefreshScreenSize();
                RefreshCanvasScale();
            }
        }

        // On update
        private void Update()
        {
            RefreshScreenSize();
            RefreshCanvasSize();
        }
        #endregion

        #region SCREEN
        // Orientation
        public LayoutOrientation orientation { get; private set; }

        // Screen sizes
        public int screenWidth { get; private set; }
        public int screenHeight { get; private set; }
        public float screenDPI { get; private set; }
        public static event Action<float, float, float> onScreenSizeChange;

        // Update canvas
        public void RefreshScreenSize()
        {
            // Get
            int newWidth = Screen.width;
            int newHeight = Screen.height;
            float newDPI = Screen.dpi;

            // If changed
            if (screenWidth != newWidth || screenHeight != newHeight || screenDPI != newDPI)
            {
                // Apply
                screenWidth = newWidth;
                screenHeight = newHeight;
                screenDPI = newDPI;
                orientation = screenWidth >= screenHeight ? LayoutOrientation.Landscape : LayoutOrientation.Portrait;

                // Log
                LogUtility.LogStatic("Screen Size", screenWidth.ToString() + "x" + screenHeight.ToString());
                LogUtility.LogStatic("Screen DPI", screenDPI.ToString("0.0"));
                LogUtility.LogStatic("Screen Orientation", orientation.ToString());

                // Call delegate
                if (onScreenSizeChange != null)
                {
                    onScreenSizeChange(screenWidth, screenHeight, screenDPI);
                }

                // Refresh scale
                RefreshCanvasScale();
            }
        }
        #endregion

        #region CANVAS
        [Header("Canvas Settings")]
        // Main canvas itself
        public Canvas canvas;
        // Desktop scale
        public float desktopScale = 1f;
        // Tablet scale
        public float tabletScale = 0.8f;
        // Mobile scale
        public float mobileScale = 0.6f;

        // Canvas size
        public RectTransform canvasRect { get; private set; }
        public float canvasWidth { get; private set; }
        public float canvasHeight { get; private set; }
        public static event Action<float, float> onCanvasSizeChange;

        // Canvas scale
        public CanvasScaler canvasScaler { get; private set; }
        public float canvasScale { get; private set; }
        public static event Action<float> onCanvasScaleChange;

        // Refreshing canvas
        public void RefreshCanvasScale()
        {
            // Adjust match
            float newMatch = orientation == LayoutOrientation.Portrait ? 0f : 1f;

            // Determine scale
            float newScale;
            switch (AppManager.instance.platform)
            {
                case AppPlatform.Mobile:
                    newScale = mobileScale;
                    break;
                case AppPlatform.Tablet:
                    newScale = tabletScale;
                    break;
                case AppPlatform.Desktop:
                default:
                    newScale = desktopScale;
                    break;
            }

            // Apply
            if (canvasScale != newScale || canvasScaler.matchWidthOrHeight != newMatch)
            {
                // Set scale
                canvasScale = newScale;
                canvasScaler.referenceResolution = Vector2.one * 1080f * canvasScale;
                canvasScaler.matchWidthOrHeight = newMatch;

                // Log
                LogUtility.LogStatic("Canvas Scale", canvasScale.ToString("0.00"));

                // Canvas scale change
                if (onCanvasScaleChange != null)
                {
                    onCanvasScaleChange(canvasScale);
                }
            }
        }

        // Update canvas
        public void RefreshCanvasSize()
        {
            // Get
            float newWidth = canvasRect.rect.width;
            float newHeight = canvasRect.rect.height;

            // If changed
            if (canvasWidth != newWidth || canvasHeight != newHeight)
            {
                // Set
                canvasWidth = newWidth;
                canvasHeight = newHeight;

                // Log
                LogUtility.LogStatic("Canvas Size", canvasWidth.ToString("0.00") + "x" + canvasHeight.ToString("0.00"));

                // Call delegate
                if (onCanvasSizeChange != null)
                {
                    onCanvasSizeChange(canvasWidth, canvasHeight);
                }
            }
        }
        #endregion

        #region LAYOUT
        [Header("Skin Settings")]
        // Swatches
        public LayoutSwatch[] swatches;
        // Labels
        public LayoutLabelSettings[] labelSettings;

        // Get index
        public int GetSwatchIndex(string id)
        {
            if (swatches != null && !string.IsNullOrEmpty(id))
            {
                for (int i = 0; i < swatches.Length; i++)
                {
                    if (id.Equals(swatches[i].swatchID, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        // Get swatch color
        public Color GetSwatchColor(string id)
        {
            int index = GetSwatchIndex(id);
            if (index != -1)
            {
                return swatches[index].swatchColor;
            }
            return Color.clear;
        }

        // Get index
        public int GetLabelIndex(string id)
        {
            if (labelSettings != null && !string.IsNullOrEmpty(id))
            {
                for (int i = 0; i < labelSettings.Length; i++)
                {
                    if (id.Equals(labelSettings[i].labelID, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        // Get settings
        public LayoutLabelSettings GetLabelSettings(string id)
        {
            int index = GetLabelIndex(id);
            if (index != -1)
            {
                return labelSettings[index];
            }
            return new LayoutLabelSettings();
        }
        // Get label
        public TextMeshProUGUI GetLabel(RectTransform parent, string id)
        {
            // Get label
            TextMeshProUGUI label = UIUtility.GetChildLabel(parent, id);

            // Apply settings
            ApplyLabelSettings(label, id);

            // Return
            return label;
        }
        // Apply settings
        public void ApplyLabelSettings(TextMeshProUGUI label, string id)
        {
            // Ignore without label
            if (label == null)
            {
                return;
            }
            // Ignore without index
            int index = GetLabelIndex(id);
            if (index == -1)
            {
                return;
            }

            // Apply settings
            LayoutLabelSettings settings = labelSettings[index];
            label.color = GetSwatchColor(settings.labelSwatchID);
            label.font = settings.labelFont;
            label.fontMaterial = settings.labelFontMaterial;
            label.fontSize = settings.labelFontSize;
            label.fontStyle = settings.labelFontStyles;
        }
        #endregion
    }
}
