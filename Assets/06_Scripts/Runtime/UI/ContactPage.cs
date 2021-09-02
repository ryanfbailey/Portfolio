using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class ContactPage : Page
    {
        [Header("UI Settings")]
        // Labels
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI subtitleLabel;
        // Contact button
        public float contactPadding = 50f;
        public RFBPButton contactBtn;

        #region BUTTONS
        // Add delegates
        protected override void Awake()
        {
            base.Awake();
            contactBtn.onClick.AddListener(ContactClick);
        }
        // Remove delegates
        protected override void OnDestroy()
        {
            base.OnDestroy();
            contactBtn.onClick.RemoveListener(ContactClick);
        }
        // Contact click
        private void ContactClick()
        {
            PortfolioManager.instance.OpenWebLocalizedURL("EMAIL_URL");
        }
        #endregion

        #region LAYOUT
        // Load
        public override void Load()
        {
            base.Load();

            // Layout texts
            titleLabel.text = LocalizationManager.instance.GetText("CONTACT_PAGE_TITLE");
            LayoutManager.instance.ApplyLabelSettings(titleLabel, "TITLE");
            subtitleLabel.text = LocalizationManager.instance.GetText("CONTACT_PAGE_SUBTITLE");
            LayoutManager.instance.ApplyLabelSettings(subtitleLabel, "SUBTITLE");

            // Set up button
            contactBtn.SetMainText(LocalizationManager.instance.GetText("CONTACT_PAGE_BTN"));
            contactBtn.SetPreferredWidth();
        }

        // Resize
        public override float Resize()
        {
            // Build
            float y = base.Resize();
            float width = contentContainer.rect.width;

            // Set title
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, width);
            float titleHeight = titleLabel.GetPreferredValues(titleLabel.text, width, 10000f).y;
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, titleHeight);
            y += titleHeight + textPadding;

            // Set subtitle
            subtitleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, width);
            float subtitleHeight = subtitleLabel.GetPreferredValues(subtitleLabel.text, width, 10000f).y;
            subtitleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, subtitleHeight);
            y += subtitleHeight + contactPadding;

            // Set button
            RectTransform contactTrans = contactBtn.GetComponent<RectTransform>();
            contactTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, contactTrans.rect.height);
            y += contactTrans.rect.height;

            // Return base
            return y;
        }
        #endregion
    }
}
