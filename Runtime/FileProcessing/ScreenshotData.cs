using System;
using UnityEngine;

namespace SaveLoadSystem.FileProcessing
{
    [Serializable]
    public class ScreenshotData
    {
        public byte[] Bytes;
        public int Width;
        public int Height;

        public ScreenshotData(){}
        public ScreenshotData(Texture2D texture)
        {
            Bytes = texture.GetRawTextureData();
            Width = texture.width;
            Height = texture.height;
        }

        public Texture2D GenerateTexture()
        {
            Texture2D ret = new(Width, Height, TextureFormat.RGBA32, false);
            ret.LoadRawTextureData(Bytes);
            ret.Apply();
            return ret;
        }
    }
}