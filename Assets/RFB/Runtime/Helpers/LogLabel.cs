using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RFB.Utilities
{
    public class LogLabel : MonoBehaviour
    {
        // URL key
        public string urlKey = "/qa";
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
        // Start
        protected virtual void Start()
        {
            // Should
            bool shouldShow = Application.absoluteURL.Contains(urlKey);
#if UNITY_EDITOR
            shouldShow = true;
#endif

            // Adjust
            gameObject.SetActive(shouldShow);
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
