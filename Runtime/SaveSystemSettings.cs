using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
#endif

namespace SaveLoadSystem
{
    [CreateAssetMenu(menuName = "Save System/Save System Settings", fileName = "(S) Save System")]
    public class SaveSystemSettings : ScriptableObjectInstaller<SaveSystemSettings>
    {
        [SerializeField] private SaveSystem.Settings _manager;
        [SerializeField] private ScreenshotMaker.Settings _screenshotMaker;
        
#if UNITY_EDITOR
        [Button]
        private void OpenSaveFolder() => EditorUtility.RevealInFinder(_manager.SaveFolderPath);
#endif
        public override void InstallBindings()
        {
            Container.BindInstance(_manager).IfNotBound();
            Container.BindInstance(_screenshotMaker).IfNotBound();
        }
    }
}