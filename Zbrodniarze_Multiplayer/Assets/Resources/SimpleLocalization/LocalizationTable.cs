using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(
       fileName = "LocalizationTable",
       menuName = "Localization/LocalizationTable",
       order = 1
       )]

public class LocalizationTable : ScriptableObject
{
    public TextAsset cvsfile;

    public List<string> languages = new List<string>();

    public List<Entry> entries = new List<Entry>();

    [Serializable]
    public class entry
    {
        public string key;
        public List<string> values = new List<string>();
    }


}
