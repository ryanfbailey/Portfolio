using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class Page : RFBMonoBehaviour
    {
        [Header("Page Settings")]
        // Page ID
        public string pageID;
        // BG swatch id
        public string pageBgSwatchID;
        // Label id
        public string pageHeaderLabelID;
        // Text padding
        public float textPadding = 5f;

        [Header("UI Settings")]
        // Background
        public Image pageBg;
        // Container
        public RectTransform contentContainer;

        // Loading
        public bool isLoading { get; protected set; }

        // Rect container
        public RectTransform rectTransform { get; private set; }
        // Header label
        public TextMeshProUGUI headerLabel { get; private set; }

        // Awake
        protected virtual void Awake()
        {
            // Get rect
            rectTransform = gameObject.GetComponent<RectTransform>();
        }
        // For future
        protected virtual void OnDestroy()
        {

        }

        // Loads page
        public virtual void Load()
        {
            // Begin loading
            isLoading = true;

            // Create & set background
            pageBg.color = LayoutManager.instance.GetSwatchColor(pageBgSwatchID);

            // Create header
            if (!string.IsNullOrEmpty(pageHeaderLabelID))
            {
                // Load settings
                headerLabel = LayoutManager.instance.GetLabel(contentContainer, pageHeaderLabelID);
                // Use localized
                headerLabel.text = LocalizationManager.instance.GetText(pageID + "_HEADER");
            }

            // Handle asset loading
            StartCoroutine(PerformAssetLoad());
        }
        // Perform load
        protected virtual IEnumerator PerformAssetLoad()
        {
            // TODO: Override
            yield return null;
            //yield return new WaitForSeconds(1f);

            // Done loading
            LoadComplete();
        }
        // Done loading
        protected void LoadComplete()
        {
            isLoading = false;
        }

        // Resize & return desired height
        public virtual float Resize()
        {
            // Height
            float height = 0f;

            // Add header label
            if (!string.IsNullOrEmpty(headerLabel.text))
            {
                height += headerLabel.preferredHeight + textPadding;
            }

            // Return height
            return height;
        }
    }
}
