using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        #region LIFECYCLE
        // Sheet ID
        public string sheetID = "localizations";
        // Language key
        public string languageKey = "key";

        // Load complete
        public bool isLoading { get; private set; }
        // Callback delegate
        public static event Action onLoadComplete;

        // Localization data
        private Dictionary<string, string>[] _localizations;

        // Add delegates
        protected override void Awake()
        {
            base.Awake();
            isLoading = true;
            currentLanguage = -1;
            AppManager.performLoad += WaitForLoad;
            GoogleSheetManager.onLoadComplete += SheetLoadComplete;
        }
        // Remove delegates
        protected override void OnDestroy()
        {
            base.OnDestroy();
            AppManager.performLoad -= WaitForLoad;
            GoogleSheetManager.onLoadComplete -= SheetLoadComplete;
        }
        // Wait for load
        private IEnumerator WaitForLoad()
        {
            while (isLoading)
            {
                yield return null;
            }
        }
        // Sheet load complete
        private void SheetLoadComplete(string newSheetID, Dictionary<string, string>[] newLocalizations)
        {
            if (!isLoading || !string.Equals(sheetID, newSheetID, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            // Done
            isLoading = false;

            // Get data
            _localizations = newLocalizations;
            if (_localizations == null)
            {
                Log("Load Failed", LogType.Error);
            }

            // Set language
            SetLanguage(0);

            // Load complete
            if (onLoadComplete != null)
            {
                onLoadComplete();
            }
        }
        // Log
        public override string GetLogTitle()
        {
            return "Localization Manager";
        }
        #endregion

        #region LANGUAGE
        // Language
        public int currentLanguage { get; private set; }
        // Language change
        public static event Action<int> onLanguageChange;

        // Get language index
        private int GetLanguageIndex(string languageID)
        {
            if (_localizations != null)
            {
                for (int i = 0; i < _localizations.Length; i++)
                {
                    Dictionary<string, string> dict = _localizations[i];
                    if (dict != null && dict.ContainsKey(languageKey))
                    {
                        string id = dict[languageKey];
                        if (id.Equals(languageID, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        // Set language
        public void SetLanguage(string languageID)
        {
            int newIndex = GetLanguageIndex(languageID);
            SetLanguage(newIndex);
        }
        // Set language index
        public void SetLanguage(int newIndex)
        {
            // Reset index
            if (_localizations == null || newIndex < 0 || newIndex >= _localizations.Length)
            {
                newIndex = -1;
            }

            // Set localization
            if (currentLanguage != newIndex)
            {
                currentLanguage = newIndex;
                if (onLanguageChange != null)
                {
                    onLanguageChange(currentLanguage);
                }
            }
        }
        #endregion

        #region TEXT
        // Get text
        public string GetText(string textID, int languageIndex = -1)
        {
            // Use current
            if (languageIndex < 0)
            {
                languageIndex = currentLanguage;
            }

            // Get localization
            if (_localizations != null && languageIndex >= 0 && languageIndex < _localizations.Length)
            {
                Dictionary<string, string> dict = _localizations[languageIndex];
                if (!dict.ContainsKey(textID))
                {
                    Log("Missing Text ID\nText ID: " + textID + "\nLanguage Index: " + languageIndex, LogType.Warning);
                }
                else
                {
                    return dict[textID];
                }
            }
            // Invalid
            else
            {
                Log("Invalid Language\nnLanguage Index: " + languageIndex, LogType.Error);
            }

            // Return nothing
            return "";
        }
        #endregion
    }
}
