using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;
using TMPro;

namespace RFB.Portfolio
{
    public class PageFooter : RFBMonoBehaviour
    {
        [Header("UI Settings")]
        // Get data
        public string middleTextID = "FOOTER_MIDDLE";
        public string middleLinkID = "REPO_URL";
        // Button
        public RFBPButton middleButton;
        // Background
        public string bgSwatchID = "";
        public Image bg;

        // Awake
        protected virtual void Awake()
        {
            middleButton.onClick += (MiddleButtonClick);
            LocalizationManager.onLoadComplete += OnLocalizationLoad;
            bg.color = LayoutManager.instance.GetSwatchColor(bgSwatchID);
        }
        // Destroy
        protected virtual void OnDestroy()
        {
            middleButton.onClick -= (MiddleButtonClick);
            LocalizationManager.onLoadComplete -= OnLocalizationLoad;
        }

        // Localization load
        private void OnLocalizationLoad()
        {
            string text = LocalizationManager.instance.GetText(middleTextID) + " - v" + Application.version;
            middleButton.SetMainText(text);
        }
        // Middle Button click
        private void MiddleButtonClick()
        {
            PortfolioManager.instance.OpenWebLocalizedURL(middleLinkID);
        }
    }
}
