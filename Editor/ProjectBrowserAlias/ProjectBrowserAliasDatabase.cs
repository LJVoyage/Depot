using UnityEngine;
using System;
using System.Collections.Generic;


namespace VoyageForge.EditorTools
{

    [Serializable]
    public class AliasItem
    {
        public string guid;

        public string path;

        public string alias;
    }



    public class ProjectBrowserAliasDatabase : ScriptableObject
    {

        public List<AliasItem> items = 
            new List<AliasItem>();

    }

}