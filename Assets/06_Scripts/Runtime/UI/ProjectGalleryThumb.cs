using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public enum ImageScale
    {
        Stretch,
        Actual,
        AspectFill,
        AspectFit,
        StretchWidth,
        StretchHeight
    }

    public class ProjectGalleryThumb : RFBPButton
    {
        [Header("Thumbnail Settings")]
        // Resize type
        public ImageScale thumbResize = ImageScale.AspectFill;
        // Icon
        public RawImage thumbIcon;
        // Loading bar
        public RFBProgressBar thumbLoader;

        // Image url
        private Texture _texture;
        private string _imageID;

        // Unload
        public void Unload()
        {
            if (_texture != null)
            {
                DestroyImmediate(_texture);
                _texture = null;
            }
            thumbIcon.texture = null;
        }
        // Set data
        public void Load(GalleryItemData newItemData)
        {
            // Set active
            thumbLoader.gameObject.SetActive(true);

            // Set data
            string thumbID = newItemData.thumbName;
            _imageID = thumbID;

            // Load
            PortfolioManager.instance.LoadImage(newItemData.thumbName, delegate (Texture2D t)
            {
                if (t != null)
                {
                    // Destroy
                    if (!string.Equals(_imageID, thumbID, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        DestroyImmediate(t);
                    }
                    // Apply
                    else
                    {
                        Apply(t);
                    }
                }
            }, delegate (float p)
            {
                thumbLoader.SetProgress(p);
            });
        }
        // Apply
        public void Apply(Texture2D t)
        {
            // Done
            _imageID = "";

            // Set texture
            _texture = t;
            thumbIcon.texture = _texture;

            // Layout
            Layout(rectTransform.rect.width);

            // Disable loader
            thumbLoader.gameObject.SetActive(false);
        }
        // Make square
        public float Layout(float width)
        {
            // Keep square
            float height = width;

            // Resize image
            ResizeView(new Vector2(width, height), thumbIcon, thumbResize);

            // Return width
            return height;
        }

        // Resize
        public static void ResizeView(Vector2 parentSize, RawImage image, ImageScale imageScale)
        {
            if (image.texture == null)
            {
                return;
            }

            // Get ratios
            float parentRatio = parentSize.x / parentSize.y;
            float textureRatio = (float)image.texture.width / (float)image.texture.height;
            bool pOverT = parentRatio > textureRatio;

            // Default to stretch
            float width = parentSize.x;
            float height = parentSize.y;
            // Set to texture size
            if (imageScale == ImageScale.Actual)
            {
                width = image.texture.width;
                height = image.texture.height;
            }
            // Keep width
            else if (imageScale == ImageScale.StretchWidth || (imageScale == ImageScale.AspectFill && pOverT) || (imageScale == ImageScale.AspectFit && !pOverT))
            {
                height = width / textureRatio;
            }
            // Keep height
            else if (imageScale == ImageScale.StretchHeight || (imageScale == ImageScale.AspectFill && !pOverT) || (imageScale == ImageScale.AspectFit && pOverT))
            {
                width = height * textureRatio;
            }

            // Apply
            //Debug.Log("Set Size\nWidth: " + width + "\nHeight: " + height);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
