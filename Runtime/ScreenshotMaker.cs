using System;
using SaveLoadSystem.FileProcessing;
using UnityEngine;

namespace SaveLoadSystem
{
    public class ScreenshotMaker
    {
        private readonly Settings _settings;
        private Camera _camera;

        public ScreenshotMaker(Settings settings) => _settings = settings;

        public void AssignCamera(Camera camera) => _camera = camera;
        
        public ScreenshotData Generate()
        {
            Rect rect = new(0, 0, _settings.Width, _settings.Height);
            RenderTexture renderTexture = new(_settings.Width, _settings.Height, 24);
            Texture2D texture2D = new(_settings.Width, _settings.Height, _settings.Format, false);
            _camera.targetTexture = renderTexture;
            _camera.Render();
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(rect, 0, 0);
            _camera.targetTexture = null;
            RenderTexture.active = null;
            return new ScreenshotData(texture2D);
        }

        [Serializable]
        public class Settings
        {
            [field: SerializeField] public TextureFormat Format { get; private set; } = TextureFormat.RGBA32;
            [field: SerializeField] public int Width { get; private set; } = 1280;
            [field: SerializeField] public int Height { get; private set; } = 720;
        }
    }
}