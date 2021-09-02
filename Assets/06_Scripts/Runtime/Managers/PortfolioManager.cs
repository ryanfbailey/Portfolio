using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class PortfolioManager : Singleton<PortfolioManager>
    {
        #region SETUP
        [Header("Google Sheet Data")]
        // Project sheet id
        public string projectSheetID = "projects";
        // Gallery data
        public string gallerySheetID = "gallery";

        [Header("Image Settings")]
        // Image directory
        public string imageDirectory = "";
        // Icon postfix
        public string iconPostfix = "_icon.jpg";

        // Project data
#if UNITY_EDITOR
        public ProjectData[] projects;
#else
        public ProjectData[] projects { get; private set; }
#endif
        // Projects loaded
        public static event Action onProjectsLoaded;

        // Whether or not loading
        public bool isLoading { get; private set; }

        // Highlighted project
        public string projectParam = "project";
        public string highlightedProjectID { get; private set; }
        // Display type
        public string displayParam = "display";
        public DisplayType displayLevel { get; private set; }

        // Currently selected project
        public int selectedProject { get; private set; }
        // Project selected
        public static event Action<int> onProjectSelected;

        // Select gallery item
        public int selectedGalleryItem { get; private set; }
        // Gallery selected
        public static event Action<int> onProjectGalleryItemSelected;

        // Add delegates
        protected override void Awake()
        {
            base.Awake();
            isLoading = true;
            selectedProject = -1;
            selectedGalleryItem = -1;
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
        private void SheetLoadComplete(string newSheetID, Dictionary<string, string>[] newSheetData)
        {
            if (!isLoading)
            {
                return;
            }
            // Decode projects
            if (string.Equals(projectSheetID, newSheetID, StringComparison.CurrentCultureIgnoreCase))
            {
                DecodeProjects(newSheetData);
            }
            // Decode gallery
            else if (string.Equals(gallerySheetID, newSheetID, StringComparison.CurrentCultureIgnoreCase))
            {
                DecodeGallery(newSheetData);
            }
        }
        // Decode projects
        private void DecodeProjects(Dictionary<string, string>[] newProjectData)
        {
            // Decode
            ProjectData[] projectData = CsvUtility.Decode<ProjectData>(newProjectData);
            if (projectData == null)
            {
                Log("Project Load Failed - Failed to decode", LogType.Error);
                return;
            }

            // Highlighted project if applicable

            // URL parameters
            Dictionary<string, string> urlParams = WebGLUtility.GetWebParameters();

            // Determine highlighted project if applicable
            highlightedProjectID = "";
            if (urlParams.ContainsKey(projectParam))
            {
                highlightedProjectID = urlParams[projectParam];
            }

            // Determine display level
            displayLevel = DisplayType.Live;
#if UNITY_EDITOR
            displayLevel = DisplayType.QA;
#endif
            if (urlParams.ContainsKey(displayParam))
            {
                DisplayType newDisplay;
                string displayVal = urlParams[displayParam];
                if (!Enum.TryParse<DisplayType>(displayVal, true, out newDisplay))
                {
                    Log("Project Display Level Invalid\nValue: " + displayVal, LogType.Warning);
                }
                else
                {
                    displayLevel = newDisplay;
                }
            }

            // Add projects
            List<string> imageURLs = new List<string>();
            List<ProjectData> projectList = new List<ProjectData>();
            for (int i = 0; i < projectData.Length; i++)
            {
                // Ignore if hidden
                ProjectData datum = projectData[i];
                if ((int)datum.display < (int)displayLevel && !string.Equals(highlightedProjectID, datum.projectID, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                // Add datum
                projectList.Add(datum);
            }

            // Sort
            projectList.Sort(delegate (ProjectData p1, ProjectData p2)
            {
                // Is highlighted
                bool h1 = string.Equals(highlightedProjectID, p1.projectID, StringComparison.CurrentCultureIgnoreCase);
                bool h2 = string.Equals(highlightedProjectID, p2.projectID, StringComparison.CurrentCultureIgnoreCase);
                if (h1 != h2)
                {
                    return h1 ? -1 : 1;
                }
                // Index
                int c = p1.index.CompareTo(p2.index);
                // Invert
                c = -c;
                return c;
            });

            // Set data
            projects = projectList.ToArray();
        }
        // Decode gallery
        private void DecodeGallery(Dictionary<string, string>[] newGalleryData)
        {
            // Decode
            GalleryItemData[] galleryData = CsvUtility.Decode<GalleryItemData>(newGalleryData);
            if (galleryData == null)
            {
                Log("Gallery Load Failed - Failed to decode", LogType.Error);
                return;
            }
            if (projects == null)
            {
                Log("Gallery Load Failed - Failed to find projects", LogType.Error);
                return;
            }

            // Add Gallery
            Dictionary<int, List<GalleryItemData>> dictionary = new Dictionary<int, List<GalleryItemData>>();
            for (int g = 0; g < galleryData.Length; g++)
            {
                // Gallery data
                GalleryItemData galleryItem = galleryData[g];

                // Project index
                int projIndex = GetProjectIndex(galleryItem.projectID);
                if (projIndex == -1)
                {
                    continue;
                }

                // Create new list
                if (!dictionary.ContainsKey(projIndex))
                {
                    dictionary[projIndex] = new List<GalleryItemData>();
                }

                // Add to dictionary
                dictionary[projIndex].Add(galleryItem);
            }

            // Set to projects
            for (int p = 0; p < projects.Length; p++)
            {
                if (dictionary.ContainsKey(p))
                {
                    List<GalleryItemData> items = dictionary[p];
                    projects[p].gallery = items.ToArray();
                }
            }

            // Next
            LoadIcon(0);
        }
        // Load icon
        private void LoadIcon(int projectIndex)
        {
            // Complete
            if (projectIndex >= projects.Length)
            {
                LoadComplete();
                return;
            }

            // Project
            ProjectData project = projects[projectIndex];

            // Load icon
            LoadImage(project.projectID.ToLower() + iconPostfix, delegate (Texture2D t)
            {
                // Apply
                projects[projectIndex].icon = t;

                // Next
                LoadIcon(projectIndex + 1);
            });
        }
        // Load complete
        private void LoadComplete()
        {
            // Done loading
            isLoading = false;

            // Complete
            Log("Projects: " + projects.Length);

            // Call delegates
            if (onProjectsLoaded != null)
            {
                onProjectsLoaded();
            }

            // Select project
            SelectProject(0);
        }
        // Log
        public override string GetLogTitle()
        {
            return "Portfolio Utility";
        }
        #endregion

#region SELECTION
        // Select project
        public void SelectProject(int newIndex)
        {
            // Already set
            if (selectedProject == newIndex)
            {
                return;
            }

            // Set
            selectedProject = newIndex;

            // Apply
            if (onProjectSelected != null)
            {
                onProjectSelected(selectedProject);
            }

            // Select item
            selectedGalleryItem = -1;
            SelectGalleryItem(0);
        }
        // Select gallery item
        public void SelectGalleryItem(int newIndex)
        {
            // Already set
            if (selectedGalleryItem == newIndex)
            {
                return;
            }

            // Set
            selectedGalleryItem = newIndex;

            // Apply
            if (onProjectGalleryItemSelected != null)
            {
                onProjectGalleryItemSelected(selectedGalleryItem);
            }
        }
        #endregion

        #region HELPERS
        // Open from text ID
        public void OpenWebLocalizedURL(string textID)
        {
            // Ignore
            string url = LocalizationManager.instance.GetText(textID);
            if (string.IsNullOrEmpty(url))
            {
                Log("Invalid URL Text ID: " + textID, LogType.Error);
            }
            // Open
            else
            {
                OpenWebURL(url);
            }
        }
        // Open URL
        public void OpenWebURL(string url)
        {
            WebGLUtility.OpenWebURL(url);
        }
        // Get project index from id
        public int GetProjectIndex(string projectID)
        {
            if (projects != null)
            {
                for (int p = 0; p < projects.Length; p++)
                {
                    ProjectData project = projects[p];
                    if (string.Equals(projectID, project.projectID, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return p;
                    }
                }
            }
            return -1;
        }
        #endregion

        #region IMAGE
        // Load image
        public void LoadImage(string iconName, Action<Texture2D> onComplete, Action<float> onProgress = null)
        {
            // Icon URL
            string iconURL = imageDirectory + iconName;

            // Add https
            string absoluteURL = Application.absoluteURL;
            if (!string.IsNullOrEmpty(absoluteURL) && absoluteURL.ToLower().StartsWith("https"))
            {
                iconURL = "https://" + iconURL;
            }
            // Add http
            else
            {
                iconURL = "http://" + iconURL;
            }

            // Icon load texture
            FileUtility.LoadTexture(iconURL, delegate (Texture2D t)
            {
                // Error
                if (t == null)
                {
                    Log("Image Load Failed\nURL: " + iconURL, LogType.Error);
                }

                // Delegate
                if (onComplete != null)
                {
                    onComplete(t);
                }

            }, true, onProgress);
        }
        #endregion
    }
}
