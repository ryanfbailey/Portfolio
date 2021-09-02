using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class PageHeader : MonoBehaviour
    {
        [Header("Background Settings")]
        // Background Swatch
        public string bgSwatchID;
        // Background Image
        public Image bgImage;

        [Header("Header Settings")]
        // Button margin
        public float buttonMargin = 20f;
        // Content
        public RectTransform buttonContainer;
        // Button prefab
        public RFBPButton headerButtonPrefab;
        // Resume button prefab
        public RFBPButton resumeButtonPrefab;
        // Buttons
        public List<RFBPButton> headerButtons { get; private set; }
        // Resume button
        public RFBButton resumeButton { get; private set; }

        // Rect container
        public RectTransform rectTransform { get; private set; }

        // Selected
        public int selectedPage { get; private set; }

        // Setup & add delegates
        private void Awake()
        {
            // Selected
            selectedPage = -1;

            // Get rect
            rectTransform = gameObject.GetComponent<RectTransform>();

            // Add delegate
            PageManager.onPageManagerLoad += PageManagerLoaded;
            PageManager.onPageSelected += PageSelected;
            RFBButton.onSelectChange += OnSelectChange;
        }
        // On destroy
        private void OnDestroy()
        {
            // Remove delegate
            PageManager.onPageManagerLoad -= PageManagerLoaded;
            PageManager.onPageSelected -= PageSelected;
            RFBButton.onSelectChange -= OnSelectChange;
        }

        // Page manager loaded
        private void PageManagerLoaded(PageManager pageManager)
        {
            // Set background
            if (bgImage != null && !string.IsNullOrEmpty(bgSwatchID))
            {
                bgImage.color = LayoutManager.instance.GetSwatchColor(bgSwatchID);
            }

            // Add resume button
            float x = 0f;
            resumeButton = GetButton(resumeButtonPrefab, "RESUME", ref x);
            resumeButton.selectButton = false;
            resumeButton.onClick.AddListener(ResumeClick);

            // Add button per page
            headerButtons = new List<RFBPButton>();
            for (int p = pageManager.pages.Count - 1; p >= 0; p--)
            {
                // Get page
                Page page = pageManager.pages[p];

                // Get button
                RFBPButton btn = GetButton(headerButtonPrefab, page.pageID, ref x);
                btn.selectButton = true;

                // Set button
                headerButtons.Add(btn);
            }
            headerButtons.Reverse();

            // Set selected
            SetSelected(0);
        }

        // Get button
        private RFBPButton GetButton(RFBPButton prefab, string localizationID, ref float x)
        {
            // Get button
            RFBPButton btn = Instantiate(prefab.gameObject).GetComponent<RFBPButton>();

            // Adjust transform
            RectTransform btnTransform = btn.GetComponent<RectTransform>();
            btnTransform.SetParent(buttonContainer);
            btnTransform.localPosition = Vector3.zero;
            btnTransform.localRotation = Quaternion.identity;
            btnTransform.localScale = Vector3.one;
            btnTransform.anchorMin = new Vector2(1f, 0.5f);
            btnTransform.anchorMax = btnTransform.anchorMin;
            btnTransform.pivot = btnTransform.anchorMin;
            btnTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.height);

            // Localize
            string pageTitle = LocalizationManager.instance.GetText(localizationID + "_BTN");
            btn.SetMainText(pageTitle);

            // Set size
            float btnWidth = btn.GetPreferredWidth();
            btnTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, x, btnWidth);
            x += btnWidth + buttonMargin;

            // Return button
            return btn;
        }

        // Button selection
        private void OnSelectChange(RFBButton btn, bool toSelected)
        {
            int index = headerButtons.IndexOf(btn.GetComponent<RFBPButton>());
            if (toSelected && index != -1)
            {

                // Set selected
                SetSelected(index);
            }
        }
        // Page manager selection (usually from scroll)
        private void PageSelected(int newPage)
        {
            SetSelected(newPage);
        }
        // Set selected
        public void SetSelected(int newIndex)
        {
            // Set selected
            if (selectedPage == newIndex)
            {
                return;
            }

            // Index
            selectedPage = newIndex;

            // Scroll to page
            PageManager.instance.ScrollToPage(selectedPage);

            // Refresh selected
            RefreshSelected();
        }
        // Refresh selected
        public void RefreshSelected()
        {
            for (int i = 0; i < headerButtons.Count; i++)
            {
                RFBButton btn = headerButtons[i];
                btn.SetSelected(i == selectedPage);
            }
        }

        // Take to pdf
        private void ResumeClick()
        {
            PortfolioManager.instance.OpenURL("RESUME_URL");
        }
    }
}
