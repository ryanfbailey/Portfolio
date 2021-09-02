using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class PageManager : Singleton<PageManager>
    {
        #region LIFECYCLE
        [Header("UI Settings")]
        // Container
        public RectTransform container;
        // Rect transform
        public RectTransform rectTransform { get; private set; }
        // Scroll rect
        public ScrollRect scroller { get; private set; }

        // Page Prefabs
        public Page[] pagePrefabs;
        // Pages
        public List<Page> pages { get; private set; }

        // Page manager loaded
        public static event Action<PageManager> onPageManagerLoad;
        public static event Action<PageManager> onPageManagerLayout;

        // On awake
        protected override void Awake()
        {
            base.Awake();

            // Setup
            currentPage = 0;
            rectTransform = gameObject.GetComponent<RectTransform>();
            scroller = gameObject.GetComponent<ScrollRect>();
            if (scroller == null)
            {
                scroller = gameObject.AddComponent<ScrollRect>();
            }
            scroller.content = container;
            scroller.horizontal = false;
            scroller.vertical = true;

            // Delegate
            LocalizationManager.onLoadComplete += OnLoadComplete;
            AppManager.performLoad += WaitForLoad;
            LayoutManager.onCanvasSizeChange += CanvasSizeChanged;
        }
        // On destroy
        protected override void OnDestroy()
        {
            base.OnDestroy();
            LocalizationManager.onLoadComplete -= OnLoadComplete;
            AppManager.performLoad -= WaitForLoad;
            LayoutManager.onCanvasSizeChange -= CanvasSizeChanged;
        }
        #endregion

        #region LOAD
        // Load complete, generate pages
        private void OnLoadComplete()
        {
            GeneratePages();
        }
        // Generate pages
        private void GeneratePages()
        {
            // Already exist
            if (pages != null)
            {
                return;
            }

            // Generate pages
            pages = new List<Page>();
            foreach (Page pagePrefab in pagePrefabs)
            {
                // Skip
                if (pagePrefab == null)
                {
                    continue;
                }

                // Generate page
                Page page = Instantiate(pagePrefab.gameObject).GetComponent<Page>();
                page.gameObject.name = pagePrefab.gameObject.name;

                // Setup transform
                RectTransform pageTransform = page.GetComponent<RectTransform>();
                pageTransform.SetParent(container);
                pageTransform.localPosition = Vector3.zero;
                pageTransform.localRotation = Quaternion.identity;
                pageTransform.localScale = Vector3.one;
                pageTransform.anchorMin = new Vector2(0f, 1f);
                pageTransform.anchorMax = new Vector2(1f, 1f);
                pageTransform.pivot = new Vector2(0.5f, 1f);
                pageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, container.rect.width);

                // Load page
                page.Load();

                // Add to list
                pages.Add(page);
            }

            // Resize
            ResizePages();

            // Page manager loaded
            if (onPageManagerLoad != null)
            {
                onPageManagerLoad(this);
            }
        }
        // Perform load
        private IEnumerator WaitForLoad()
        {
            // Wait a moment
            yield return new WaitForEndOfFrame();

            // Wait for all pages to load
            while (IsLoading())
            {
                yield return null;
            }
        }
        // Check if loading
        public bool IsLoading()
        {
            // Check pages
            if (pages != null)
            {
                // Get loading page ids
                string pageIDs = "";
                foreach (Page page in pages)
                {
                    if (page.isLoading)
                    {
                        if (!string.IsNullOrEmpty(pageIDs))
                        {
                            pageIDs += ", ";
                        }
                        pageIDs += page.pageID;
                    }
                }

                // Set log
                LogUtility.LogStatic("Pages Loading", pageIDs);

                // Loading
                if (!string.IsNullOrEmpty(pageIDs))
                {
                    return true;
                }
            }

            // Not loading
            return false;
        }
        #endregion

        #region RESIZE
        [Header("Layout Settings")]
        // Page margins
        public float pageHeaderHeight = 150f;
        public float pageFooterHeight = 150f;
        // Page margin
        public float pageMargin = 50f;
        // Page min margin
        public float pageMinPadding = 50f;
        // Page
        public float pageAspectScale { get; private set; }

        // Canvas size changed
        private void CanvasSizeChanged(float w, float h)
        {
            ResizePages();
        }
        // Resize pages
        public void ResizePages()
        {
            // Ignore
            if (pages == null)
            {
                return;
            }

            // Offset
            Vector2 offset = container.anchoredPosition;
            //float progress = offset.y / container.rect.height;

            // Get aspect
            pageAspectScale = rectTransform.rect.height / 1080f;
            LogUtility.LogStatic("Page Aspect", pageAspectScale.ToString("0.00"));

            // Add header height
            float height = pageHeaderHeight;

            // Determine min page height
            float minPageHeight = LayoutManager.instance.canvasHeight;
            // Subtract header
            minPageHeight -= pageHeaderHeight;
            // Subtract footer
            minPageHeight -= pageFooterHeight;

            // Iterate pages
            for (int p = 0; p < pages.Count; p++)
            {
                // Add margin
                if (p > 0)
                {
                    height += pageMargin;
                }

                // Get page
                Page page = pages[p];

                // Get content height
                float pageContentHeight = page.Resize();

                // Get full height
                float pageFullHeight = pageContentHeight + 2f * pageMinPadding * pageAspectScale;
                pageFullHeight = Mathf.Max(pageFullHeight, minPageHeight);
                page.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, height, pageFullHeight);

                // Set content height
                page.contentContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (pageFullHeight - pageContentHeight) * .5f , pageContentHeight);

                // Add height
                height += pageFullHeight;
            }

            // Add bottom margin
            height += pageFooterHeight;

            // Set size
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // Set offset
            //offset.y = progress * height;
            container.anchoredPosition = offset;
            // Scroll to current page
            //ScrollToPage(currentPage, true);

            // Layout
            if (onPageManagerLayout != null)
            {
                onPageManagerLayout(this);
            }
        }
        #endregion

        #region SCROLL
        [Header("Animation Settings")]
        // Duration
        public float scrollDuration = 1f;
        // Fast to slow
        public TweenEase scrollEase = TweenEase.easeOutQuad;

        // Animating
        public bool isAnimating { get; private set; }
        // Selected page
        public int currentPage { get; private set; }
        // Page selected
        public static event Action<int> onPageSelected;

        // Page auto scroll
        public const string ANIM_ID = "PAGE_SCROLL";

        // Current offset
        public float GetCurrentOffset()
        {
            if (container != null)
            {
                return container.anchoredPosition.y;
            }
            return 0f;
        }
        // Get page offset
        public float GetPageOffset(int newIndex)
        {
            if (pages != null && newIndex >= 0 && newIndex < pages.Count)
            {
                return Mathf.Round(-pages[newIndex].rectTransform.anchoredPosition.y - pageHeaderHeight * PageManager.instance.pageAspectScale);
            }
            return 0f;
        }
        // Set page
        private void SetPage(int newIndex)
        {
            // Set page
            if (currentPage == newIndex)
            {
                return;
            }

            // Set page
            currentPage = newIndex;

            // Page selected
            if (onPageSelected != null)
            {
                onPageSelected(currentPage);
            }
        }
        // Scroll to page
        public void ScrollToPage(int newIndex, bool immediately = false)
        {
            // Stop from animating
            if (currentPage == newIndex && !immediately)
            {
                return;
            }

            // Set page
            SetPage(newIndex);

            // Disable scrolling
            isAnimating = true;
            scroller.enabled = false;

            // Get data
            TweenUtility.TweenData tData = new TweenUtility.TweenData();
            tData.tweenID = ANIM_ID;
            tData.startValue = GetCurrentOffset();
            tData.endValue = GetPageOffset(newIndex);
            tData.duration = immediately ? 0f : scrollDuration;
            tData.ease = scrollEase;
            tData.onUpdate = OnTweenUpdate;
            tData.onComplete = OnTweenComplete;

            // Tween
            TweenUtility.StartTween(gameObject, tData);
        }
        // Tween update
        private void OnTweenUpdate(GameObject go, string tweenID, float newValue)
        {
            Vector2 pos = container.anchoredPosition;
            pos.y = newValue;
            container.anchoredPosition = pos;
        }
        // Tween complete
        private void OnTweenComplete(GameObject go, string tweenID, bool cancelled)
        {
            // Enable scrolling
            isAnimating = false;
            scroller.enabled = true;
        }
        // Update
        private void Update()
        {
            // Check for page change
            if (!isAnimating && pages != null)
            {
                int newPage = GetClosestPage();
                if (currentPage != newPage)
                {
                    //Debug.Log("Set Page: " + newPage);
                    SetPage(newPage);
                }
            }
        }
        // Closest page
        private int GetClosestPage()
        {
            // Current
            int page = 0;
            float offset = GetCurrentOffset();
            //string log = "Cur: " + offset.ToString("0.00");

            // Check
            for (int p = 0; p < pages.Count; p++)
            {
                float pageOffset = GetPageOffset(p);
                //log += "\n" + p + ": " + pageOffset.ToString("0.00");
                if (offset >= pageOffset)
                {
                    page = p;
                }
                else
                {
                    break;
                }
            }

            // Return
            //Debug.Log(log);
            return page;
        }
        #endregion
    }
}
