using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    [Serializable]
    public struct GoogleSheetData
    {
        // Sheet ID
        public string sheetID;
        // File name
        public string fileName;
        // Google Tab ID
        public string googleTabID;
        // Google Sheet ID
        public string googleSheetID;
        // Key is first row vs first column
        public bool keyIsFirstRow;
    }

    public class GoogleSheetManager : Singleton<GoogleSheetManager>
    {
        #region STATIC
        // Get sheet url
        public static string GetSheetURL(string googleSheetID, string googleTabID)
        {
            string url = "https://docs.google.com/spreadsheets/d/";
            url += googleSheetID;
            url += "/gviz/tq?";
            url += "tqx=out:csv";
            url += "&sheet=" + googleTabID;
            return url;
        }

        // Log
        private void Log(string comment, bool error = false)
        {
            //LogUtility.Log(comment, "Google Sheet Utility", error);
        }
        #endregion

        #region LIFECYCLE
        // Download
        public bool downloadAtRuntime = true;
        // Google Sheets
        public GoogleSheetData[] sheets;

        // Whether loaded or not
        public bool isLoading { get; private set; }
        // Once all are loaded
        public static event Action<string, Dictionary<string, string>[]> onLoadComplete;

        // Awake
        protected override void Awake()
        {
            base.Awake();
            AppManager.performLoad += WaitForLoad;
            Load();
        }
        // Destroy
        protected override void OnDestroy()
        {
            base.OnDestroy();
            AppManager.performLoad -= WaitForLoad;
        }
        #endregion

        #region LOAD
        // Wait for load
        private IEnumerator WaitForLoad()
        {
            while (isLoading)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        // Load
        private void Load()
        {
            StartCoroutine(PerformSheetLoad());
        }
        // Perform download
        private IEnumerator PerformSheetLoad()
        {
            // Begin Loading
            isLoading = true;

#if UNITY_EDITOR
            // Download
            downloadAtRuntime = true;
#endif

            // Load all sheets
            if (sheets != null)
            {
                for (int i = 0; i < sheets.Length; i++)
                {
                    // Get sheet
                    GoogleSheetData sheet = sheets[i];

                    // Contents
                    string sheetContents = "";

                    // Perform download
                    if (downloadAtRuntime)
                    {
                        // Download
                        bool loading = true;
                        string sheetURL = GetSheetURL(sheet.googleSheetID, sheet.googleTabID);
                        Log("Download Begin\nSheet URL: " + sheetURL);
                        FileUtility.LoadText(sheetURL, delegate (string r)
                        {
                            sheetContents = r;
                            loading = false;
                        }, true);
                        while (loading)
                        {
                            yield return null;
                        }

                        // Failed
                        if (string.IsNullOrEmpty(sheetContents))
                        {
                            Log("Download Failed\nSheet URL: " + sheetURL, true);
                        }
#if UNITY_EDITOR
                        // Save to streaming assets
                        else
                        {
                            string sheetPath = GetSheetPath(sheet);
                            Log("Save Begin\nSheet Path: " + sheetPath);
                            try
                            {
                                File.WriteAllText(sheetPath, sheetContents);
                            }
                            catch (Exception e)
                            {
                                Log("Save Failed\nSheet Path: " + sheetPath + "\nException: " + e.Message, true);
                            }
                        }
#endif
                    }

                    // Not downloaded, load from streaming
                    if (string.IsNullOrEmpty(sheetContents))
                    {
                        // Begin loading
                        bool loading = true;
                        string sheetPath = GetSheetPath(sheet);
                        Log("Load Begin\nSheet Path: " + sheetPath);
                        FileUtility.LoadText(sheetPath, delegate (string r)
                        {
                            sheetContents = r;
                            loading = false;
                        }, true);
                        while (loading)
                        {
                            yield return null;
                        }

                        // Failed
                        if (string.IsNullOrEmpty(sheetContents))
                        {
                            Log("Load Failed\nSheet Path: " + sheetPath, true);
                            continue;
                        }
                    }

                    // CSV Parse
                    Log("CSV Parse Begin\nSheet: " + sheet.sheetID);
                    Dictionary<string, string>[] dictionaries = null;
                    try
                    {
                        dictionaries = CsvUtility.Decode(sheetContents, sheet.keyIsFirstRow);
                    }
                    catch (Exception e)
                    {
                        Log("CSV Parse Failed\nSheet: " + sheet.sheetID + "\nException: " + e.Message, true);
                        continue;
                    }

                    // Load callback
                    Log("Sheet Loaded\nSheet: " + sheet.sheetID + "\nDictionaries: " + dictionaries.Length);
                    if (onLoadComplete != null)
                    {
                        onLoadComplete(sheet.sheetID, dictionaries);
                    }
                }
            }

            // Complete
            Log("Load Complete\nSheets: " + sheets.Length);
            isLoading = false;
        }

        // Get sheet path
        private string GetSheetPath(GoogleSheetData sheet)
        {
            return Application.streamingAssetsPath + "/" + sheet.fileName + ".csv";
        }
        #endregion
    }
}
