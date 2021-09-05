using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RFB.Utilities
{
    // Swipe gestures
    public enum RFBSwipeGesture
    {
        None,
        Left,
        Right,
        Up,
        Down,
        DiagonalNE,
        DiagonalNW,
        DiagonalSE,
        DiagonalSW
    }

    // Checks for swipes
    public class RFBSwipeHandler : RFBInteractable, IDragHandler
    {
        [Header("Register Settings")]
        // Velicty lerp
        [Range(0f, 1f)]
        public float velocityLerp = 0.15f;
        // Min swipe distance (per inch)
        public float minSwipeVelocity = 10f;
        // Max swipe time
        public float maxSwipeTime = 1f;

        [Header("Type Settings")]
        // Horizontal swipes
        public bool registerHorizontal = true;
        // Vertical swipes
        public bool registerVertical = false;
        // Diagonal swipes
        public bool registerDiagonal = false;
        // Invert direction
        public bool invertDirection = false;

        // Swiped
        public Action<RFBSwipeGesture> onSwipe;

        // Check if swiping
        public bool isSwiping { get; private set; }
        // Velocity in inches per second
        public Vector2 swipeVelocity { get; private set; }
        // Start of swipe
        private float _touchStartTime = 0f;
        private Vector2 _touchStartPosition = Vector2.zero;

        // Update
        protected virtual void Update()
        {
            // Check for a swipe
            bool isMousePressed = Input.GetMouseButton(0) || Input.touchCount == 1;
            bool isBtnPressed = isMousePressed && interactiveState == RFBInteractiveState.Pressed;
            if (!isBtnPressed)
            {
                if (isSwiping)
                {
                    // Check if swipe was performed
                    float elapsed = Time.realtimeSinceStartup - _touchStartTime;
                    if (elapsed <= maxSwipeTime && !isMousePressed)
                    {
                        RFBSwipeGesture gesture = GetSwipeGesture(swipeVelocity);
                        if (gesture != RFBSwipeGesture.None)
                        {
                            //Debug.Log("Complete Swipe\nGesture: " + gesture.ToString());
                            if (onSwipe != null)
                            {
                                onSwipe(gesture);
                            }
                        }
                    }
                    else
                    {
                        //Debug.Log("Cancel Swipe\nToo Long: " + elapsed.ToString());
                    }

                    // Swipe complete
                    _touchStartTime = 0f;
                    _touchStartPosition = Vector2.zero;
                    swipeVelocity = Vector2.zero;
                    isSwiping = false;
                }
                return;
            }

            // Begin swiping
            if (!isSwiping)
            {
                isSwiping = true;
                _touchStartTime = Time.realtimeSinceStartup;
                _touchStartPosition = Input.mousePosition;
                swipeVelocity = Vector2.zero;
                return;
            }

            // Get mouse
            Vector2 mousePosition = Input.mousePosition;

            // Get velocity
            Vector2 velocity = (mousePosition - _touchStartPosition) * Time.deltaTime;
            // Multiply by density per inch
            velocity *= Screen.dpi;

            // Lerp
            if (velocityLerp < 1f)
            {
                velocity = Vector2.Lerp(swipeVelocity, velocity, velocityLerp);
            }

            // Set velocity
            swipeVelocity = velocity;
        }

        // Get swipe gesture for velocity
        private RFBSwipeGesture GetSwipeGesture(Vector2 velocity)
        {
            // Determine result
            RFBSwipeGesture result = RFBSwipeGesture.None;

            // Swipe velocity
            float magnitudeX = registerHorizontal ? Mathf.Abs(velocity.x) : 0f;
            float magnitudeY = registerVertical ? Mathf.Abs(velocity.y) : 0f;
            float magnitudeDiag = registerDiagonal ? Mathf.Abs(velocity.magnitude) : 0f;

            // Remove
            if (magnitudeX < magnitudeY || magnitudeX < magnitudeDiag)
            {
                magnitudeX = 0f;
            }
            if (magnitudeY < magnitudeX || magnitudeY < magnitudeDiag)
            {
                magnitudeY = 0f;
            }
            if (magnitudeDiag < magnitudeY || magnitudeDiag < magnitudeX)
            {
                magnitudeDiag = 0f;
            }

            // Horizontal swipe
            if (magnitudeX > minSwipeVelocity)
            {
                // Get positive
                bool pos = velocity.x > 0;
                if (invertDirection)
                {
                    pos = !pos;
                }
                // Set
                //Debug.Log("VAL: " + velocity.x);
                result = pos ? RFBSwipeGesture.Right : RFBSwipeGesture.Left;
            }
            // Vertical swipe
            else if (magnitudeY > minSwipeVelocity)
            {
                // Get positive
                bool pos = velocity.y > 0;
                if (invertDirection)
                {
                    pos = !pos;
                }
                // Set
                result = pos ? RFBSwipeGesture.Up : RFBSwipeGesture.Down;
            }
            // Diagonal swipe
            else if (magnitudeDiag > minSwipeVelocity)
            {
                // TODO
            }

            // Return
            return result;
        }

        // Drag
        public void OnDrag(PointerEventData eventData)
        {
            
        }
    }
}
