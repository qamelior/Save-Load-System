using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Runtime.FileProcessing;
using Runtime.GameSession;
using Runtime.JsonConverters;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Runtime
{
    public class SaveSystem
    {
        public enum FileType
        {
            Info,
            Data,
        }

        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        private readonly List<SaveEntity> _entities = new();
        private readonly FileReadWriter _fileReadWriter = new();
        private readonly string _playerSettingsFullPath;
        private readonly ScreenshotMaker _screenshotMaker;
        private readonly Settings _settings;
        private GameSessionManager _gameSessionManager;
        private SaveInfoFile _loadedFile;

        public SaveSystem(Settings settings, ScreenshotMaker screenshotMaker)
        {
            AddJsonConverters();
            _settings = settings;
            _screenshotMaker = screenshotMaker;
            _fileReadWriter.CreateDirectory(_settings.DirectoryPath);
            _fileReadWriter.CreateDirectory(_settings.SaveFolderPath);
            _playerSettingsFullPath = $"{_settings.DirectoryPath}/{_settings.PlayerSettingsFileName}.json";
        }

        public bool LoadingInProgress { get; private set; }

        public event Action OnSavingStarted = delegate { };

        private static void AddJsonConverters()
        {
            JsonSerializerSettings.Converters.Add(new DictionaryJsonConverter());
            JsonSerializerSettings.Converters.Add(new Vector2Converter());
            JsonSerializerSettings.Converters.Add(new Vector2IntConverter());
            JsonSerializerSettings.Converters.Add(new Vector3Converter());
            JsonSerializerSettings.Converters.Add(new Vector3IntConverter());
            JsonSerializerSettings.Converters.Add(new QuaternionConverter());
        }

        private string FullPath(string fileName, FileType type) =>
            $"{_settings.SaveFolderPath}/{fileName}{_settings.Extension(type)}";

        public void SavePlayerSettings(PlayerSettingsData data) =>
            _fileReadWriter.WriteFile(_playerSettingsFullPath, Encoding.UTF8.GetBytes(SerializeObject(data)));

        public PlayerSettingsData LoadPlayerSettings()
        {
            var data = _fileReadWriter.ReadFile(_playerSettingsFullPath);
            return data == null
                ? null
                : JsonConvert.DeserializeObject<PlayerSettingsData>(Encoding.UTF8.GetString(data),
                    JsonSerializerSettings);
        }

        public void SaveFile(string fileName)
        {
            OnSavingStarted.Invoke();
            SaveInfo(fileName);
            SaveEntities(fileName);
        }

        private void SaveInfo(string fileName)
        {
            var gameVersionString = Application.version;
            var sessionInfo = _gameSessionManager.RefreshDataForCurrentGameSession();
            var data = new SaveInfoFile
            {
                Date = new SaveInfoFile.DateSaveData(DateTime.Now),
                InnName = sessionInfo.DisplayName,
                Screenshot = _screenshotMaker.Generate(),
                GameVersion = gameVersionString,
                FileName = fileName,
                FileType = SaveInfoFile.FileLocation.LocalStorage,
            };
            var fileData = Encoding.UTF8.GetBytes(SerializeObject(data));
            var filePath = FullPath(fileName, FileType.Info);
            _fileReadWriter.WriteFile(filePath, fileData);
            _gameSessionManager.AddSaveToCurrentGameSession(data);
        }

        private void SaveEntities(string fileName)
        {
            var filePath = FullPath(fileName, FileType.Data);
            var fileData = new SaveDataFile { EntitiesData = _entities.Select(entity => entity.Save()).ToList() };
            _fileReadWriter.WriteFile(filePath, Encoding.UTF8.GetBytes(SerializeObject(fileData)));
        }

        private static string SerializeObject(object fileData) =>
            JsonConvert.SerializeObject(fileData, Formatting.Indented, JsonSerializerSettings);

        public void RegisterSaveEntity(SaveEntity entity) => _entities.Add(entity);

        public void ProcessSceneExit()
        {
            for (var i = _entities.Count - 1; i >= 0; i--)
            {
                if (_entities[i].IsPersistent) continue;
                _entities.RemoveAt(i);
            }
        }

        public void ProcessSceneEnter()
        {
            LoadingInProgress = true;
            LoadEntities(_loadedFile.FileName);
            LoadingInProgress = false;
            _loadedFile = null;
        }

        public static SaveInfoFile ParseInfoData(byte[] data) =>
            JsonConvert.DeserializeObject<SaveInfoFile>(Encoding.UTF8.GetString(data), JsonSerializerSettings);

        public void DeleteSave(SaveInfoFile fileInfo)
        {
            DeleteSave(fileInfo.FileName, FileType.Info);
            DeleteSave(fileInfo.FileName, FileType.Data);
        }

        private void DeleteSave(string fileName, FileType type)
        {
            if (TryDeleteFile(fileName, type))
                return;

            fileName = fileName.Replace(" ", "_");
            if (TryDeleteFile(fileName, type))
                return;

            fileName = fileName.Replace("[", "").Replace("]", "");
            TryDeleteFile(fileName, type);
        }

        private bool TryDeleteFile(string fileName, FileType type)
        {
            if (!FileExists(fileName, type)) return false;
            _fileReadWriter.DeleteFile(FullPath(fileName, type));
            return true;
        }

        public void ProcessLoadSaveRequest(SaveInfoFile value) => _loadedFile = value;

        private void LoadEntities(string fileName)
        {
            var data = ReadDataFile(fileName);
            if (data == null) return;
            var saveFileData =
                JsonConvert.DeserializeObject<SaveDataFile>(Encoding.UTF8.GetString(data), JsonSerializerSettings);
            var sortedEntities = SortEntities(_entities);
            if (sortedEntities == null) return;
            var gameVersion = new Version(_loadedFile.GameVersion);
            foreach (var entity in sortedEntities)
            {
                var saveData = saveFileData.EntitiesData.Find(el => el.EntityID == entity.EntityID);
                if (saveData != null)
                    entity.Load(saveData, gameVersion);
            }
        }

        private IEnumerable<SaveEntity> SortEntities(List<SaveEntity> entities)
        {
            var sorting = new TopologicalSorting<SaveEntity>("Save System");
            foreach (var entity in entities)
                sorting.AddNode(entity, entity.EntitiesToLoadBeforeMe.Select(FindEntityOfMatchingType));

            return sorting.Execute();
            SaveEntity FindEntityOfMatchingType(Type t) => _entities.FirstOrDefault(el => el.GetType() == t);
        }

        private byte[] ReadDataFile(string fileName)
        {
            var data = _fileReadWriter.ReadFile(FullPath(fileName, FileType.Data));
            if (data != null)
                return data;

            fileName = fileName.Replace(" ", "_");
            data = _fileReadWriter.ReadFile(FullPath(fileName, FileType.Data));
            if (data != null)
                return data;

            fileName = fileName.Replace("]", "");
            data = _fileReadWriter.ReadFile(FullPath(fileName, FileType.Data));
            if (data != null)
                return data;

            fileName = fileName.Replace("[", "");
            data = _fileReadWriter.ReadFile(FullPath(fileName, FileType.Data));
            return data;
        }

        public bool FileExists(string fileName) => FileExists(fileName, FileType.Info);

        private bool FileExists(string fileName, FileType type)
        {
            var fullPath = FullPath(fileName, type);
            return _fileReadWriter.FileAlreadyExists(fullPath);
        }

        public void LinkGameSessionManager(GameSessionManager gameSessionManager)
        {
            _gameSessionManager = gameSessionManager;
            _gameSessionManager.InitGameSessionList(
                _fileReadWriter.ReadAllFilesOfExtension(_settings.SaveFolderPath, _settings.InfoExtension));
        }

        public bool IsSaveSupported(SaveInfoFile save)
        {
            var saveVersion = save.GameVersion.Split(".");
            if (saveVersion.Any(el => !int.TryParse(el, out _)))
                return false;
            return new Version(save.GameVersion).CompareTo(new Version(_settings.EarliestSupportedVersion)) >= 0;
        }


        private void QuickSave() => SaveFile(_gameSessionManager.GetDefaultFileNameForCurrentSession(_settings.QuickSavePrefix));

        private void QuickLoad() => _gameSessionManager.QuickLoad();

        [Serializable]
        public class Settings
        {
            [SerializeField] private string _rootFolder = "";
            [SerializeField] private string _saveFilesFolder = "/Saves";
            [SerializeField] private string _dataExtension = ".sav";
            [field: SerializeField] public string InfoExtension { get; private set; } = ".savinfo";
            [field: SerializeField] public string PlayerSettingsFileName { get; private set; } = "PlayerSettings";
            [field: SerializeField] public string QuickSavePrefix { get; private set; } = "Quicksave - ";
            [field: SerializeField] public string EarliestSupportedVersion { get; private set; } = "0.0.1";
            private string MyDocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            public string DirectoryPath => MyDocumentsPath.Replace("\\", "/") + _rootFolder;
            public string SaveFolderPath => $"{DirectoryPath}{_saveFilesFolder}";

            public string Extension(FileType type) => type switch
            {
                FileType.Info => InfoExtension,
                FileType.Data => _dataExtension,
                _ => "",
            };
        }
    }
}