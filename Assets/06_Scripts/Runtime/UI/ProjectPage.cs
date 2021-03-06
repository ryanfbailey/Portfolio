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
        public string galleryBackgroundSwatchID;
        public Image galleryBackground;

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

        // Selected project
        public int selectedProject { get; private set; }

        // Awake
        protected override void Awake()
        {
            base.Awake();
            PortfolioManager.onProjectSelected += OnProjectSelected;
            PortfolioManager.onProjectGalleryItemSelected += OnGalleryItemSelected;
            websiteButton.onClick += WebsiteClick;
            galleryPrevButton.onClick += GalleryPrevClick;
            galleryNextButton.onClick += GalleryNextClick;
            galleryPlayButton.onClick += GalleryVideoPlay;
            galleryGesture.onSwipe += GallerySwipe;
        }
        // Destroy
        protected override void OnDestroy()
        {
            base.OnDestroy();
            PortfolioManager.onProjectSelected -= OnProjectSelected;
            PortfolioManager.onProjectGalleryItemSelected -= OnGalleryItemSelected;
            websiteButton.onClick -= WebsiteClick;
            galleryPrevButton.onClick -= GalleryPrevClick;
            galleryNextButton.onClick -= GalleryNextClick;
            galleryPlayButton.onClick -= GalleryVideoPlay;
            galleryGesture.onSwipe -= GallerySwipe;
        }

        // Load assets
        public override void Load()
        {
            base.Load();

            // Set background colors
            infoBackground.color = LayoutManager.instance.GetSwatchColor(infoBackgroundSwatchID);
            infoBackgroundGradient.color = LayoutManager.instance.GetSwatchColor(infoBackgroundGradientSwatchID);
            galleryBackground.color = LayoutManager.instance.GetSwatchColor(galleryBackgroundSwatchID);
            galleryLoadProgress.progressImage.color = LayoutManager.instance.GetSwatchColor(galleryLoaderSwatchID);

            // Set button
            websiteButton.SetMainText(LocalizationManager.instance.GetText("PROJECT_WEBSITE_BTN"));

            // Set label templates
            LayoutManager.instance.ApplyLabelSettings(titleLabel, titleTemplateID);
            LayoutManager.instance.ApplyLabelSettings(infoKeyLabel, infoKeyTemplateID);
            LayoutManager.instance.ApplyLabelSettings(infoValueLabel, infoValueTemplateID);
            LayoutManager.instance.ApplyLabelSettings(descriptionLabel, descriptionTemplateID);
            LayoutManager.instance.ApplyLabelSettings(galleryDescriptionLabel, galleryDescriptionTemplateID);
            LayoutManager.instance.ApplyLabelSettings(galleryTotalLabel, galleryTotalTemplateID);
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
            while (!string.IsNullOrEmpty(_galleryName))
            {
                yield return new WaitForEndOfFrame();
            }

            // Perform load
            yield return StartCoroutine(base.PerformAssetLoad());
        }
        #endregion

        #region SELECTION
        // Single section
        public int GetSectionCount()
        {
            return 1;
        }
        // No section prefab
        public RectTransform GetSectionPrefab(int sectionIndex)
        {
            return null;
        }
        // No section
        public Vector2 LoadSection(int sectionIndex, RectTransform section)
        {
            return Vector2.one * -1f;
        }

        // Cell per project
        public int GetCellCount(int sectionIndex)
        {
            return PortfolioManager.instance.projects.Length;
        }
        // Cell prefab
        public RectTransform GetCellPrefab(int sectionIndex, int cellIndex)
        {
            return selectTableCell.GetComponent<RectTransform>();
        }
        // Load cell
        public Vector2 LoadCell(int sectionIndex, int cellIndex, RectTransform cell)
        {
            // Only works since single cell
            int pIndex = selectTable.GetCellIndex(sectionIndex, cellIndex);

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

        // Select cell
        public void SelectCell(int sectionIndex, int cellIndex)
        {
            // Only works since single cell
            int pIndex = selectTable.GetCellIndex(sectionIndex, cellIndex);
            // Select
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
            bool hasGallery = project.gallery != null && project.gallery.Length > 0;
            galleryImageContainer.gameObject.SetActive(hasGallery);
            galleryTextContainer.gameObject.SetActive(hasGallery);

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
            bool hasGallery = galleryImageContainer.gameObject.activeSelf;

            // Use portrait if gallery is bigger than half
            float maxWidth = contentContainer.rect.width;
            float galWidth = maxWidth;
            bool isPortrait = true;
            /*
            float galWidth = galleryWidth;
            bool isPortrait = LayoutManager.instance.orientation == LayoutOrientation.Portrait;
            if (hasGallery || galWidth + galleryMargin > maxWidth * .65f)
            {
                isPortrait = true;
                galWidth = Mathf.Min(maxWidth, galWidth);
            }
            */

            // Add margin
            float padding = PageManager.instance.pageMinPadding;
            float leftHeight = PageManager.instance.pageMinPadding;

            // Add title
            float textWidth = maxWidth - (isPortrait ? 0f : galWidth + galleryMargin);
            float titleHeight = titleLabel.GetPreferredValues(titleLabel.text, textWidth, 10000f).y;
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, textWidth);
            titleLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, titleHeight);
            leftHeight += titleHeight + textPadding;

            // Add info key
            Vector2 infoKeySize = infoKeyLabel.GetPreferredValues(infoKeyLabel.text, 10000f, 10000f);
            infoKeyLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, infoKeySize.x);
            infoKeyLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, infoKeySize.y);

            // Add info value
            float infoValueWidth = textWidth - infoKeySize.x - infoMargin;
            infoValueLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, infoKeySize.x + infoMargin, infoValueWidth);
            infoValueLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, infoKeySize.y);
            leftHeight += infoKeySize.y + textPadding;

            // Add description
            float descriptionHeight = descriptionLabel.GetPreferredValues(descriptionLabel.text, textWidth, 10000f).y;
            descriptionLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, textWidth);
            descriptionLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, descriptionHeight);
            leftHeight += descriptionHeight;

            // Add website button
            if (hasWebsite)
            {
                websiteButton.ResizeWidth();
                float websiteWidth = websiteButton.rectTransform.rect.width;
                websiteButton.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, textWidth - websiteWidth, websiteWidth);
                leftHeight += websiteMargin;
                float websiteHeight = websiteButton.rectTransform.rect.height;
                websiteButton.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, leftHeight, websiteHeight);
                leftHeight += websiteHeight;
            }

            // Gallery
            float rightHeight = 0f;
            if (hasGallery)
            {
                // Adjust gallery image container
                rightHeight = isPortrait ? leftHeight + galleryMargin : padding;
                float galleryX = isPortrait ? (maxWidth - galWidth) / 2f : 0f;
                galleryImageContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, galleryX, galWidth);
                float galleryHeight = (galWidth - 40f) * (9f / 16f) + 40f;
                galleryImageContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, rightHeight, galleryHeight);
                rightHeight += galleryHeight;

                // Adjust gallery text
                float galleryTextHeight = galleryTextContainer.rect.height;
                galleryTextContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, galleryX, galWidth);
                galleryTextContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, rightHeight, galleryTextHeight);
                rightHeight += galleryTextHeight;
            }

            // Adjust container
            float infoHeight = Mathf.Max(leftHeight, rightHeight);
            infoContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, topHeight, infoHeight);

            // Set height
            return topHeight + infoHeight;
        }
        #endregion
    }
}
