using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RFB.Utilities
{
    public static class UIUtility
    {
        // Get child transform
        public static RectTransform GetChildTransform(RectTransform parent, string newName)
        {
            // Get new child
            GameObject newChild = new GameObject(newName);
            // Setup Transform
            RectTransform newRect = newChild.AddComponent<RectTransform>();
            newRect.SetParent(parent);
            newRect.localPosition = Vector3.zero;
            newRect.localRotation = Quaternion.identity;
            newRect.localScale = Vector3.one;
            // Setup RectTransform
            newRect.anchorMin = Vector2.zero;
            newRect.anchorMax = Vector2.one;
            newRect.pivot = Vector2.one * 0.5f;
            newRect.anchoredPosition = Vector2.zero;
            newRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent.rect.width);
            newRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parent.rect.height);
            // Return
            return newRect;
        }
        // Get child image
        public static Image GetChildImage(RectTransform parent, string newName)
        {
            Image newImage = GetChildTransform(parent, newName).gameObject.AddComponent<Image>();
            newImage.color = Color.white;
            return newImage;
        }
        // Get child raw image
        public static RawImage GetChildRawImage(RectTransform parent, string newName)
        {
            RawImage newImage = GetChildTransform(parent, newName).gameObject.AddComponent<RawImage>();
            newImage.color = Color.white;
            return newImage;
        }
        // Get child label
        public static TextMeshProUGUI GetChildLabel(RectTransform parent, string newName)
        {
            TextMeshProUGUI newLabel = GetChildTransform(parent, newName).gameObject.AddComponent<TextMeshProUGUI>();
            newLabel.color = Color.white;
            newLabel.raycastTarget = false;
            return newLabel;
        }
    }
}
