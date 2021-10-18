using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RFB.Utilities
{
    public static class FileUtility
    {
        #region REQUEST
        // Default web request timeout
        public const int DEFAULT_TIMEOUT = 5;

        // Load async or not
        private static bool ShouldAsync(string url, bool wantsAsync)
        {
            // Always async for http
            if (url.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            // Editor file load, not async
            else if (!Application.isPlaying)
            {
                return false;
            }

#if UNITY_WEBGL || UNITY_ANDROID
            // Always async
            return true;
#else
            // Do what user wants
            return wantsAsync;
#endif
        }

        // Load url with callback
        public static void Load(string url, Action<UnityWebRequest> onComplete, Action<float> onProgress = null)
        {
            // Must start with http or file
            if (!url.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                url = "http://" + url;
#else
                url = "file://" + url;
#endif
            }

            // Log url
            //Log("LOAD FILE\nURL: " + url);

            // Load url
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = DEFAULT_TIMEOUT;
            Load(request, onComplete, onProgress);
        }
        // Load request with callback
        public static void Load(UnityWebRequest request, Action<UnityWebRequest> onComplete, Action<float> onProgress = null)
        {
            FileHelper helper = new GameObject("FILE_HELPER").AddComponent<FileHelper>();
            helper.LoadRequest(request, onComplete, onProgress);
        }
        #endregion

        #region FILE
        // Load bytes
        public static void LoadBytes(string url, Action<byte[]> onBytesLoaded, bool wantsAsync = false, Action<float> onBytesProgress = null)
        {
            // Determine if should load async
            bool shouldAsync = ShouldAsync(url, wantsAsync);

            // Load async
            if (shouldAsync)
            {
                Load(url, delegate (UnityWebRequest request)
                {
                    // Create result
                    byte[] result = null;

                    // Success
                    if (request.isDone && !request.isHttpError && !request.isNetworkError)
                    {
                        if (request.downloadHandler == null)
                        {
                            Log("LOAD BYTES ASYNC FAILED\nERROR: No download handler found\nURL: " + url, LogType.Error);
                        }
                        else
                        {
                            result = request.downloadHandler.data;
                        }
                    }

                    // Return result
                    if (onBytesLoaded != null)
                    {
                        onBytesLoaded(result);
                    }
                }, onBytesProgress);
            }
            // Load sync
            else
            {
                // Create result
                byte[] result = null;

                try
                {
                    // Load
                    result = File.ReadAllBytes(url);
                }
                catch (Exception e)
                {
                    Log("LOAD BYTES FAILED\nPATH: " + url + "\nERROR: " + e.Message, LogType.Error);
                }

                // Return result
                if (onBytesLoaded != null)
                {
                    onBytesLoaded(result);
                }
            }
        }
        #endregion

        #region TEXT
        // Load text
        public static void LoadText(string url, Action<string> onTextLoaded, bool wantsAsync = false, Action<float> onTextProgress = null)
        {
            // Determine if should load async
            bool shouldAsync = ShouldAsync(url, wantsAsync);

            // Load async
            if (shouldAsync)
            {
                Load(url, delegate (UnityWebRequest request)
                {
                    // Create result
                    string result = "";

                    // Success
                    if (request.isDone && !request.isHttpError && !request.isNetworkError)
                    {
                        if (request.downloadHandler == null)
                        {
                            Log("LOAD TEXT FAILED\nERROR: No download handler found\nURL: " + url, LogType.Error);
                        }
                        else
                        {
                            result = request.downloadHandler.text;
                        }
                    }

                    // Return result
                    if (onTextLoaded != null)
                    {
                        onTextLoaded(result);
                    }
                }, onTextProgress);
            }
            // Load sync
            else
            {
                // Create result
                string result = "";

                try
                {
                    // Load
                    result = File.ReadAllText(url);
                }
                catch (Exception e)
                {
                    Log("LOAD TEXT FAILED\nPATH: " + url + "\nERROR: " + e.Message, LogType.Error);
                }

                // Return result
                if (onTextProgress != null)
                {
                    onTextProgress(1f);
                }
                if (onTextLoaded != null)
                {
                    onTextLoaded(result);
                }
            }
        }
        #endregion

        #region TEXTURE
        // Load texture
        public static void LoadTexture(string url, Action<Texture2D> onTextureLoad, bool wantsAsync = false, Action<float> onTextureProgress = null)
        {
            LoadBytes(url, delegate (byte[] bytes)
            {
                // Get result
                Texture2D result = null;

                // Has bytes
                if (bytes != null)
                {
                    try
                    {
                        // Parse
                        result = new Texture2D(2, 2, url.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ? TextureFormat.RGB24 : TextureFormat.ARGB32, false);
                        result.filterMode = FilterMode.Bilinear;
                        result.wrapMode = TextureWrapMode.Clamp;
                        result.LoadImage(bytes);
                        result.Apply();
                    }
                    catch (Exception e)
                    {
                        Log("PARSE TEXTURE FAILED\nURL: " + url + "\nERROR: " + e.Message, LogType.Error);
                    }
                }

                // Return result
                if (onTextureLoad != null)
                {
                    onTextureLoad(result);
                }
            }, wantsAsync, onTextureProgress);
        }
        #endregion

        #region JSON
        // Load json
        public static void LoadJson<T>(string url, Action<T> onJsonParsed, bool wantsAsync = false)
        {
            LoadText(url, delegate (string text)
            {
                // Get result
                T result = default(T);

                // Has text
                if (!string.IsNullOrEmpty(text))
                {
                    Log("PARSE JSON\nURL: " + url + "\nTEXT: " + text);
                    try
                    {
                        // Parse
                        result = JsonUtility.FromJson<T>(text);
                    }
                    catch (Exception e)
                    {
                        Log("PARSE JSON FAILED\nURL: " + url + "\nTEXT: " + text + "\nERROR: " + e.Message, LogType.Error);
                    }
                }

                // Return result
                if (onJsonParsed != null)
                {
                    onJsonParsed(result);
                }
            }, wantsAsync);
        }
        #endregion

        #region HELPERS
        // Current
        public static int currentLoaders = 0;
#if UNITY_WEBGL
        // Total concurrent at a time
        public const int TOTAL_CONCURRENT = 1;
#else
        public const int TOTAL_CONCURRENT = 2;
#endif

        // Loading
        public static bool AreFilesLoading()
        {
            return currentLoaders > 0;
        }

        // Log
        private static void Log(string log, LogType type = LogType.Log)
        {
            LogUtility.Log(log, "File Utility", type);
        }


        // Class for watching requests
        [ExecuteInEditMode]
        public class FileHelper : MonoBehaviour
        {
            // Web request
            private UnityWebRequest _request;
            // On complete delegate
            private Action<UnityWebRequest> _onComplete;
            // On progress
            private Action<float> _onProgress;

            //
            public bool isLoading { get; private set; }

            // Dont destroy
            private void Awake()
            {
                //DontDestroyOnLoad(gameObject);
            }

            // Load url with request
            public void LoadRequest(UnityWebRequest request, Action<UnityWebRequest> onComplete, Action<float> onProgress)
            {
                // Set request
                _request = request;
                // Set on complete
                _onComplete = onComplete;
                // Set on progress
                _onProgress = onProgress;

                // Begin loading
                StartCoroutine(WaitToBegin());
            }

            // Wait
            private IEnumerator WaitToBegin()
            {
                // Wait
                while (currentLoaders >= TOTAL_CONCURRENT)
                {
                    yield return new WaitForEndOfFrame();
                }

                // Update
                currentLoaders++;
                isLoading = true;

                // Begin loading
                _request.SendWebRequest();
            }

            // Update
            private void Update()
            {
                // Ignore without request
                if (_request == null)
                {
                    return;
                }

                // Call delegate
                if (_onProgress != null)
                {
                    float progress = Mathf.Max(_request.uploadProgress, _request.downloadProgress);
                    //Debug.Log(_request.url + "\nDLP: " + _request.downloadProgress + "\nULP: " + _request.uploadProgress);
                    _onProgress(progress);
                }

                // Load complete
                if (_request.isDone)
                {
                    LoadComplete(true);
                }
            }

            // Load complete
            private void LoadComplete(bool destroy)
            {
                // Complete
                if (isLoading)
                {
                    currentLoaders--;
                    isLoading = false;
                }

                // Early
                if (!_request.isDone)
                {
                    Log("REQUEST ABORTED\nURL: " + _request.url, LogType.Error);
                    _request.Abort();
                }

                // Errors
                if (_request.isHttpError)
                {
                    Log("REQUEST HTTP ERROR\nURL: " + _request.url + "\nERROR: " + _request.error, LogType.Error);
                }
                if (_request.isNetworkError)
                {
                    Log("REQUEST NETWORK ERROR\nURL: " + _request.url + "\nERROR: " + _request.error, LogType.Error);
                }

                // Complete
                if (_onProgress != null)
                {
                    _onProgress(1f);
                    _onProgress = null;
                }

                // Call on complete delegate
                if (_onComplete != null)
                {
                    _onComplete(_request);
                    _onComplete = null;
                }

                // Dispose request
                _request.Dispose();
                _request = null;

                // Destroy
                if (destroy)
                {
                    DestroyImmediate(gameObject);
                }
            }

            // Destroy
            private void OnDestroy()
            {
                if (_request != null)
                {
                    LoadComplete(false);
                }
            }
        }
#endregion
    }
}
