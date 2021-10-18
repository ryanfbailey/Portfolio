using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class ProjectGallery : MonoBehaviour
    {
        #region LIFECYCLE
        [Header("Thumbnail Settings")]
        // Thumbnail margin
        public float galleryThumbMargin = 20f;
        // Thumbnail padding
        public float galleryThumbPadding = 50f;
        // Max Size
        public float galleryThumbMaxSize = 200f;
        // Thumbnail prefab
        public ProjectGalleryThumb galleryThumbPrefab;
        // Thumbnail container
        public RectTransform galleryThumbContainer;

        // Rect
        public RectTransform rectTransform { get; private set; }
        // Project thumbnails
        public ProjectGalleryThumb[] thumbnails { get; private set; }

        // Add delegate
        protected virtual void Awake()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
        }
        // Remove delegate
        protected virtual void OnDestroy()
        {
            UnloadThumbs();
        }
        // Unload thumbnails
        private void UnloadThumbs()
        {
            // Unload button gameobjects
            if (thumbnails != null)
            {
                foreach (ProjectGalleryThumb thumb in thumbnails)
                {
                    if (thumb != null)
                    {
                        // Unload thumb
                        thumb.Unload();

                        // Unload
                        if (RFBPool.instance != null)
                        {
                            RFBPool.instance.Unload(thumb.gameObject);
                        }
                    }
                }
                thumbnails = null;
            }
            // Disable
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }
        #endregion

        #region LOAD
        // Load thumbnails
        public void LoadThumbs()
        {
            // Ensure unloaded
            UnloadThumbs();

            // Ignore without projects
            int projectIndex = PortfolioManager.instance.selectedProject;
            if (PortfolioManager.instance.projects == null || projectIndex < 0 || projectIndex >= PortfolioManager.instance.projects.Length)
            {
                return;
            }
            // Ignore without gallery items
            ProjectData project = PortfolioManager.instance.projects[projectIndex];
            int total = project.gallery == null ? 0 : project.gallery.Length;
            if (total <= 0)
            {
                return;
            }

            // Thumbnails
            thumbnails = new ProjectGalleryThumb[total];
            for (int t = 0; t < total; t++)
            {
                // Instantiate
                ProjectGalleryThumb thumb = RFBPool.instance.Load(galleryThumbPrefab.gameObject).GetComponent<ProjectGalleryThumb>();
                thumb.gameObject.name = "THUMB_" + t.ToString("00");

                // Setup
                RectTransform thumbTrans = thumb.GetComponent<RectTransform>();
                thumbTrans.SetParent(galleryThumbContainer);
                thumbTrans.localPosition = Vector3.zero;
                thumbTrans.localRotation = Quaternion.identity;
                thumbTrans.localScale = Vector3.one;
                thumbTrans.pivot = new Vector2(0f, 1f);
                thumbTrans.anchorMin = new Vector2(0f, 1f);
                thumbTrans.anchorMax = new Vector2(0f, 1f);

                // Load
                GalleryItemData thumbData = project.gallery[t];
                thumb.Load(thumbData);

                // Set thumb
                thumbnails[t] = thumb;
            }

            // Enable
            gameObject.SetActive(true);

            // Layout
            LayoutThumbs(rectTransform.rect.width);
        }
        #endregion

        #region LAYOUT
        // Layout thumbs
        public float LayoutThumbs(float width)
        {
            // Not valid
            if (!gameObject.activeSelf || thumbnails == null)
            {
                return 0f;
            }

            // Get thumbnail width
            float maxWidth = width - galleryThumbMargin * 2f;
            int columns = Mathf.FloorToInt(maxWidth / galleryThumbMaxSize);
            maxWidth = maxWidth - (galleryThumbPadding * Mathf.Max(0f, columns - 1));
            float thumbWidth = maxWidth / (float)columns;
            int rows = Mathf.CeilToInt((float)thumbnails.Length / (float)columns);

            // Build
            float x = galleryThumbMargin;
            float y = galleryThumbMargin;

            // Helpers
            int column = 0;
            int row = 0;
            float rowHeight = 0f;

            // Iterate thumbnails
            for (int t = 0; t < thumbnails.Length; t++)
            {
                // Get thumbnail & layout
                ProjectGalleryThumb thumb = thumbnails[t];

                // Add column margin
                if (column > 0)
                {
                    x += galleryThumbPadding;
                }

                // Set width
                thumb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, thumbWidth);
                x += thumbWidth;

                // Layout
                float thumbHeight = thumb.Layout(thumbWidth);

                // Set height
                thumb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, thumbHeight);
                rowHeight = Mathf.Max(rowHeight, thumbHeight);

                // Iterate column
                column++;

                // Iterate row
                if (column == columns)
                {
                    column = 0;
                    row++;
                    x = galleryThumbMargin;
                    y += rowHeight + galleryThumbPadding;
                    rowHeight = 0f;
                    if (row >= rows - 1)
                    {
                        int remainder = thumbnails.Length - ((rows - 1) * columns);
                        Debug.Log("Remainder: " + remainder);
                        x = remainder * thumbWidth + Mathf.Max(0f, remainder - 1) * galleryThumbPadding;
                        x = (width - x) / 2f;
                    }
                }
            }

            // Remove Padding
            if (column == 0)
            {
                y -= galleryThumbPadding;
            }
            // Add row height
            else
            {
                y += rowHeight;
            }

            // Add margin
            y += galleryThumbMargin;

            // Return height
            return y;
        }
        #endregion
    }
}
