using System;

namespace Runtime
{
    public abstract class SaveEntity
    {
        protected SaveEntity(SaveSystem saveSystem) => saveSystem.RegisterSaveEntity(this);
        public abstract string EntityID { get; }
        public abstract bool IsPersistent { get; }
        public abstract Type[] EntitiesToLoadBeforeMe { get; }
        public abstract SaveDataContainer Save();
        public abstract void Load(SaveDataContainer data, Version gameVersion);
    }
}