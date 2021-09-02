using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RFB.Portfolio
{
    public class UVPatternAnimator : MonoBehaviour
    {
        [Header("UI Settings")]
        // Texture
        public Texture texture;
        // Icon
        public RawImage rawImage;

        // Setup
        protected virtual void Awake()
        {
            // Find image
            if (rawImage == null)
            {
                rawImage = gameObject.GetComponent<RawImage>();
                if (rawImage == null)
                {
                    rawImage = gameObject.AddComponent<RawImage>();
                    rawImage.color = Color.white;
                }
            }
            // Find texture
            if (texture == null && rawImage != null)
            {
                texture = rawImage.texture;
            }
            // Apply
            rawImage.texture = texture;
            // Add delegate
            LayoutManager.onCanvasSizeChange += OnCanvasSizeChange;
        }
        // Remove delegates
        protected virtual void OnDestroy()
        {
            LayoutManager.onCanvasSizeChange -= OnCanvasSizeChange;
        }

        #region RESIZE
        // Resize UVs
        protected virtual void OnCanvasSizeChange(float w, float h)
        {
            ResizeUVs();
        }

        // Resize
        public virtual void ResizeUVs()
        {
            // Set sizes
            _width = (float)Screen.width;
            _height = (float)Screen.height;

            // Adjust uvs
            if (rawImage != null && texture != null)
            {
                // Get rect
                Rect rect = rawImage.uvRect;

                // Update sizes
                rect.width = _width / (float)texture.width;
                rect.height = _height / (float)texture.height;

                // Apply rect
                rawImage.uvRect = rect;

                // Reposition
                RepositionUVs();
            }
        }
        #endregion

        #region ANIMATE
        [Header("Animation Settings")]
        // Percentage speed per second
        public Vector2 movementSpeed = new Vector2(-1f, 0f);

        // Offset
        public Vector2 uvOffset { get; private set; }
        // Sizing
        private float _width;
        private float _height;

        // Update
        protected virtual void Update()
        {
            // Ignore
            if (/*_width <= 0f || _height <= 0f || */movementSpeed == Vector2.zero)
            {
                return;
            }

            // Get
            Vector2 newOffset = uvOffset;

            // Animate X
            newOffset.x += (movementSpeed.x /*/ _width*/) * Time.deltaTime;
            // Animate y
            newOffset.y += (movementSpeed.y /*/ _height*/) * Time.deltaTime;

            // Set
            uvOffset = newOffset;
            RepositionUVs();
        }

        // Reposition
        public virtual void RepositionUVs()
        {
            if (rawImage != null)
            {
                // Get rect
                Rect rect = rawImage.uvRect;

                // Update offset
                rect.x = uvOffset.x;
                rect.y = uvOffset.y;

                // Apply rect
                rawImage.uvRect = rect;
            }
        }
        #endregion
    }
}
