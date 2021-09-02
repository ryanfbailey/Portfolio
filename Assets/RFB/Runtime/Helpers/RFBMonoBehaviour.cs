/*******************************
 * 
 * RFBMonoBehaviour.cs
 * by: Ryan F. Bailey
 * 
 * description: 
*******************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    // Used for editor scripts
    public class RFBMonoBehaviour : MonoBehaviour
    {
        // Log title
        public virtual string GetLogTitle()
        {
            return GetType().ToString();
        }
        // Log
        public void Log(string comment, LogType type = LogType.Log)
        {
            LogUtility.Log(comment, GetLogTitle(), type);
        }
    }
}
