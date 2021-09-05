using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    // Interactive state
    public enum RFBInteractiveState
    {
        Default,
        Hovered,
        Pressed,
        Disabled
    }

    // Toggle group of objects that only show during specific states
    [Serializable]
    public struct RFBInteractiveStateToggleGroup
    {
        public string id;
        public bool invert;
        public RFBInteractiveState[] states;
        public GameObject[] objects;
    }

    // Interactable
    public class RFBInteractable : MonoBehaviour
    {
        [Header("State Settings")]
        // GameObject Groups
        public RFBInteractiveStateToggleGroup[] toggleGroups;

        // Interactive state
        public RFBInteractiveState interactiveState { get; private set; }
        // On interactive state change
        public Action<RFBInteractiveState> onInteractiveStateChange;

        // Transform
        public RectTransform rectTransform { get; private set; }
        // Button Helper
        public ButtonHelper btnHelper { get; private set; }

        // On awake, get helper
        protected virtual void Awake()
        {
            // Get rect
            rectTransform = gameObject.GetComponent<RectTransform>();
            // Get helper
            btnHelper = gameObject.GetComponent<ButtonHelper>();
            if (btnHelper == null)
            {
                btnHelper = gameObject.AddComponent<ButtonHelper>();
                //Debug.Log("Added: " + gameObject.name);
            }
            // Disable transition
            btnHelper.transition = UnityEngine.UI.Selectable.Transition.None;
            // Add delegates
            btnHelper.onHoverChange += OnHoverChange;
            btnHelper.onPressChange += OnPressChange;
            btnHelper.onDisableChange += OnDisableChange;
        }
        // Set state
        protected virtual void Start()
        {
            SetState(RFBInteractiveState.Default);
        }
        // On destroy
        protected virtual void OnDestroy()
        {
            if (btnHelper != null)
            {
                btnHelper.onHoverChange -= OnHoverChange;
                btnHelper.onPressChange -= OnPressChange;
                btnHelper.onDisableChange -= OnDisableChange;
            }
        }
        // Set button
        public virtual void SetDisabled(bool toDisabled)
        {
            btnHelper.SetDisabled(toDisabled);
        }

        // Hovered
        protected virtual void OnHoverChange(bool toPressed)
        {
            RefreshState();
        }
        // Pressed
        protected virtual void OnPressChange(bool toPressed)
        {
            RefreshState();
        }
        // Disabled
        protected virtual void OnDisableChange(bool toPressed)
        {
            RefreshState();
        }
        // Refresh State
        protected virtual void RefreshState()
        {
            // State
            RFBInteractiveState newState = RFBInteractiveState.Default;

            // Disabled
            if (btnHelper.isDisabled)
            {
                newState = RFBInteractiveState.Disabled;
            }
            // Pressed
            else if (btnHelper.isPressing)
            {
                newState = RFBInteractiveState.Pressed;
            }
            // Hovered
            else if (btnHelper.isHovering)
            {
                newState = RFBInteractiveState.Hovered;
            }

            // If changed
            if (interactiveState != newState)
            {
                SetState(newState);
            }
        }
        // Set state
        protected virtual void SetState(RFBInteractiveState newState)
        {
            // Set state
            interactiveState = newState;

            // Refresh groups
            RefreshToggleGroups();

            // Delegate
            if (onInteractiveStateChange != null)
            {
                onInteractiveStateChange(interactiveState);
            }
        }
        // Update all groups
        protected void RefreshToggleGroups()
        {
            if (toggleGroups != null)
            {
                foreach (RFBInteractiveStateToggleGroup group in toggleGroups)
                {
                    RefreshToggleGroup(group);
                }
            }
        }
        // Update group
        protected void RefreshToggleGroup(RFBInteractiveStateToggleGroup group)
        {
            // Get should
            bool shouldShow = ShouldToggleGroupShow(group);

            // Apply
            foreach (GameObject obj in group.objects)
            {
                if (obj.activeSelf != shouldShow)
                {
                    obj.SetActive(shouldShow);
                }
            }
        }
        // Determine if should show group
        public virtual bool ShouldToggleGroupShow(RFBInteractiveStateToggleGroup group)
        {
            // Default to hidden
            bool result = false;

            // Check for valid state
            if (group.states != null)
            {
                foreach (RFBInteractiveState s in group.states)
                {
                    if (s == interactiveState)
                    {
                        result = true;
                        break;
                    }
                }
            }

            // Invert
            if (group.invert)
            {
                result = !result;
            }

            // Return result
            return result;
        }
    }
}
