using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RFB.Utilities
{
    // Delegate
    public interface RFBTableDelegate
    {
        // Section
        int GetSectionCount();
        RectTransform GetSectionPrefab(int sectionIndex);
        Vector2 LoadSection(int sectionIndex, RectTransform section);

        // Cells
        int GetCellCount(int sectionIndex);
        RectTransform GetCellPrefab(int sectionIndex, int cellIndex);
        Vector2 LoadCell(int sectionIndex, int cellIndex, RectTransform cell);

        // Select cell
        void SelectCell(int sectionIndex, int cellIndex);
    }

    // Table
    public class RFBTable : RFBMonoBehaviour
    {
        #region SETUP
        [Header("Main Settings")]
        // Container
        public RectTransform container;
        // Rect transform
        public RectTransform rectTransform { get; private set; }
        // Scroll rect
        public ScrollRect scroller { get; private set; }

        // On awake
        protected virtual void Awake()
        {
            // Find rect
            rectTransform = gameObject.GetComponent<RectTransform>();
            // Find scroller
            scroller = gameObject.GetComponent<ScrollRect>();
            if (scroller == null)
            {
                scroller = gameObject.AddComponent<ScrollRect>();
            }
            scroller.content = container;

            // Container missing
            if (container == null)
            {
                Log("No container found", LogType.Error);
            }
        }
        // On destroy
        protected virtual void OnDestroy()
        {
            // Unload table
            UnloadTable();
        }
        #endregion

        #region LOAD
        // Table delegate
        public RFBTableDelegate tableDelegate { get; private set; }
        // Sections
        public int totalSections { get; private set; }
        public RectTransform[] sectionTransforms { get; private set; }
        private Vector2[] _sectionSizes;
        // Cells
        public int[] totalCellsPerSection { get; private set; }
        public RectTransform[] cellTransforms { get; private set; }
        private Vector2[] _cellSizes;

        // Unload table
        public void UnloadTable()
        {
            // Null delegate
            tableDelegate = null;

            // Unload sections
            totalSections = 0;
            if (sectionTransforms != null)
            {
                foreach (RectTransform section in sectionTransforms)
                {
                    if (section != null)
                    {
                        RFBPool.instance.Unload(section.gameObject);
                    }
                }
                sectionTransforms = null;
            }

            // Unload cells
            totalCellsPerSection = null;
            if (cellTransforms != null)
            {
                foreach (RectTransform cell in cellTransforms)
                {
                    if (cell != null)
                    {
                        // Unload
                        RFBPool.instance.Unload(cell.gameObject);
                    }
                }
                cellTransforms = null;
            }
        }

        // Load table
        public void LoadTable(RFBTableDelegate newTableDelegate)
        {
            // Unload
            UnloadTable();

            // Done
            tableDelegate = newTableDelegate;
            if (tableDelegate == null)
            {
                return;
            }

            // Get section count
            totalSections = tableDelegate.GetSectionCount();
            sectionTransforms = new RectTransform[totalSections];
            _sectionSizes = new Vector2[totalSections];

            // Get cell items
            totalCellsPerSection = new int[totalSections];
            List<RectTransform> cellList = new List<RectTransform>();
            List<Vector2> cellSizes = new List<Vector2>();

            // Load sections
            for (int s = 0; s < totalSections; s++)
            {
                // Get section prefab
                RectTransform sectionPrefab = tableDelegate.GetSectionPrefab(s);
                if (sectionPrefab != null)
                {
                    // Instantiate section
                    RectTransform section = QuickInstantiate(sectionPrefab, "SECTION_" + s.ToString("000"));

                    // Load
                    Vector2 sectionSize = tableDelegate.LoadSection(s, section);

                    // Apply
                    sectionTransforms[s] = section;
                    _sectionSizes[s] = sectionSize;
                }

                // Get cell count
                int totalCells = tableDelegate.GetCellCount(s);
                totalCellsPerSection[s] = totalCells;

                // Load cells
                for (int c = 0; c < totalCells; c++)
                {
                    // Get cell prefab
                    RectTransform cellPrefab = tableDelegate.GetCellPrefab(s, c);
                    if (cellPrefab == null)
                    {
                        Log("Invalid Cell Prefab\nSection: " + s + "\nCell: " + c, LogType.Warning);
                        continue;
                    }

                    // Instantiate cell
                    string cellName = "CELL_S" + s.ToString("000") + "_C" + c.ToString("000");
                    RectTransform cell = QuickInstantiate(cellPrefab, cellName);

                    // Set selected
                    RFBButton cellButton = cell.GetComponent<RFBButton>();
                    if (cellButton != null && cellButton.selectButton)
                    {
                        cellButton.SetSelected(false);
                        cellButton.onSelectChange = delegate (bool toSelected)
                        {
                            OnSelectChange(cellButton, toSelected);
                        };
                    }

                    // Load
                    Vector2 cellSize = tableDelegate.LoadCell(s, c, cell);

                    // Apply
                    cellList.Add(cell);
                    cellSizes.Add(cellSize);
                }
            }

            // Apply cells
            cellTransforms = cellList.ToArray();
            _cellSizes = cellSizes.ToArray();
            selectedSection = -1;
            selectedSectionCell = -1;

            // Layout
            offset = 0f;
            LayoutTable();
        }
        // Quick instantiate
        protected RectTransform QuickInstantiate(RectTransform prefab, string id)
        {
            // Instantiate section
            RectTransform instance = RFBPool.instance.Load(prefab.gameObject).GetComponent<RectTransform>();
            instance.gameObject.name = id;

            // Transform setup
            instance.SetParent(container);
            instance.localPosition = Vector3.zero;
            instance.localRotation = Quaternion.identity;
            instance.localScale = Vector3.one;

            // Rect transform setup
            instance.anchorMin = new Vector2(0f, 1f);
            instance.anchorMax = instance.anchorMin;
            instance.pivot = instance.pivot;
            instance.anchoredPosition = Vector2.zero;

            // Return instance
            return instance;
        }
        #endregion

        #region LAYOUT
        [Header("Layout Settings")]
        // Force on update
        public bool needsLayout = false;
        // Whether the layout should be vertical or horizontal
        public bool isVertical;
        // The header size
        public float headerSize;
        // The footer size
        public float footerSize;
        // The section margin size
        public float sectionMarginSize;
        // The cell margin size
        public float cellMarginSize;
        // Content alt anchor
        [Range(0f, 1f)]
        public float contentAltAnchor = 0f;

        // Offset
        public float offset { get; private set; }

        // Handle layout of data
        public void LayoutTable()
        {
            // No longer needs layout
            needsLayout = false;
            if (sectionTransforms == null)
            {
                return;
            }

            // Get viewport sizes
            float altSize = isVertical ? rectTransform.rect.width : rectTransform.rect.height;

            // Add header
            float mainSize = headerSize;

            // Iterate sections
            int i = 0;
            for (int s = 0; s < totalSections; s++)
            {
                // Get section
                RectTransform section = sectionTransforms[s];
                if (section != null)
                {
                    /*
                    // Layout section
                    Vector2 sectionSize = _sectionSizes[s];
                    section.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, isVertical ? , sec)
                    */
                }

                // Iterate cells
                int totalCells = totalCellsPerSection[s];
                for (int c = 0; c < totalCells; c++)
                {
                    // Get cell
                    RectTransform cell = i >= 0 && i < cellTransforms.Length ? cellTransforms[i] : null;
                    if (cell != null)
                    {
                        // Add cell margin
                        if (c > 0)
                        {
                            mainSize += cellMarginSize;
                        }

                        // Layout cell
                        Vector2 cellSize = _cellSizes[i];
                        // Set anchors/pivots
                        cell.pivot = new Vector2(isVertical ? contentAltAnchor : 0f, isVertical ? 1f : contentAltAnchor);
                        cell.anchorMin = cell.pivot;
                        cell.anchorMax = cell.pivot;

                        // Set position
                        cell.anchoredPosition = new Vector2(isVertical ? 0f : mainSize, isVertical ? mainSize : 0f);

                        // Set alt size
                        float altCellSize = isVertical ? cellSize.x : cellSize.y;
                        if (altCellSize < 0f)
                        {
                            altCellSize = altSize;
                        }
                        cell.SetSizeWithCurrentAnchors(isVertical ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical, altCellSize);

                        // Set main size
                        float mainCellSize = isVertical ? cellSize.y : cellSize.x;
                        if (mainCellSize < 0f)
                        {
                            mainCellSize = isVertical ? cell.rect.height : cell.rect.width;
                        }
                        cell.SetSizeWithCurrentAnchors(isVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, mainCellSize);

                        // Add main size
                        mainSize += mainCellSize;
                    }

                    // Iterate
                    i++;
                }
            }

            // Add footer
            mainSize += footerSize;

            // Apply alt size
            container.SetSizeWithCurrentAnchors(isVertical ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical, altSize);
            // Apply main size
            container.SetSizeWithCurrentAnchors(isVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, mainSize);

            // Adjust scrolling
            RefreshCanScroll();
        }
        // Refresh can scroll
        public void RefreshCanScroll()
        {
            float minSize = isVertical ? rectTransform.rect.height : rectTransform.rect.width;
            float mainSize = isVertical ? container.rect.height : container.rect.width;
            bool allowed = mainSize > minSize;
            scroller.horizontal = isVertical ? false : allowed;
            scroller.vertical = isVertical ? allowed : false;
        }
        #endregion

        #region SELECTION
        // Selected cell
        public int selectedSection { get; private set; }
        public int selectedSectionCell { get; private set; }

        // Select cell
        private void OnSelectChange(RFBButton cellButton, bool toSelected)
        {
            if (toSelected)
            {
                int index = GetCellIndex(cellButton.GetComponent<RectTransform>());
                if (index != -1)
                {
                    SelectCell(index);
                }
            }
        }
        // Select cell with index
        private void SelectCell(int cellIndex)
        {
            int sectionIndex;
            int sectionCellIndex;
            GetCellIndexPath(cellIndex, out sectionIndex, out sectionCellIndex);
            SelectCell(sectionIndex, sectionCellIndex);
        }
        // Select cell with section/cell
        public virtual void SelectCell(int sectionIndex, int sectionCellIndex)
        {
            // Ignore
            if (selectedSection == sectionIndex && selectedSectionCell == sectionCellIndex)
            {
                return;
            }

            // Set
            selectedSection = sectionIndex;
            selectedSectionCell = sectionCellIndex;

            // Refresh
            RefreshCellSelections();

            // Call
            if (tableDelegate != null)
            {
                tableDelegate.SelectCell(selectedSection, selectedSectionCell);
            }
        }
        // Refresh
        public void RefreshCellSelections()
        {
            int i = 0;
            for (int s = 0; s < totalSections; s++)
            {
                int totalCells = totalCellsPerSection[s];
                for (int c = 0; c < totalCells; c++)
                {
                    // Get cell button
                    RectTransform cell = cellTransforms[i];
                    if (cell != null)
                    {
                        RFBButton cellButton = cell.GetComponent<RFBButton>();
                        if (cellButton != null)
                        {
                            cellButton.SetSelected(s == selectedSection && c == selectedSectionCell);
                        }
                    }

                    // Iterate
                    i++;
                }
            }
        }
        #endregion

        #region HELPERS
        // Get index
        public int GetCellIndex(RectTransform cell)
        {
            List<RectTransform> cells = new List<RectTransform>();
            if (cellTransforms != null)
            {
                cells.AddRange(cellTransforms);
            }
            return cells.IndexOf(cell);
        }
        // Get
        public int GetCellIndex(int sectionIndex, int sectionCellIndex)
        {
            // Iterate
            int i = 0;
            for (int s = 0; s <= Mathf.Max(sectionIndex, totalSections - 1); s++)
            {
                if (s == sectionIndex)
                {
                    return i + sectionCellIndex;
                }
                else
                {
                    i += totalCellsPerSection[s];
                }
            }

            // None
            return -1;
        }
        // Determine index path
        public void GetCellIndexPath(int cellIndex, out int sectionIndex, out int sectionCellIndex)
        {
            // Iterate
            int i = 0;
            for (int s = 0; s < totalSections; s++)
            {
                int totalCells = totalCellsPerSection[s];
                for (int c = 0; c < totalCells; c++)
                {
                    if (i == cellIndex)
                    {
                        sectionIndex = s;
                        sectionCellIndex = c;
                        return;
                    }
                    i++;
                }
            }

            // Not found
            sectionIndex = -1;
            sectionCellIndex = -1;
        }
        #endregion
    }
}
