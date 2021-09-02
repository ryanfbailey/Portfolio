using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RFB.Utilities
{
    public class LogLabel : MonoBehaviour
    {
        // Label
        public TextMeshProUGUI label;

        // Add delegate
        protected virtual void Awake()
        {
            if (label == null)
            {
                label = gameObject.GetComponent<TextMeshProUGUI>();
            }
            LogUtility.onStaticLogChange += StaticLogChange;
        }
        // Remove delegate
        protected virtual void OnDestroy()
        {
            LogUtility.onStaticLogChange -= StaticLogChange;
        }

        // Apply log
        protected virtual void StaticLogChange(string newLog)
        {
            if (label != null)
            {
                label.text = newLog;
            }
        }
    }
}
