using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RFB.Utilities
{
    public class ButtonHelper : Button
    {
        #region LIFECYCLE
        // On awake, set disable
        protected override void Awake()
        {
            base.Awake();
            SetDisabled(!interactable);
        }
        // On disable, remove hover & press
        protected override void OnDisable()
        {
            base.OnDisable();
            SetHover(false);
            SetPress(false);
        }
        #endregion

        #region HOVER
        // Hovered
        public bool isHovering { get; private set; }
        // On hover
        public Action<bool> onHoverChange;

        // Hover enter
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            SetHover(true);
        }
        // Hover exit
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            SetHover(false);
        }
        // Set hover
        protected virtual void SetHover(bool toHover)
        {
            if (isHovering != toHover)
            {
                // Apply
                isHovering = toHover;

                // Call delegate
                if (onHoverChange != null)
                {
                    onHoverChange(isHovering);
                }
            }
        }
        #endregion

        #region PRESS
        // Pressed
        public bool isPressing { get; private set; }
        // On hover
        public Action<bool> onPressChange;

        // Press down
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            SetPress(true);
        }
        // Press up
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            SetPress(false);
        }
        // Set press
        protected virtual void SetPress(bool toPress)
        {
            if (isPressing != toPress)
            {
                // Apply
                isPressing = toPress;

                // Call delegate
                if (onPressChange != null)
                {
                    onPressChange(isPressing);
                }
            }
        }
        #endregion

        #region DISABLE
        // Is disabled
        public bool isDisabled { get; private set; }
        // Whether changed
        public Action<bool> onDisableChange;

        // Apply
        public virtual void SetDisabled(bool toDisabled)
        {
            if (isDisabled != toDisabled)
            {
                // Apply
                isDisabled = toDisabled;

                // Apply
                interactable = !isDisabled;

                // Set
                if (onDisableChange != null)
                {
                    onDisableChange(isDisabled);
                }
            }
        }
        #endregion
    }
}
