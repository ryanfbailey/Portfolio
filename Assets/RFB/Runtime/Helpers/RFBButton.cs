using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RFB.Utilities
{
    public class RFBButton : Button
    {
        #region HELPERS
        [Header("Button Settings")]
        // Default object
        public GameObject defaultObject;
        // Hover object
        public GameObject hoverObject;
        // Press object
        public GameObject pressObject;
        // Disabled object
        public GameObject disabledObject;
        // Selected object
        public GameObject selectedObject;

        [Header("Group Settings")]
        // Disable tint
        public float disableTint = 0.4f;
        // Selected object
        public CanvasGroup canvasGroup;

        // Hovered
        public bool isHovered { get; private set; }
        // Pressed
        public bool isPressed { get; private set; }

        // Selected
        public bool isDisabled { get; private set; }
        // Selected
        public bool isSelected { get; private set; }
        // Select change
        public static event Action<RFBButton, bool> onSelectChange;

        // Selectable
        public bool selectButton = false;

        // On awake
        protected override void Awake()
        {
            base.Awake();
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // On Enable
        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshState();
            onClick.AddListener(BtnClick);
        }
        // On Disable
        protected override void OnDisable()
        {
            base.OnDisable();
            onClick.RemoveListener(BtnClick);
            isHovered = false;
            isPressed = false;
            RefreshState();
        }
        // Check click
        private void BtnClick()
        {
            if (selectButton)
            {
                SetSelected(true);
            }
        }
        // Disable
        public void SetDisabled(bool toDisabled)
        {
            isDisabled = toDisabled;
            interactable = !toDisabled;
            RefreshState();
        }
        // Hover enter
        public override void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            base.OnPointerEnter(eventData);
            RefreshState();
        }
        // Hover exit
        public override void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            base.OnPointerExit(eventData);
            RefreshState();
        }

        // Press down
        public override void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            base.OnPointerDown(eventData);
            RefreshState();
        }
        // Press up
        public override void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            base.OnPointerUp(eventData);
            RefreshState();
        }
        // Set selected
        public void SetSelected(bool toSelected)
        {
            // Skip
            if (!selectButton || isSelected == toSelected)
            {
                return;
            }

            // Set
            isSelected = toSelected;

            // Refresh
            RefreshState();

            // Set
            if (onSelectChange != null)
            {
                onSelectChange(this, isSelected);
            }
        }
        // Refresh state
        protected virtual void RefreshState()
        {
            if (defaultObject != null)
            {
                defaultObject.SetActive((!isHovered && !isPressed && !isSelected || !interactable));
            }
            if (pressObject != null)
            {
                pressObject.SetActive(interactable && isPressed);
            }
            if (hoverObject != null)
            {
                hoverObject.SetActive(interactable && isHovered);
            }
            if (disabledObject != null)
            {
                disabledObject.SetActive(!interactable);
            }
            if (selectedObject != null)
            {
                selectedObject.SetActive(isSelected);
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = interactable ? 1f : disableTint;
            }
        }
        #endregion

        #region RESIZING
        // Main labels
        public TextMeshProUGUI[] mainLabels;
        // Label ideal padding
        public float labelPreferredPaddingX;

        // Apply all text
        public void SetMainText(string newText)
        {
            if (mainLabels != null)
            {
                foreach (TextMeshProUGUI label in mainLabels)
                {
                    label.text = newText;
                }
            }
        }

        // Get preferred width
        public float GetPreferredWidth()
        {
            float idealWidth = 0f;
            if (mainLabels != null)
            {
                foreach (TextMeshProUGUI label in mainLabels)
                {
                    idealWidth = Mathf.Max(idealWidth, label.preferredWidth);
                    //Debug.Log(label.text + " Pref: " + label.preferredWidth);
                }
            }
            return idealWidth + labelPreferredPaddingX * 2f;
        }

        // Set
        public void SetPreferredWidth()
        {
            float prefWidth = GetPreferredWidth();
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, prefWidth);
        }
        #endregion
    }
}
