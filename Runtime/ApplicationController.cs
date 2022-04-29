

using System.Collections;
using System.Collections.Generic;
using Rayark.Mast;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime;

namespace MacacaGames.GameSystem
{
    /// <summary>
    /*
    /// Life cycle design
    ///
                                    Unity App Start
                                            | 
                                            V
                                ApplicitionController Init
                                            |   
                                            V        
                            Instance all ScriptableObjectLifeCycle
                    Get all MonoBehaviourLifeCycle instance in Scene
                            Instance all [ResloveTarget] class
                                            |  
                                            V 
            Init all ScriptableObjectLifeCycle, MonoBehaviourLifeCycle, [ResloveTarget] instance
                                            |  
                                            V 
                                    Inject all target
                                            |   
                                            V  
                    ┌─────────────>─────────┐   [OnApplicationBeforeGamePlay]
                    |                       |  
                    |                       |───────────────────────────────────────[ApplicitionController.ApplicationTask]
                    |       ┌───────>──┐    |
                    |       |          |    | 
                    |       |       [Game Lobby] (A state for waiting for enter gameplay)
                    |       |          |    | 
                    |       └──────────┘    |
                    |                       |   [ApplicationController.Instance.StartGame] 
                    |                       V
                    |       ┌───────>──┐    |
                    |       |          |    |  
                    |       |     [GamePlayData.GamePlay()]
                    |       |          |    | 
                    |       └──────────┘    |
                    |                       | 
                    |                       |
                    |                       |                   
                    └───────────────────────┘   [GamePlayController.SuccessGamePlay]
    /// 
    /// 
    */
    /// </summary>
    [ResolveTarget]
    public class ApplicationController : MonoBehaviour
    {
        public static ApplicationController Instance;
        void Awake()
        {
            Instance = this;
            Init();
        }

        [SerializeField]
        ScriptableObjectGamePlayData[] gamePlayDatas;

        public IGamePlayData GetDefaultGameplayData()
        {
            return gamePlayDatas[0];
        }

        [SerializeField] ScriptableObjectLifeCycle[] scriptableObjectLifeCycle;
        MonoBehaviourLifeCycle[] monoBehaviourLifeCycleInstance;
        [System.NonSerialized]
        public List<ScriptableObjectLifeCycle> scriptableObjectLifeCycleInstances = new List<ScriptableObjectLifeCycle>();
        object[] resolveTargetInstance;

        Dictionary<Type, IApplicationLifeCycle> allApplicationLifeCycles = new Dictionary<Type, IApplicationLifeCycle>();

        /// <summary>
        /// Fire while application init, only fire once
        /// Usually to use in other scene before Scene with ApplicationController
        /// e.g. Loading scene
        /// </summary>
        Action OnApplicationInit;

        /// <summary>
        /// Fire once between GameEnd and next GamePlay, also fire once after init. 
        /// </summary>
        Action OnEnterLobby;

        GamePlayController gamePlayController;

        public bool IsInit = false;

        public void Init()
        {
            if (IsInit) return;
            OnApplicationInit?.Invoke();
            gamePlayController = new GamePlayController(this, GetDefaultGameplayData());
            applicationExecutor = new Executor();

            //Prepare Instance
            foreach (var item in scriptableObjectLifeCycle)
            {
                var temp = Instantiate(item);
                scriptableObjectLifeCycleInstances.Add(temp);
            }
            monoBehaviourLifeCycleInstance =
                Resources.FindObjectsOfTypeAll<MonoBehaviourLifeCycle>()
                        .Where((m) =>
                                m.gameObject != null &&
                                m.gameObject.scene.IsValid())
                        .ToArray();

            resolveTargetInstance = GenerateResolveTargetInstance(GetAllRegisterType().ToArray());

            //Inject Dependency
            foreach (var item in gamePlayDatas)
            {
                InjectByClass(item);
            }

            foreach (var item in scriptableObjectLifeCycleInstances)
            {
                InjectByClass(item);
            }

            foreach (var item in monoBehaviourLifeCycleInstance)
            {
                InjectByClass(item);
            }

            foreach (var item in resolveTargetInstance)
            {
                InjectByClass(item);
            }

            //Init
            gamePlayController.Init();

            foreach (var item in scriptableObjectLifeCycleInstances)
            {
                item.Init();
                allApplicationLifeCycles.Add(item.GetType(), item);
            }

            foreach (var item in monoBehaviourLifeCycleInstance)
            {
                item.Init();
                allApplicationLifeCycles.Add(item.GetType(), item);
            }

            foreach(var item in gamePlayDatas)
            {
                item.Init();
            }

            foreach (var item in resolveTargetInstance)
            {
                if (item is IApplicationLifeCycle applicationLifeCycle)
                {
                    applicationLifeCycle.Init();
                    allApplicationLifeCycles.Add(item.GetType(), applicationLifeCycle);
                }
            }

            applicationExecutor.Add(ApplicationTask());
            applicationExecutor.Add(ApplicationUpdateRunner());
            IsInit = true;
        }

