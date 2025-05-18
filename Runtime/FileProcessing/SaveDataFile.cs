using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.FileProcessing
{
    [Serializable]
    public class SaveDataFile
    {
        [SerializeReference] public List<SaveDataContainer> EntitiesData;
    }
}