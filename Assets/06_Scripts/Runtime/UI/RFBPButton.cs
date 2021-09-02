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
        // Default
        public string defaultLabelID;
        // Hover
        public string hoverLabelID;
        // Press
        public string pressLabelID;
        // Selected
        public string selectedLabelID;
        // Graphic
        public Graphic outlineGraphic;

        // Current id
        public string currentID { get; private set; }

        // On awake
        protected override void Awake()
        {
            base.Awake();
            SetLabelSettings(defaultLabelID);
        }

        // Refresh state
        protected override void RefreshState()
        {
            base.RefreshState();
            string newID = defaultLabelID;
            if (isSelected)
            {
                newID = selectedLabelID;
            }
            else if (!isDisabled && isPressed)
            {
                newID = pressLabelID;
            }
            else if (!isDisabled && isHovered)
            {
                newID = hoverLabelID;
            }
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
                foreach (TextMeshProUGUI label in mainLabels)
                {
                    LayoutManager.instance.ApplyLabelSettings(label, currentID);
                }

                // Set color
                if (outlineGraphic != null)
                {
                    string colorID = LayoutManager.instance.GetLabelSettings(currentID).labelSwatchID;
                    outlineGraphic.color = LayoutManager.instance.GetSwatchColor(colorID);
                }
            }
        }
    }
}
