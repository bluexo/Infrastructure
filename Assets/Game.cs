using Origine;
using Origine.Fsm;
using Origine.ObjectPool;
using Origine.Setting;

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.AddressableAssets;

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
            yield return ConfigManager.LoadConfigFilesAsync($"ConfigFiles");
            yield return UIManager.InitializeAsync($"UI/UIManager.prefab");
            yield return AudioManager.InitializeAsync($"Settings");
            InitializeInterpreter();
            Initialized = true;
        }

        private void InitializeInterpreter()
        {
            Interpreter.SetValue("GUI", new Func<string, BaseUI>(ui => UIManager.GetOrCreate(Utility.AssemblyCollection.GetType(ui))));
            Interpreter.SetValue("STG", new Action<string>(stage => StageManager.Switch(Utility.AssemblyCollection.GetType(stage))));
            Interpreter.SetValue("PST", new Func<string, PresenterBase>(controller => PresenterManager.GetByName(controller)));
            Interpreter.SetValue("CMD", new Action<string, object>((c, o) => EventManager.Publish(CommandEventArgs.EventId, new CommandEventArgs(c, o))));
        }

        private void Update() => GameContext.Update(Time.deltaTime);

        private void OnDestroy() => GameContext.Dispose();
    }
}