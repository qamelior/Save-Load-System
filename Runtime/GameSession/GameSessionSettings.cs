using UnityEngine;
using Zenject;

namespace SaveLoadSystem.GameSession
{
    [CreateAssetMenu(menuName = "Save System/Game Session Settings", fileName = "(S) Game Sessions")]
    public class GameSessionSettings : ScriptableObjectInstaller<SaveSystemSettings>
    {
        [field: SerializeField] public string EditorGameSessionName { get; set; } = "[Develop]";
        [field: SerializeField] public string DayString { get; private set; } = "Day";
        public override void InstallBindings() => Container.BindInstance(this).IfNotBound();
    }
}