        IEnumerator ApplicationUpdateRunner()
        {
            while (true)
            {
                foreach (var item in allApplicationLifeCycles)
                {
                    try
                    {
                        item.Value.OnApplicationUpdate();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception in {item.GetType().ToString()} , msg:{ex.ToString()}");
                    }
                }
                yield return null;
            }
        }
        IEnumerator GamePlayUpdateRunner()
        {
            while (true)
            {
                foreach (var item in allApplicationLifeCycles)
                {
                    try
                    {
                        item.Value.OnGamePlayUpdate();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception in {item.GetType().ToString()} , msg:{ex.ToString()}");
                    }
                }
                yield return null;
            }
        }
        IEnumerator UnPauseGamePlayUpdateRunner()
        {
            while (true)
            {
                foreach (var item in allApplicationLifeCycles)
                {
                    try
                    {
                        item.Value.OnGamePlayUpdate();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception in {item.GetType().ToString()} , msg:{ex.ToString()}");
                    }
                }
                yield return null;
            }
        }

        Executor applicationExecutor;
        Executor gamePlayTask;
        public bool isInGame { get; private set; } = false;

        IEnumerator ApplicationTask()
        {
            while (true)
            {
                currentApplicationState = ApplicationState.Lobby;
                currentGameState = GameState.OutsideGamePlay;
                isInGame = false;
                OnEnterLobby?.Invoke();
                gamePlayController.OnEnterLobby();

                foreach (var item in allApplicationLifeCycles)
                {
                    try
                    {
                        item.Value.OnEnterLobby();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception in {item.GetType().ToString()} , msg:{ex.ToString()}");
                    }
                }

                while (!isInGame)
                {
                    yield return null;
                }
                currentApplicationState = ApplicationState.Gaming;
                currentGameState = GameState.Playing;
                //Game Start
                gamePlayTask = gamePlayController.GamePlayControllerCoreLoop(GamePlayUpdateRunner(), UnPauseGamePlayUpdateRunner());
                while (!gamePlayTask.Finished)
                {
                    gamePlayController.gamePlayUnpauseUpdateExecuter.Resume(Rayark.Mast.Coroutine.Delta);

                    if (gamePlayController.isPause == false && gamePlayController.isContinueing == false)
                    {
                        gamePlayController.gamePlayUpdateExecuter.Resume(Rayark.Mast.Coroutine.Delta);
                        gamePlayTask.Resume(Rayark.Mast.Coroutine.Delta);
                    }

                    if (gamePlayController.isPause)
                    {
                        currentGameState = GameState.Pause;
                    }
                    else if (gamePlayController.isContinueing)
                    {
                        currentGameState = GameState.Continuing;
                    }
                    else if (gamePlayController.isInResult)
                    {
                        currentGameState = GameState.Result;
                    }
                    else
                    {
                        currentGameState = GameState.Playing;
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Regist an IResumable which update by Application
        /// </summary>
        /// <param name="c"></param>
        public void RegistApplicationExecuter(IResumable c)
        {
            if (!applicationExecutor.Contains(c))
                applicationExecutor.Add(c);
        }

        /// <summary>
        /// Unregist an IResumable which update by Application
        /// </summary>
        /// <param name="c"></param>
        public void UnRegistApplicationExecuter(IResumable c)
        {
            if (applicationExecutor.Contains(c))
                applicationExecutor.Remove(c);
        }

        /// <summary>
        /// Get the game system instance
        /// </summary>
        /// <typeparam name="T">The game system class you wish to get</typeparam>
        /// <returns>The game system instance, null if no instance</returns>
        public T GetScriptableLifeCycle<T>() where T : ScriptableObjectLifeCycle
        {
            T result;
            result = scriptableObjectLifeCycleInstances.SingleOrDefault(m => m is T) as T;
            return result;
        }

        /// <summary>
        /// Get the game system instance
        /// </summary>
        /// <param name="t">The game system class you wish to get</param>
        /// <returns>The game system instance, null if no instance</returns>
        public object GetScriptableLifeCycle(Type t)
        {
            object result;
            result = scriptableObjectLifeCycleInstances.SingleOrDefault(m => m.GetType() == t);
            return result;
        }

        public T GetGameplayData<T>() where T : ScriptableObjectGamePlayData
        {
            T result;
            result = gamePlayDatas.SingleOrDefault(m => m is T) as T;
            return result;
        }

        public object GetGameplayData(Type t)
        {
            object result;
            result = gamePlayDatas.SingleOrDefault(m => m.GetType() == t);
            return result;
        }

        /// <summary>
        /// Get Current GamePlayController
        /// </summary>
        /// <returns>The GamePlayController instance, null if no instance</returns>
        public GamePlayController GetGamePlayController()
        {
            return gamePlayController;
        }

        /// <summary>
        /// Get the ApplicationLifeCycle instance
        /// </summary>
        /// <typeparam name="T">The ApplicationLifeCycle class you wish to get</typeparam>
        /// <returns>The ApplicationLifeCycle instance, null if no instance</returns>
        public T GetMonobehaviourLifeCycle<T>() where T : MonoBehaviourLifeCycle
        {
            T result;
            result = monoBehaviourLifeCycleInstance.SingleOrDefault(m => m is T) as T;
            return result;
        }

        /// <summary>
        /// Get the ApplicationLifeCycle instance
        /// </summary>
        /// <typeparam name="T">The ApplicationLifeCycle class you wish to get</typeparam>
        /// <returns>The ApplicationLifeCycle instance, null if no instance</returns>
        public object GetMonobehaviourLifeCycle(Type t)
        {
            object result;
            result = monoBehaviourLifeCycleInstance.SingleOrDefault(m => m.GetType() == t);
            return result;
        }

        /// <summary>
        /// Get the object instance which has Register Attribute
        /// </summary>
        /// <typeparam name="T">The Register class you wish to get</typeparam>
        /// <returns>The Register instance, null if no instance</returns>
        public object GetResloveTargetInstance(Type t)
        {
            object result;
            result = resolveTargetInstance.SingleOrDefault(m => m.GetType() == t);
            return result;
        }


        /// <summary>
        /// Start the Game
        /// </summary>
        public void StartGame()
        {
            SetIsInGame(true);
        }

        void SetIsInGame(bool isInGame)
        {
            this.isInGame = isInGame;
        }


        #region  Unity Callback

        void OnGUI()
        {
            foreach (var gd in gamePlayDatas)
            {
                gd.OnGUI();
            }
        }

        void Update()
        {
            if (applicationExecutor == null)
            {
                return;
            }

            if (!applicationExecutor.Finished)
            {
                applicationExecutor.Resume(Time.deltaTime);
            }
        }

        #endregion

        #region State
        /// <summary>
        /// An enum to explain which state the gamePlay is.
        /// </summary>
        public enum GameState
        {
            /// <summary>
            /// OutsideGamePlay, means user is "not" in the GamePlay, equal to <see cref="GamePlayController.isGaming"/> == false, usually it is also in ApplicationState.Lobby state
            /// </summary>
            OutsideGamePlay,

            /// <summary>
            /// Playing, means user is in the GamePlay, equal to <see cref="GamePlayController.isGaming"/> == true
            /// , Use <see cref="GamePlayController.alreadyContinue"/> to know is the player have continue or not. 
            /// </summary>
            Playing,

            /// <summary>
            /// In the gameplay but paused, equal to <see cref="GamePlayController.isPause"/>  == true
            /// </summary>
            Pause,

            /// <summary>
            /// GameOver and in the result, use <see cref="GamePlayController.isFailed"/> to know if the player win or lose.
            /// </summary>
            Result,

            /// <summary>
            /// Game faild and proccessing the continue flow
            /// </summary>
            Continuing,
        }
        GameState currentGameState = GameState.OutsideGamePlay;

        /// <summary>
        /// Current application state
        /// </summary>
        /// <value>See <see cref="ApplicationController.ApplicationState"/> to get more detail about each state</value>
        public GameState CurrentGameState
        {
            get
            {
                return currentGameState;
            }
        }

        /// <summary>
        /// An enum to explain which state the application is.
        /// </summary>
        public enum ApplicationState
        {
            /// <summary>
            /// Lobby is the homepage waiting for user to start the game
            /// </summary>
            Lobby,

            /// <summary>
            /// Gaming, means user is in the GamePlay, equal to <see cref="GamePlayController.isGaming"/> == true
            /// , Use <see cref="ApplicationController.GameState"/> to know the detail state of the gaming. 
            /// </summary>
            Gaming,
        }

        ApplicationState currentApplicationState = ApplicationState.Lobby;

        /// <summary>
        /// Current application state
        /// </summary>
        /// <value>See <see cref="ApplicationController.ApplicationState"/> to get more detail about each state</value>
        public ApplicationState CurrentApplicationState
        {
            get
            {
                return currentApplicationState;
            }
        }
        #endregion

        #region  Injection

        /// <summary>
        /// Create a new instance with target type and do inject immediately
        /// </summary>
        /// <param name="type">The object type to create new instance</param>
        /// <param name="inject">Inject the reference in new instance</param>
        /// <returns>The new Instance</returns>
        public object CreateInstance(Type type, bool inject = true)
        {
            object result = Activator.CreateInstance(type);
            if (IsInit && inject)
                InjectByClass(result);
            return result;
        }

        /// <summary>
        /// Create a new instance with target type and do inject immediately
        /// </summary>
        /// <param name="inject">Inject the reference in new instance</param>
        /// <typeparam name="T">The object type to create new instance </typeparam>
        public T CreateInstance<T>(bool inject = true) where T : class
        {
            return CreateInstance(typeof(T), inject) as T;
        }

        object[] GenerateResolveTargetInstance(Type[] types)
        {
            List<object> result = new List<object>();
            foreach (var item in types)
            {
                var temp = CreateInstance(item, false);
                result.Add(temp);
            }
            return result.ToArray();
        }

        IEnumerable<Type> GetAllRegisterType()
        {
            var a = typeof(IApplicationLifeCycle);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.GetCustomAttribute(typeof(ResolveTargetAttribute), true) != null)
                    .OrderBy(p => p.GetCustomAttribute<ResolveTargetAttribute>(true).order);

            // bool IsSystemLifeCycle(Type t)
            // {
            //     return
            //         !t.IsSubclassOf(typeof(MonoBehaviourLifeCycle)) &&
            //         !t.IsSubclassOf(typeof(ScriptableObjectLifeCycle)) &&
            //         !t.IsSubclassOf(typeof(GamePlayData));
            // }
        }

        /// <summary>
        /// Inject all member with [Inject] attribute on target object
        /// </summary>
        /// <param name="injectable">The object to inject</param>
        public void ResolveInjection(object injectable)
        {
            InjectByClass(injectable);
        }

        private void InjectByClass(object injectable, Type[] types = null)
        {
            Type contract = injectable.GetType();

            IEnumerable<MemberInfo> members =
            contract.FindMembers(
                MemberTypes.Property | MemberTypes.Field | MemberTypes.NestedType,
                BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                (m, i) => m.GetCustomAttribute(typeof(InjectAttribute), true) != null,
                null);

            IEnumerable<FieldInfo> fieldInfos =
                members
                .Where(m => m.MemberType == MemberTypes.Field)
                .Cast<FieldInfo>()
                .Where(m => types == null || types.Contains(m.FieldType));

            IEnumerable<PropertyInfo> propertyInfos =
                members
                .Where(m => m.MemberType == MemberTypes.Property)
                .Cast<PropertyInfo>()
                .Where(m => types == null || types.Contains(m.PropertyType));

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                var target = GetInstanceValue(fieldInfo.FieldType);
                if (target != null)
                {
                    fieldInfo.SetValue(injectable, target);
                }
            }

            foreach (PropertyInfo info in propertyInfos)
            {
                var target = GetInstanceValue(info.PropertyType);
                if (target != null)
                {
                    info.SetValue(injectable, target);
                }
            }

            object GetInstanceValue(Type t)
            {
                if (t.IsSubclassOf(typeof(GamePlayController)) || t == typeof(GamePlayController))
                {
                    return GetGamePlayController();
                }
                if (t.IsSubclassOf(typeof(ApplicationController)) || t == typeof(ApplicationController))
                {
                    return this;
                }
                if (t.IsSubclassOf(typeof(MonoBehaviourLifeCycle)))
                {
                    return GetMonobehaviourLifeCycle(t);
                }
                if (t.IsSubclassOf(typeof(ScriptableObjectLifeCycle)))
                {
                    return GetScriptableLifeCycle(t);
                }
                if (t.IsSubclassOf(typeof(ScriptableObjectGamePlayData)))
                {
                    return GetGameplayData(t);
                }

                if (typeof(IGamePlayData).IsAssignableFrom(t))
                {
                    return GetGamePlayController().GetGamePlayData();
                }
                //Finally try to find RegisterInstance
                return GetResloveTargetInstance(t);
            }
        }
        #endregion
    }

}