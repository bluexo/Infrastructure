using Origine;
using Origine.BT;

using System;
using System.Collections;

using UnityEngine;

namespace Wars
{
    public partial class Game : MonoBehaviour
    {
        public static readonly GameContext GameContext = new GameContext();

        public static IStageManager StageManager { get; private set; }
        public static IConfigManager ConfigManager { get; private set; }
        public static IPresenterManager PresenterManager { get; private set; }
        public static IObjectPoolManager ObjectPoolManager { get; private set; }
        public static IResourceManager ResourceManager { get; private set; }
        public static IUIManager UIManager { get; private set; }
        public static IInterpreter Interpreter { get; private set; }
        public static IEventManager EventManager { get; private set; }
        public static IFsmManager FsmManager { get; private set; }
        public static IStorageManager StorageManager { get; private set; }
        public static ISettingManager SettingManager { get; private set; }
        public static IAudioManager AudioManager { get; private set; }
        public static IBTManager BTManager { get; private set; }

        public static bool Initialized { get; private set; }

        private void Awake()
        {
            GameContext.Initialize();

            FsmManager = GameContext.GetModule<IFsmManager>();
            EventManager = GameContext.GetModule<IEventManager>();
            ConfigManager = GameContext.GetModule<IConfigManager>();
            StorageManager = GameContext.GetModule<IStorageManager>();

            PresenterManager = GameContext.GetModule<IPresenterManager>();
            ObjectPoolManager = GameContext.GetModule<IObjectPoolManager>();
            ResourceManager = GameContext.GetModule<IResourceManager>();

            SettingManager = GameContext.GetModule<ISettingManager>();
            AudioManager = GameContext.GetModule<IAudioManager>();
            StageManager = GameContext.GetModule<IStageManager>();
            Interpreter = GameContext.GetModule<IInterpreter>();
            UIManager = GameContext.GetModule<IUIManager>();

            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator Start()
        {
            yield return ConfigManager.InitializeAsync($"ConfigFiles");
            yield return UIManager.InitializeAsync($"UIManager.prefab");
            yield return AudioManager.InitializeAsync($"Settings");
            Initialized = true;
            StageManager.Start<StartStage>();
        }

        private void Update() => GameContext.Update(Time.deltaTime);

        private void OnDestroy() => GameContext.Dispose();
    }
}
