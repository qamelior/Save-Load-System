using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.FileProcessing;
using UnityEngine;

namespace Runtime.GameSession
{
    public class GameSessionManager : SaveEntity
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly SaveSystem _saveSystem;
        private readonly GameSessionSettings _settings;
        private GameSessionInfo _currentGameSession;
        private List<GameSessionInfo> _sortedGameSessions = new();

        public GameSessionManager(GameSessionSettings settings, SaveSystem saveSystem) : base(saveSystem)
        {
            _settings = settings;
            _saveSystem = saveSystem;
            _saveSystem.LinkGameSessionManager(this);
        }

        public override Type[] EntitiesToLoadBeforeMe => new Type[] { };
        public override bool IsPersistent => true;
        public override string EntityID => "GameSessionManager";
        public ReadOnlyCollection<GameSessionInfo> SortedGameSessions => _sortedGameSessions.AsReadOnly();

        public void QuickLoad()
        {
            var youngestSession = FindLatestGameSession();
            if (youngestSession == null) return;
            LoadGameSession(youngestSession, _cancellationTokenSource.Token).Forget();
        }

        public async UniTask LoadGameSession(GameSessionInfo gameSession, CancellationToken token)
        {
            await UniTask.DelayFrame(1, cancellationToken: token);
            _saveSystem.ProcessLoadSaveRequest(gameSession.LatestSave);
        }

        public GameSessionInfo FindLatestGameSession()
        {
            GameSessionInfo youngestGameSession = null;
            foreach (var session in SortedGameSessions)
            {
                if (!session.IsSupported) continue;
                if (youngestGameSession == null || youngestGameSession.LastPlayedDate <= session.LastPlayedDate)
                    youngestGameSession = session;
            }

            return youngestGameSession;
        }

        public override SaveDataContainer Save() => new SessionSaveData(EntityID) { Name = _currentGameSession.DisplayName };

        public override void Load(SaveDataContainer data, Version gameVersion)
        {
            if (data is not SessionSaveData saveData) return;
            SetCurrentGameSessionOrCreateNew(saveData.Name);
        }

        public void InitGameSessionList(List<byte[]> fileData)
        {
            _sortedGameSessions = ReadSaveFiles(fileData);
            _sortedGameSessions.Sort((a, b) =>
                a.LastPlayedDate > b.LastPlayedDate ? -1 : a.LastPlayedDate < b.LastPlayedDate ? 1 : 0);

#if UNITY_EDITOR
            SetCurrentGameSessionOrCreateNew(_settings.EditorGameSessionName);
#endif
        }

        private List<GameSessionInfo> ReadSaveFiles(List<byte[]> fileData)
        {
            var sessions = new List<GameSessionInfo>();
            foreach (var fileBytes in fileData)
            {
                var saveData = SaveSystem.ParseInfoData(fileBytes);
                var existingSession = sessions.FirstOrDefault(el => el.DisplayName == saveData.InnName);
                if (existingSession != null)
                    existingSession.AddSave(saveData, IsSaveSupported(saveData));
                else
                    sessions.Add(new GameSessionInfo(saveData, IsSaveSupported(saveData)));
            }

            return sessions;
        }

        private void SetCurrentGameSessionOrCreateNew(string name) =>
            _currentGameSession = _sortedGameSessions.FirstOrDefault(el => el.DisplayName == name) ?? new GameSessionInfo(name);

        public GameSessionInfo RefreshDataForCurrentGameSession()
        {
            if (_currentGameSession == null) return null;
            _currentGameSession.SetVersion(Application.version);
            return _currentGameSession;
        }

        public void AddSaveToCurrentGameSession(SaveInfoFile info)
        {
            _currentGameSession.RemoveSave(info.FileName);
            _currentGameSession.AddSave(info, IsSaveSupported(info));
            if (_sortedGameSessions.Contains(_currentGameSession)) return;
            _sortedGameSessions.Insert(0, _currentGameSession);
        }

        public bool IsSaveSupported(SaveInfoFile save) => _saveSystem.IsSaveSupported(save);

        public virtual string GetDefaultFileNameForCurrentSession(string prefix = "")
        {
            var iteration = 1;
            var fileName = ComposeFileName(prefix, iteration);
            while (_saveSystem.FileExists(fileName))
                fileName = ComposeFileName(prefix, ++iteration);
            return fileName;
        }

        public virtual string ComposeFileName(string prefix, int iteration) =>
            $"{prefix} {(iteration < 2 ? "" : $" - {iteration}")}";

        [Serializable]
        public class SessionSaveData : SaveDataContainer
        {
            public string Name;

            public SessionSaveData(string entityID) : base(entityID) { }
        }
    }
}