using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    public static class LogUtility
    {
        #region LOG
        // Logging Assist
        public static void Log(string comment, string category, LogType type = LogType.Log)
        {
            string full = category + " " + type.ToString();
            full += " - " + comment;
            if (type == LogType.Error)
            {
                Debug.LogError(full);
            }
            else if (type == LogType.Warning)
            {
                Debug.LogWarning(full);
            }
            else
            {
                Debug.Log(full);
            }
        }
        #endregion

        #region STATIC
        // Log
        public static string staticLog { get; private set; }
        public static event Action<string> onStaticLogChange;

        // Used locally
        private static Dictionary<string, string> _staticPairs = new Dictionary<string, string>();

        // Log static
        public static void LogStatic(string key, string val)
        {
            // Ignore for null key
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            // Check for null val
            bool nullVal = string.IsNullOrEmpty(val);

            // Exists
            if (_staticPairs.ContainsKey(key))
            {
                if (nullVal)
                {
                    _staticPairs.Remove(key);
                    RefreshStatic();
                }
                else
                {
                    string oldVal = _staticPairs[key];
                    if (!oldVal.Equals(val))
                    {
                        _staticPairs[key] = val;
                        RefreshStatic();
                    }
                }
            }
            // New
            else if (!nullVal)
            {
                _staticPairs[key] = val;
                RefreshStatic();
            }
        }

        // Refresh
        private static void RefreshStatic()
        {
            // Build
            string newLog = "";
            foreach (string key in _staticPairs.Keys)
            {
                newLog += "\n" + key + ": " + _staticPairs[key];
            }

            // Apply
            staticLog = newLog;
            if (onStaticLogChange != null)
            {
                onStaticLogChange(staticLog);
            }
        }
        #endregion
    }
}
