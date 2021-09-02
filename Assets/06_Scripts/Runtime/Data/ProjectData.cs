using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Type of display
public enum DisplayType
{
    Hidden = 0,
    QA = 1,
    Live = 2
}

// Type of project
public enum ProjectType
{
    Student,
    Work,
    Personal
}

[Serializable]
public struct ProjectData
{
    // Project id
    public string projectID;
    // Row index
    public int index;
    // Whether project should be hidden from portfolio
    public DisplayType display;
    // Type of project
    public ProjectType type;

    // Title
    public string title;

    // Role
    public string role;
    // Company
    public string company;
    // Skills
    public string skills;
    // Platforms
    public string platforms;
    // Release
    public string release;

    // Description
    public string description;
    // Contribution
    public string contribution;

    // Icon
    public Texture2D icon;
    // Gallery Images 
    public GalleryItemData[] gallery;
}
