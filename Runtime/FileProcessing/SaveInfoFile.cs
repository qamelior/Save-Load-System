using System;
using UnityEngine;

namespace SaveLoadSystem.FileProcessing
{
    [Serializable]
    public class SaveInfoFile
    {
        public string FileName;
        public string InnName;
        public DateSaveData Date;
        public ScreenshotData Screenshot;
        public string GameVersion;
        public FileLocation FileType;
        public DateTime GetDateTime() => new(Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute, Date.Seconds);
        public string GetDateTimeString() => GetDateTime().ToString("g");
        public Texture2D GetScreenshotTexture() => Screenshot?.GenerateTexture();

        [Serializable]
        public class DateSaveData
        {
            public int Year;
            public int Month;
            public int Day;
            public int Hour;
            public int Minute;
            public int Seconds;

            public DateSaveData(DateTime currentTime)
            {
                Year = currentTime.Year;
                Month = currentTime.Month;
                Day = currentTime.Day;
                Hour = currentTime.Hour;
                Minute = currentTime.Minute;
                Seconds = currentTime.Second;
            }
        }

        public enum FileLocation
        {
            LocalStorage,
            CloudStorage,
            Virtual,
        }
    }
}