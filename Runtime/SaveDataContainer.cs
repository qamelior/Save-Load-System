using System;

namespace SaveLoadSystem
{
    [Serializable]
    public class SaveDataContainer
    {
        public string EntityID;
        public SaveDataContainer(string entityID) => EntityID = entityID;
    }
}