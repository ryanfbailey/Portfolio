using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;
using TMPro;

namespace RFB.Portfolio
{
    public class RFBPButton : RFBButton
    {
        [Header("Layout Overrides")]
        // Default
        public string defaultLabelID;
        // Hover
        public string hoverLabelID;
        // Press
        public string pressLabelID;
        // Disabled
        public string disabledLabelID;
        // Selected
        public string selectedLabelID;
        // Graphic
        public Graphic[] tintGraphics;

        // Current id
        public string currentID { get; private set; }

        // On awake
        protected override void Awake()
        {
            base.Awake();
            SetLabelSettings(defaultLabelID);
        }

        // Refresh
        protected override void RefreshState()
        {
            base.RefreshState();

            // Get label id
            string newID = defaultLabelID;
            if (interactiveState == RFBInteractiveState.Disabled)
            {
                newID = disabledLabelID;
            }
            else if (isSelected)
            {
                newID = selectedLabelID;
            }
            else if (interactiveState == RFBInteractiveState.Pressed)
            {
                newID = pressLabelID;
            }
            else if (interactiveState == RFBInteractiveState.Hovered)
            {
                newID = hoverLabelID;
            }

            // Apply label settings
            SetLabelSettings(newID);
        }

        // Set id
        private void SetLabelSettings(string newID)
        {
            if (LayoutManager.instance != null && !string.Equals(currentID, newID, System.StringComparison.CurrentCultureIgnoreCase))
            {
                // Set id
                currentID = newID;

                // Set id
                foreach (TextMeshProUGUI label in labels)
                {
                    LayoutManager.instance.ApplyLabelSettings(label, currentID);
                }

                // Set color
                if (tintGraphics != null)
                {
                    string colorID = LayoutManager.instance.GetLabelSettings(currentID).labelSwatchID;
                    foreach (Graphic g in tintGraphics)
                    {
                        g.color = LayoutManager.instance.GetSwatchColor(colorID);
                    }
                }
            }
        }
    }
}
