using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class AboutPage : Page
    {
        [Header("UI Settings")]
        // Name
        public string nameSwatchID;
        public RawImage nameImage;
        public Vector2 nameShadowOffset;
        public RectTransform nameImageShadow;
        // Profile
        public RawImage profileImage;
        // Buttons
        public RectTransform buttonContainer;
        public RFBPButton linkedInBtn;
        public RFBPButton githubBtn;
        public RFBPButton instagramBtn;
        // Labels
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI subtitleLabel;
        // Contact button
        public RFBPButton contactBtn;

        [Header("Layout Settings")]
        public float namePadding = 10f;
        public float imagePadding = 20f;
        public float contactPrePadding = 20f;
        public float contactPostPadding = 20f;

        #region BUTTONS
        // Add delegates
        protected override void Awake()
        {
            base.Awake();
            linkedInBtn.onClick.AddListener(LinkedInClick);
            githubBtn.onClick.AddListener(GithubClick);
            instagramBtn.onClick.AddListener(InstagramClick);
            contactBtn.onClick.AddListener(ContactClick);
        }
        // Remove delegates
        protected override void OnDestroy()
        {
            base.OnDestroy();
            linkedInBtn.onClick.RemoveListener(LinkedInClick);
            githubBtn.onClick.RemoveListener(GithubClick);
            instagramBtn.onClick.RemoveListener(InstagramClick);
            contactBtn.onClick.RemoveListener(ContactClick);
        }
        // Linked In Click
        private void LinkedInClick()
        {
            PortfolioManager.instance.OpenWebLocalizedURL("LINKEDIN_URL");
        }
        // Github click
        private void GithubClick()
        {
            PortfolioManager.instance.OpenWebLocalizedURL("GITHUB_URL");
        }
        // Instagram click
        private void InstagramClick()
        {
            PortfolioManager.instance.OpenWebLocalizedURL("INSTAGRAM_URL");
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

            // Set name label & apply
            nameImage.color = LayoutManager.instance.GetSwatchColor(nameSwatchID);

            // Layout additional texts
            titleLabel.text = LocalizationManager.instance.GetText("ABOUT_PAGE_TITLE");
            LayoutManager.instance.ApplyLabelSettings(titleLabel, "TITLE");
            subtitleLabel.text = LocalizationManager.instance.GetText("ABOUT_PAGE_SUBTITLE");
            LayoutManager.instance.ApplyLabelSettings(subtitleLabel, "SUBTITLE");

            // Set up button
            contactBtn.SetMainText(LocalizationManager.instance.GetText("ABOUT_PAGE_BTN"));
            contactBtn.SetPreferredWidth();
        }
        // Get fancy string
        private string GetFancyString(string content, string fontID, float fontSize)
        {
            string result = content;
            result = "<font=" + fontID + ">" + result + "</font>";
            result = "<size=" + fontSize + ">" + result + "</size>";
            return result;
        }

        // Resize
        public override float Resize()
        {
            // Portrait
            bool isPortrait = LayoutManager.instance.orientation == LayoutOrientation.Portrait;
            float containerWidth = contentContainer.rect.width;

            // Make portrait if profile doesnt fit
            if (profileImage.texture.width + imagePadding > containerWidth * 0.65f)
            {
                isPortrait = true;
            }
            float profileWidth = profileImage.texture.width;

            // Begin
            float y = base.Resize();
            float rightY = 0f;

            // Set name position/size
            float nameWidth = containerWidth - (isPortrait ? 0f : profileWidth + imagePadding);
            float nameHeight = nameWidth * (float)nameImage.texture.height / (float)nameImage.texture.width;
            nameImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, nameWidth);
            nameImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, nameHeight);
            nameImageShadow.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, nameShadowOffset.x, nameWidth);
            nameImageShadow.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y + nameShadowOffset.y, nameHeight);
            y += nameHeight + namePadding;

            // Set profile image position/size
            float profileX = isPortrait ? ((containerWidth - profileWidth) * .5f) : 0f;
            float profileHeight = profileWidth * (float)profileImage.texture.height / (float)profileImage.texture.width;
            profileImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, profileX, profileWidth);
            buttonContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, profileX, profileWidth);
            if (isPortrait)
            {
                y -= namePadding;
                y += imagePadding;
                profileImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, profileHeight);
                y += profileHeight + imagePadding;
                buttonContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, buttonContainer.rect.height);
                y += buttonContainer.rect.height + imagePadding;
            }
            else
            {
                profileImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, profileHeight);
                rightY += profileHeight + imagePadding;
                buttonContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, rightY, buttonContainer.rect.height);
                rightY += buttonContainer.rect.height + contactPostPadding;
            }

            // Set title
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, nameWidth);
            float titleHeight = titleLabel.GetPreferredValues(titleLabel.text, nameWidth, 10000f).y;
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, titleHeight);
            y += titleHeight + textPadding;

            // Set subtitle
            subtitleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, nameWidth);
            float subtitleHeight = subtitleLabel.GetPreferredValues(subtitleLabel.text, nameWidth, 10000f).y;
            subtitleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, subtitleHeight);
            y += subtitleHeight + contactPrePadding;

            // Set button
            RectTransform contactTrans = contactBtn.GetComponent<RectTransform>();
            contactTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, contactTrans.rect.height);
            y += contactTrans.rect.height + contactPostPadding;

            // Add container padding
            y = Mathf.Max(rightY, y);

            // Return base
            return y;
        }
        #endregion
    }
}
