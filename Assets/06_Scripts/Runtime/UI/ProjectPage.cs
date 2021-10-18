using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class ProjectPage : Page, RFBTableDelegate
    {
        #region LIFECYCLE
        [Header("Project Select Settings")]
        // Selection default icon
        public Texture2D selectDefaultIcon;
        // Selection Table
        public RFBPButton selectTableCell;
        // Selection Table
        public RFBTable selectTable;

        // Backgrounds
        [Header("Project Background Settings")]
        public string infoBackgroundSwatchID;
        public Image infoBackground;
        public string infoBackgroundGradientSwatchID;
        public Image infoBackgroundGradient;
        //public string galleryBackgroundSwatchID;
        //public Image galleryBackground;

        // Info
        [Header("Project Info Settings")]
        public RectTransform infoContainer;
        public string titleTemplateID;
        public TextMeshProUGUI titleLabel;
        public float infoMargin = 10f;
        public string infoKeyTemplateID;
        public TextMeshProUGUI infoKeyLabel;
        public string infoValueTemplateID;
        public TextMeshProUGUI infoValueLabel;
        public string descriptionTemplateID;
        public TextMeshProUGUI descriptionLabel;
        public float websiteMargin = 50f;
        public RFBPButton websiteButton;

        // Gallery
        [Header("Project Gallery Settings")]
        public float galleryMargin = 50f;
        public ProjectGalleryThumb galleryThumbCell;
        public RFBTable galleryTable;
        /*
        public float galleryWidth = 300f;
        public RectTransform galleryImageContainer;
        public RectTransform galleryTextContainer;
        public string galleryDescriptionTemplateID;
        public TextMeshProUGUI galleryDescriptionLabel;
        public string galleryTotalTemplateID;
        public TextMeshProUGUI galleryTotalLabel;
        public RawImage galleryImage;

        // Gallery Buttons
        public RFBPButton galleryPrevButton;
        public RFBPButton galleryNextButton;
        public RFBSwipeHandler galleryGesture;

        // Loader
        [Header("Project Gallery Loader")]
        public string galleryLoaderSwatchID;
        public RFBProgressBar galleryLoadProgress;

        // Video
        [Header("Project Video Settings")]
        public RFBPButton galleryPlayButton;
        */

        // Selected project
        public int selectedProject { get; private set; }

        // Awake
        protected override void Awake()
        {
            base.Awake();
            PortfolioManager.onProjectSelected += OnProjectSelected;
            //PortfolioManager.onProjectGalleryItemSelected += OnGalleryItemSelected;
            websiteButton.onClick += WebsiteClick;
            /*
            galleryPrevButton.onClick += GalleryPrevClick;
            galleryNextButton.onClick += GalleryNextClick;
            galleryPlayButton.onClick += GalleryVideoPlay;
            galleryGesture.onSwipe += GallerySwipe;
            */
        }
        // Destroy
        protected override void OnDestroy()
        {
            base.OnDestroy();
            PortfolioManager.onProjectSelected -= OnProjectSelected;
            //PortfolioManager.onProjectGalleryItemSelected -= OnGalleryItemSelected;
            websiteButton.onClick -= WebsiteClick;
            /*
            galleryPrevButton.onClick -= GalleryPrevClick;
            galleryNextButton.onClick -= GalleryNextClick;
            galleryPlayButton.onClick -= GalleryVideoPlay;
            galleryGesture.onSwipe -= GallerySwipe;
            */
        }

        // Load assets
        public override void Load()
        {
            base.Load();

            // Set background colors
            infoBackground.color = LayoutManager.instance.GetSwatchColor(infoBackgroundSwatchID);
            infoBackgroundGradient.color = LayoutManager.instance.GetSwatchColor(infoBackgroundGradientSwatchID);
            //galleryBackground.color = LayoutManager.instance.GetSwatchColor(galleryBackgroundSwatchID);
            //galleryLoadProgress.progressImage.color = LayoutManager.instance.GetSwatchColor(galleryLoaderSwatchID);

            // Set button
            websiteButton.SetMainText(LocalizationManager.instance.GetText("PROJECT_WEBSITE_BTN"));

            // Set label templates
            LayoutManager.instance.ApplyLabelSettings(titleLabel, titleTemplateID);
            LayoutManager.instance.ApplyLabelSettings(infoKeyLabel, infoKeyTemplateID);
            LayoutManager.instance.ApplyLabelSettings(infoValueLabel, infoValueTemplateID);
            LayoutManager.instance.ApplyLabelSettings(descriptionLabel, descriptionTemplateID);
            /*
            LayoutManager.instance.ApplyLabelSettings(galleryDescriptionLabel, galleryDescriptionTemplateID);
            LayoutManager.instance.ApplyLabelSettings(galleryTotalLabel, galleryTotalTemplateID);
            */
        }

        // Perform load
        protected override IEnumerator PerformAssetLoad()
        {
            // Wait for portfolio
            while (PortfolioManager.instance.isLoading)
            {
                yield return new WaitForEndOfFrame();
            }

            // Load
            selectTable.LoadTable(this);
            selectTable.SelectCell(0, PortfolioManager.instance.selectedProject);

            // Wait for gallery
            while (FileUtility.AreFilesLoading())
            {
                yield return new WaitForEndOfFrame();
            }

            // Perform load
            yield return StartCoroutine(base.PerformAssetLoad());
        }
        #endregion

        #region TABLE
        // Single section
        public int GetSectionCount(RFBTable table)
        {
            return 1;
        }
        // No section prefab
        public RectTransform GetSectionPrefab(RFBTable table, int sectionIndex)
        {
            return null;
        }
        // No section
        public Vector2 LoadSection(RFBTable table, int sectionIndex, RectTransform section)
        {
            return Vector2.one * -1f;
        }

        // Cell per project
        public int GetCellCount(RFBTable table, int sectionIndex)
        {
            if (table == selectTable)
            {
                return PortfolioManager.instance.projects.Length;
            }
            else if (table == galleryTable)
            {
                return GetTotalGalleryThumbs();
            }
            return 0;
        }
        // Cell prefab
        public RectTransform GetCellPrefab(RFBTable table, int sectionIndex, int cellIndex)
        {
            if (table == selectTable)
            {
                return selectTableCell.GetComponent<RectTransform>();
            }
            else if (table == galleryTable)
            {
                return galleryThumbCell.GetComponent<RectTransform>();
            }
            return null;
        }
        // Load cell
        public Vector2 LoadCell(RFBTable table, int sectionIndex, int cellIndex, RectTransform cell)
        {
            if (table == selectTable)
            {
                int pIndex = selectTable.GetCellIndex(sectionIndex, cellIndex);
                return LoadProjectCell(cell, pIndex);
            }
            else if (table == galleryTable)
            {
                return LoadGalleryThumb(cell, cellIndex);
            }
            return Vector2.zero;
        }
        // Select cell
        public void SelectCell(RFBTable table, int sectionIndex, int cellIndex)
        {
            if (table == selectTable)
            {
                int pIndex = selectTable.GetCellIndex(sectionIndex, cellIndex);
                SelectProject(pIndex);
            }
            else if (table == galleryTable)
            {

            }
        }
        #endregion

        #region SELECTION
        // Load cell
        private Vector2 LoadProjectCell(RectTransform cell, int pIndex)
        {
            // Get data
            ProjectData project = PortfolioManager.instance.projects[pIndex];

            // Get image
            RawImage image = cell.GetComponentInChildren<RawImage>(true);
            if (image != null)
            {
                // Apply image
                image.texture = project.icon == null ? selectDefaultIcon : project.icon;
                // Adjust color
                image.color = image.texture == null ? Color.clear : Color.white;
            }

            // Return full stretch
            return cell.rect.size;
        }
        // Select a project
        private void SelectProject(int pIndex)
        {
            PortfolioManager.instance.SelectProject(pIndex);
        }
        #endregion

        #region INFO
        // Project selected
        private void OnProjectSelected(int newIndex)
        {
            // Invalid
            if (PortfolioManager.instance.projects == null || newIndex < 0 || newIndex >= PortfolioManager.instance.projects.Length)
            {
                return;
            }

            // Get data
            ProjectData project = PortfolioManager.instance.projects[newIndex];

            // Set title
            titleLabel.text = project.title;
            titleLabel.characterSpacing = -5f;

            // Set info key & value
            string infoKey = "";
            string infoValue = "";
            Dictionary<string, string> infoData = GetProjectInfo(project);
            foreach (string key in infoData.Keys)
            {
                // New Line
                if (!string.IsNullOrEmpty(infoKey))
                {
                    infoKey += "\n";
                    infoValue += "\n";
                }
                // Add info
                infoKey += key + ": ";
                infoValue += infoData[key];
            }
            infoKeyLabel.text = infoKey;
            infoValueLabel.text = infoValue;

            // Set description
            descriptionLabel.text = "\n" + project.description + "\n\n" + project.contribution;

            // Adjust website
            bool hasWebsite = !string.IsNullOrEmpty(project.website);
            websiteButton.gameObject.SetActive(hasWebsite);

            // Adjust gallery
            galleryTable.LoadTable(this);
            galleryTable.SelectCell(0, 0);

            // Select cell
            selectTable.SelectCell(0, newIndex);
            selectTable.RefreshCellSelections();

            // Resize
            PageManager.instance.ResizePages();
        }
        // Get info
        private Dictionary<string, string> GetProjectInfo(ProjectData project)
        {
            // Begin
            Dictionary<string, string> result = new Dictionary<string, string>();

            // Add Company
            string key = LocalizationManager.instance.GetText("PROJECT_COMPANY_" + project.type.ToString().ToUpper());
            result[key] = project.company;

            // Add Role
            key = LocalizationManager.instance.GetText("PROJECT_ROLE");
            result[key] = project.role;

            // Add Release
            key = LocalizationManager.instance.GetText("PROJECT_RELEASE");
            result[key] = project.release;

            // Add Skills
            key = LocalizationManager.instance.GetText("PROJECT_SKILLS");
            result[key] = project.skills.Replace(",", ", ");

            // Add Platforms
            key = LocalizationManager.instance.GetText("PROJECT_PLATFORMS");
            result[key] = project.platforms.Replace(",", ", ");

            // Return
            return result;
        }
        // Click website
        private void WebsiteClick()
        {
            // Get project index
            int projectIndex = PortfolioManager.instance.selectedProject;
            if (PortfolioManager.instance.projects == null || projectIndex < 0 || projectIndex >= PortfolioManager.instance.projects.Length)
            {
                return;
            }

            // Get website url
            ProjectData p = PortfolioManager.instance.projects[projectIndex];
            string url = p.website;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            // Apply
            PortfolioManager.instance.OpenWebURL(url);
        }
        #endregion

        #region GALLERY
        // Get gallery
        private GalleryItemData[] GetCurrentGalleryItems()
        {
            // No selected project
            int projectIndex = PortfolioManager.instance.selectedProject;
            if (PortfolioManager.instance.projects == null || projectIndex < 0 || projectIndex >= PortfolioManager.instance.projects.Length)
            {
                return null;
            }

            // No project gallery
            ProjectData project = PortfolioManager.instance.projects[projectIndex];
            if (project.gallery == null || project.gallery.Length == 0)
            {
                return null;
            }

            // Return gallery
            return project.gallery;
        }
        // Get total current gallery items
        private int GetTotalGalleryThumbs()
        {
            GalleryItemData[] items = GetCurrentGalleryItems();
            return items == null ? 0 : items.Length;
        }
        // Load gallery thumb
        private Vector2 LoadGalleryThumb(RectTransform cell, int gIndex)
        {
            // Items not found
            GalleryItemData[] items = GetCurrentGalleryItems();
            if (items == null || gIndex < 0 || gIndex >= items.Length)
            {
                return Vector2.zero;
            }

            // Load thumb
            GalleryItemData thumbData = items[gIndex];
            ProjectGalleryThumb thumb = cell.GetComponent<ProjectGalleryThumb>();
            if (thumb != null)
            {
                thumb.Load(thumbData);
            }

            // Return container height as square
            float size = galleryTable.rectTransform.rect.height;
            return new Vector2(size, size);
        }
        /*
        // Placeholder image
        public Texture2D placeholderImage;
        // Last gallery thumb
        private string _galleryName;
        private Texture2D _galleryThumb;

        // Update gallery image
        private void OnGalleryItemSelected(int galleryIndex)
        {
            // Get project index
            int projectIndex = PortfolioManager.instance.selectedProject;
            if (PortfolioManager.instance.projects == null || projectIndex < 0 || projectIndex >= PortfolioManager.instance.projects.Length)
            {
                return;
            }

            // Fill amount
            galleryLoadProgress.SetProgress(0f);
            galleryLoadProgress.gameObject.SetActive(false);

            // Destroy previous
            if (_galleryThumb != null)
            {
                Destroy(_galleryThumb);
            }

            // Set to placeholder
            SetGalleryTexture(placeholderImage);

            // Get gallery
            ProjectData project = PortfolioManager.instance.projects[projectIndex];
            if (project.gallery == null || galleryIndex < 0 || galleryIndex >= project.gallery.Length)
            {
                Log("Invalid Gallery Index\nProject: " + project.projectID + "\nIndex: " + galleryIndex);
                return;
            }

            // Get item
            GalleryItemData galleryItem = project.gallery[galleryIndex];

            // Set description
            galleryDescriptionLabel.text = galleryItem.title;
            // Set total
            galleryTotalLabel.text = (galleryIndex + 1).ToString() + " of " + (project.gallery.Length).ToString();

            // Prev/Next Button
            galleryPrevButton.SetDisabled(galleryIndex <= 0);
            galleryNextButton.SetDisabled(galleryIndex >= project.gallery.Length - 1);

            // Has URL
            bool hasURL = !string.IsNullOrEmpty(galleryItem.videoURL);
            galleryPlayButton.gameObject.SetActive(hasURL);

            // Load image
            _galleryName = galleryItem.thumbName;
            galleryLoadProgress.gameObject.SetActive(true);
            PortfolioManager.instance.LoadImage(_galleryName, delegate (Texture2D t)
            {
                // Too Late
                if (!string.Equals(galleryItem.thumbName, _galleryName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    Log("Gallery Image Load - Too Late\nName: " + galleryItem.thumbName, LogType.Warning);
                    if (t != null)
                    {
                        Destroy(t);
                    }
                }
                // Apply
                else
                {
                    _galleryName = "";
                    if (t == null)
                    {
                        Log("Gallery Image Load - Null Texture\nName: " + galleryItem.thumbName, LogType.Warning);
                        SetGalleryTexture(placeholderImage);
                    }
                    else
                    {
                        //Log("Gallery Image Load - Success!\nName: " + galleryItem.thumbName);
                        _galleryThumb = t;
                        SetGalleryTexture(_galleryThumb);
                    }
                }
            }, delegate(float progress)
            {
                if (string.Equals(galleryItem.thumbName, _galleryName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    galleryLoadProgress.SetProgress(progress);
                }
            });
        }
        // Hide
        private void Update()
        {
            if (galleryLoadProgress.gameObject.activeSelf && galleryLoadProgress.value == 1f)
            {
                galleryLoadProgress.gameObject.SetActive(false);
            }
        }
        // Apply
        private void SetGalleryTexture(Texture2D newTexture)
        {
            // Set texture
            galleryImage.texture = newTexture;

            // Hide
            if (galleryImage.texture == null)
            {
                galleryImage.gameObject.SetActive(false);
                return;
            }

            // Show
            galleryImage.gameObject.SetActive(true);

            // Get parent rect, parent aspect & image aspect
            RectTransform parentRect = galleryImage.transform.parent.GetComponent<RectTransform>();
            float parentAspect = (float)parentRect.rect.width / (float)parentRect.rect.height;
            float imageAspect = (float)newTexture.width / (float)newTexture.height;

            // Width/Height
            float width;
            float height;
            // Scale to width
            if (imageAspect > parentAspect)
            {
                width = parentRect.rect.width;
                height = width / imageAspect;
            }
            // Scale to height
            else
            {
                height = parentRect.rect.height;
                width = height * imageAspect;
            }

            // Set image
            RectTransform imageRect = galleryImage.GetComponent<RectTransform>();
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        // Previous selected
        private void GalleryPrevClick()
        {
            GalleryIterate(false);
        }
        // Next selected
        private void GalleryNextClick()
        {
            GalleryIterate(true);
        }
        // Swipe gesture
        private void GallerySwipe(RFBSwipeGesture swipeGesture)
        {
            if (swipeGesture == RFBSwipeGesture.Right)
            {
                GalleryIterate(false);
            }
            else if (swipeGesture == RFBSwipeGesture.Left)
            {
                GalleryIterate(true);
            }
        }
        // Play
        private void GalleryVideoPlay()
        {
            // Get project index
            int projectIndex = PortfolioManager.instance.selectedProject;
            if (PortfolioManager.instance.projects == null || projectIndex < 0 || projectIndex >= PortfolioManager.instance.projects.Length)
            {
                return;
            }

            // Get gallery
            ProjectData project = PortfolioManager.instance.projects[projectIndex];
            int galleryIndex = PortfolioManager.instance.selectedGalleryItem;
            if (project.gallery == null || galleryIndex < 0 || galleryIndex >= project.gallery.Length)
            {
                return;
            }

            // Gallery item
            GalleryItemData galleryItem = project.gallery[galleryIndex];
            string url = galleryItem.videoURL;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            // Open URL
            PortfolioManager.instance.OpenWebURL(url);
        }
        // Iterate
        private void GalleryIterate(bool toNext)
        {
            // Get project index
            int projectIndex = PortfolioManager.instance.selectedProject;
            if (PortfolioManager.instance.projects == null || projectIndex < 0 || projectIndex >= PortfolioManager.instance.projects.Length)
            {
                return;
            }

            // Get gallery
            ProjectData project = PortfolioManager.instance.projects[projectIndex];
            if (project.gallery == null)
            {
                return;
            }

            // Iterate
            int galleryIndex = PortfolioManager.instance.selectedGalleryItem + (toNext ? 1 : -1);
            galleryIndex = Mathf.Clamp(galleryIndex, 0, project.gallery.Length - 1);

            // Apply
            PortfolioManager.instance.SelectGalleryItem(galleryIndex);
        }
        */
        #endregion

        #region RESIZE
        // Project page
        public override float Resize()
        {
            // Get default height
            float topHeight = base.Resize();

            // Adjust table
            float tableHeight = selectTable.rectTransform.rect.height;
            selectTable.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, topHeight, tableHeight);
            topHeight += tableHeight;
            selectTable.RefreshCanScroll();

            // Check sections
            bool hasWebsite = websiteButton.gameObject.activeSelf;
            bool hasGallery = galleryTable.totalCellsPerSection != null;

            // Determine text sizing
            float maxWidth = contentContainer.rect.width;
            Vector2 titleSize = titleLabel.GetPreferredValues(titleLabel.text, maxWidth, 10000f);
            Vector2 infoKeySize = infoKeyLabel.GetPreferredValues(infoKeyLabel.text, maxWidth, 10000f);
            Vector2 infoValueSize = infoValueLabel.GetPreferredValues(infoValueLabel.text, maxWidth - infoKeySize.x - infoMargin, 10000f);
            float textWidth = Mathf.Max(titleSize.x, infoKeySize.x + infoMargin + infoValueSize.x);
            float textHeight = titleSize.y + textPadding + Mathf.Max(infoKeySize.y, infoValueSize.y);

            // Determine portrait mode & gallery width
            bool isPortrait = textWidth + infoMargin > maxWidth * .5f;
            float galleryWidth = maxWidth + (isPortrait ? 0 : -textWidth - infoMargin);

            // Add margin
            float padding = PageManager.instance.pageMinPadding;
            float leftHeight = padding;

            // Add title
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, textWidth);
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, titleSize.y);
            leftHeight += titleSize.y + textPadding;

            // Add info key
            infoKeyLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, infoKeySize.x);
            infoKeyLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, infoKeySize.y);

            // Add info value
            infoValueLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, infoKeySize.x + infoMargin, infoValueSize.x);
            infoValueLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, infoValueSize.y);
            leftHeight += Mathf.Max(infoKeySize.y, infoValueSize.y) + textPadding;

            // Add description
            float descriptionHeight = descriptionLabel.GetPreferredValues(descriptionLabel.text, maxWidth, 10000f).y;
            descriptionLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, maxWidth);
            descriptionLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, descriptionHeight);
            leftHeight += descriptionHeight;

            // Add website button
            if (hasWebsite)
            {
                websiteButton.ResizeWidth();
                float websiteWidth = websiteButton.rectTransform.rect.width;
                websiteButton.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, websiteWidth);
                leftHeight += websiteMargin;
                float websiteHeight = websiteButton.rectTransform.rect.height;
                websiteButton.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, websiteHeight);
                leftHeight += websiteHeight;
            }

            // Gallery
            float rightHeight = isPortrait ? leftHeight : padding;
            if (hasGallery)
            {
                // Add margin
                if (isPortrait)
                {
                    rightHeight += galleryMargin;
                }
                float galleryHeight = textHeight;
                galleryTable.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0f, galleryWidth);
                bool needsRefresh = (galleryTable.rectTransform.rect.height != galleryHeight);
                galleryTable.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, rightHeight, galleryHeight);
                rightHeight += galleryHeight;

                // Reload
                if (needsRefresh)
                {
                    galleryTable.LoadTable(this);
                }
            }

            // Adjust container
            float totalHeight = Mathf.Max(leftHeight, rightHeight) + padding;
            infoContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, topHeight, totalHeight);

            // Set height
            return topHeight + totalHeight;
        }
        #endregion
    }
}
