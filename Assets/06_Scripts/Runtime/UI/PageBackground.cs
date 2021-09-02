using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class PageBackground : MonoBehaviour
    {
        // Awake
        private void Awake()
        {
            SetLoaded(false);
            AppManager.onLoadComplete += LoadComplete;
        }
        // Loaded
        private void LoadComplete()
        {
            SetLoaded(true);
        }
        // Destroy
        private void OnDestroy()
        {
            AppManager.onLoadComplete -= LoadComplete;
        }

        // Adjust based on loaded
        private void SetLoaded(bool toLoaded)
        {
            // Move to front
            if (toLoaded)
            {
                transform.SetAsFirstSibling();
            }
            // Move to back
            else
            {
                transform.SetAsLastSibling();
            }
        }
    }
}
