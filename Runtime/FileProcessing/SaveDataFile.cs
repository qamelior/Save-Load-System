using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveLoadSystem.FileProcessing
{
    [Serializable]
    public class SaveDataFile
    {
        [SerializeReference] public List<SaveDataContainer> EntitiesData;
    }
}