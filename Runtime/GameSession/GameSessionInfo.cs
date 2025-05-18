using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SaveLoadSystem.FileProcessing;
using UnityEngine;

namespace SaveLoadSystem.GameSession
{
    public class GameSessionInfo
    {
        private readonly List<SaveInfoFile> _saves = new();

        public GameSessionInfo(string displayName)
        {
            DisplayName = displayName;
            GameVersion = Application.version;
        }

        public GameSessionInfo(SaveInfoFile fileInfo, bool isSaveSupported)
        {
            DisplayName = fileInfo.InnName;
            Screenshot = fileInfo.Screenshot;
            GameVersion = fileInfo.GameVersion;
            AddSave(fileInfo, isSaveSupported);
        }
        
        public string DisplayName { get; private set; }
        public DateTime LastPlayedDate { get; private set; }
        public ScreenshotData Screenshot { get; private set; }
        public string GameVersion { get; private set; }
        public bool IsSupported { get; private set; }
        public ReadOnlyCollection<SaveInfoFile> Saves => _saves.AsReadOnly();
        public SaveInfoFile LatestSave { get; private set; }

        public void AddSave(SaveInfoFile saveInfo, bool isSaveSupported)
        {
            AddSaveToCollection(saveInfo);
            UpdateLastPlayedInfo(saveInfo);
            UpdateGameVersion(saveInfo.GameVersion);
            if (!IsSupported)
                IsSupported = isSaveSupported;
        }

        private void AddSaveToCollection(SaveInfoFile saveInfo)
        {
            if (_saves.Count > 0)
            {
                var saveTime = saveInfo.GetDateTime();
                for (var i = 0; i < _saves.Count; i++)
                {
                    if (saveTime < _saves[i].GetDateTime()) continue;
                    _saves.Insert(i, saveInfo);
                    return;
                }
            }

            _saves.Add(saveInfo);
        }

        private void UpdateGameVersion(string saveDataGameVersion) =>
            GameVersion = GetYoungestVersion(GameVersion, saveDataGameVersion);

        private string GetYoungestVersion(string firstVersion, string secondVersion)
        {
            if (firstVersion == "") return secondVersion;
            if (secondVersion == "") return firstVersion;
            var firstDigits = firstVersion.Split(".");
            var secondDigits = secondVersion.Split(".");
            var sharedDigitsCount = Math.Min(firstDigits.Length, secondDigits.Length);
            for (var i = 0; i < sharedDigitsCount; i++)
            {
                var first = int.Parse(firstDigits[i]);
                var second = int.Parse(secondDigits[i]);
                if (first == second) continue;
                if (first > second) return firstVersion;
                if (first < second) return secondVersion;
            }

            return firstDigits.Length > secondDigits.Length ? firstVersion : secondVersion;
        }

        private void UpdateLastPlayedInfo(SaveInfoFile saveInfo)
        {
            var newDate = saveInfo.GetDateTime();
            if (newDate > LastPlayedDate)
                SetAsLatest();
            return;

            void SetAsLatest()
            {
                LastPlayedDate = newDate;
                Screenshot = saveInfo.Screenshot;
                LatestSave = saveInfo;
            }
        }

        public void RemoveSave(SaveInfoFile saveInfo)
        {
            _saves.Remove(saveInfo);
            LastPlayedDate = default;
            GameVersion = "";
            LatestSave = null;
            foreach (var save in _saves)
            {
                UpdateLastPlayedInfo(save);
                UpdateGameVersion(save.GameVersion);
            }
        }

        public void SetVersion(string version) => GameVersion = version;

        public void RemoveSave(string infoFileName)
        {
            var files = _saves.FindAll(el => el.FileName == infoFileName);
            for (var i = files.Count - 1; i >= 0; i--)
                RemoveSave(files[i]);
        }
    }
}