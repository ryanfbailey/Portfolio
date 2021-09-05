using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RFB.Utilities
{
    public class RFBButton : RFBInteractable
    {
        #region LIFECYCLE
        // Disable tint
        public float disableTint = 0.4f;
        // Selected object
        public CanvasGroup canvasGroup;

        [Header("Label Settings")]
        // Auto resize
        public bool autoResizeWidth = false;
        // Label padding
        public float labelPadding = 10f;
        // Labels
        public TextMeshProUGUI[] labels;

        [Header("Selection Settings")]
        // Selectable
        public bool selectButton = false;
        // Selected object
        public GameObject selectedObject;
        // Selected
        public bool isSelected { get; private set; }

        // On Click
        public Action onClick;
        // Select change
        public Action<bool> onSelectChange;

        // On awake
        protected override void Awake()
        {
            base.Awake();
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            if (selectButton)
            {
                isSelected = true;
                SetSelected(false);
            }
        }
        #endregion

        #region BUTTONS
        // On Enable
        protected virtual void OnEnable()
        {
            btnHelper.onClick.AddListener(BtnClick);
        }
        // On Disable
        protected virtual void OnDisable()
        {
            btnHelper.onClick.RemoveListener(BtnClick);
        }
        // Check click
        public virtual void BtnClick()
        {
            // Set select
            if (selectButton)
            {
                SetSelected(true);
            }
            // Click
            else
            {
                if (onClick != null)
                {
                    onClick();
                }
            }
        }
        // Set selected
        public virtual void SetSelected(bool toSelected)
        {
            // Skip
            if (!selectButton || isSelected == toSelected)
            {
                return;
            }

            // Set
            isSelected = toSelected;

            // Toggle
            if (selectedObject != null)
            {
                selectedObject.SetActive(isSelected);
            }

            // Refresh
            RefreshState();

            // Set
            if (onSelectChange != null)
            {
                onSelectChange(isSelected);
            }
        }
        // Refresh state
        protected override void SetState(RFBInteractiveState newState)
        {
            // Base
            base.SetState(newState);

            // Canvas group
            if (canvasGroup != null)
            {
                canvasGroup.alpha = interactiveState == RFBInteractiveState.Disabled ? disableTint : 1f;
            }
        }
        #endregion

        #region LABEL
        // Apply all text
        public void SetMainText(string newText)
        {
            // Set label
            if (labels != null)
            {
                foreach (TextMeshProUGUI label in labels)
                {
                    label.text = newText;
                }
            }
            // Auto resize
            if (autoResizeWidth)
            {
                ResizeWidth();
            }
        }

        // Resize width
        public void ResizeWidth()
        {
            // Set label
            if (labels != null)
            {
                // Get width
                float width = 0f;
                foreach (TextMeshProUGUI label in labels)
                {
                    width = Mathf.Max(width, label.preferredWidth);
                }
                // Add padding
                width += labelPadding * 2f;
                // Set width
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }
        #endregion
    }
}
