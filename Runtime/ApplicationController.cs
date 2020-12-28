﻿/*
/// Life cycle design
///
                        Unity App Start
                                | 
                                V
                    ApplicitionController init
                                |   [Fire OnApplicationInit() callback once]
                                V        
     Init and instance all GameSystems set in GameSystemBase[] gameSystems
                                |   
                                |   [ApplicitionController.ApplicationTask]
                                |
        ----------------------->| 
        |                       |  
        |                       |   [ApplicitionController.OnApplicationBeforeGamePlay]
        |                       |   [GameSystemBase.OnApplicationBeforeGamePlay]
        |                       V 
        |                  Game Lobby (A state for waiting for enter gameplay)
        |                       |
        |                       |
        |                       |   [ApplicationController.Instance.StartGame] 
        |                       V
        |                       |
        |                       |  
        |                       |  
        |                       |
        |                       |   [GamePlayData.GamePlay()]
        |                       | 
        |                       |
        |                       |                   
        -------------------------   [GamePlayController.SuccessGamePlay]
/// 
/// 
*/

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
    /// Main ApplicationController for all games
    /// </summary>
    public class ApplicationController : MonoBehaviour
    {
        public static ApplicationController Instance;
        void Awake()
        {
            Instance = this;
            Init();
        }

        [SerializeField] GamePlayData gamePlayData;
        [SerializeField] ScriptableObjectLifeCycle[] gameSystems;
        MonoBehaviourLifeCycle[] monoBehaviourLifeCycles;
         List<ScriptableObjectLifeCycle> scriptableObjectLifeCycleInstances = new List<ScriptableObjectLifeCycle>();
        IApplicationLifeCycle[] applicationLifeCycles;

        /// <summary>
        /// Fire while application init, only fire once
        /// Usually to use in other scene before Scene with ApplicationController
        /// e.g. Loading scene
        /// </summary>
        Action OnApplicationInit;

        /// <summary>
        /// Fire once between GameEnd and next GamePlay, also fire once after init. 
        /// </summary>
        Action OnApplicationBeforeGamePlay;

        GamePlayController gamePlayController;

        public void Init()
        {
            OnApplicationInit?.Invoke();
            gamePlayController = new GamePlayController(this, gamePlayData);
            gamePlayController.Init();
            applicationExecutor = new Executor();

            //Prepare Instance
            foreach (var item in gameSystems)
            {
                var temp = Instantiate(item);
                scriptableObjectLifeCycleInstances.Add(temp);
            }
            monoBehaviourLifeCycles = Resources.FindObjectsOfTypeAll<MonoBehaviourLifeCycle>().Where(
                (m) =>
                m.gameObject != null &&
                m.gameObject.scene.IsValid()).ToArray();

            //Inject Dependency
            InjectByClass(gamePlayData);

            foreach (var item in scriptableObjectLifeCycleInstances)
            {
                InjectByClass(item);
            }

            foreach (var item in monoBehaviourLifeCycles)
            {
                InjectByClass(item);
            }

            //Init
            foreach (var item in scriptableObjectLifeCycleInstances)
            {
                item.Init();
            }
            foreach (var item in monoBehaviourLifeCycles)
            {
                item.Init();
            }

            applicationExecutor.Add(ApplicationTask());
            applicationLifeCycles = scriptableObjectLifeCycleInstances.Cast<IApplicationLifeCycle>()
                                        .Union(monoBehaviourLifeCycles.Cast<IApplicationLifeCycle>())
                                        .ToArray();

            applicationExecutor.Add(ApplicationUpdateRunner());
        }

        IEnumerator ApplicationUpdateRunner()
        {
            while (true)
            {
                foreach (var item in applicationLifeCycles)
                {
                    try
                    {
                        item.OnApplicationUpdate();
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
                foreach (var item in applicationLifeCycles)
                {
                    try
                    {
                        item.OnGamePlayUpdate();
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
                foreach (var item in applicationLifeCycles)
                {
                    try
                    {
                        item.OnGamePlayUpdate();
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
                isInGame = false;
                OnApplicationBeforeGamePlay?.Invoke();
                gamePlayController.OnApplicationBeforeGamePlay();

                foreach (var item in applicationLifeCycles)
                {
                    try
                    {
                        item.OnApplicationBeforeGamePlay();
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
        public T GetGameSystem<T>() where T : ScriptableObjectLifeCycle
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
        public object GetGameSystem(Type t)
        {
            object result;
            result = scriptableObjectLifeCycleInstances.SingleOrDefault(m => m.GetType() == t);
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
        public T GetApplicationLifeCycle<T>() where T : MonoBehaviourLifeCycle
        {
            T result;
            result = monoBehaviourLifeCycles.SingleOrDefault(m => m is T) as T;
            return result;
        }

        /// <summary>
        /// Get the ApplicationLifeCycle instance
        /// </summary>
        /// <typeparam name="T">The ApplicationLifeCycle class you wish to get</typeparam>
        /// <returns>The ApplicationLifeCycle instance, null if no instance</returns>
        public object GetApplicationLifeCycle(Type t)
        {
            object result;
            result = monoBehaviourLifeCycles.SingleOrDefault(m => m.GetType() == t);
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

        //You should exit game through GamePlayController.QuitGamePlay
        //public void ExitGame()
        //{
        //    gamePlayController.QuitGamePlay();
        //}
        #region  Unity Callback

        void OnGUI()
        {
            gamePlayData.OnGUI();
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
        #region  Injection
        public void ResolveInjection(IApplicationInjectable injectable)
        {
            InjectByClass(injectable);
        }

        private void InjectByClass(IApplicationInjectable injectable)
        {
            Type contract = injectable.GetType();

            MemberInfo[] members = contract.FindMembers(
                MemberTypes.Property | MemberTypes.Field | MemberTypes.NestedType,
                BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
            null, null);

            foreach (MemberInfo member in members)
            {
                object[] attrs = member.GetCustomAttributes(typeof(InjectAttribute), true);

                foreach (object attr in attrs)
                {
                    if (attr.GetType() != typeof(InjectAttribute))
                    {
                        continue;
                    }

                    if (member.MemberType == MemberTypes.Field)
                    {
                        System.Reflection.FieldInfo fieldInfo = member as FieldInfo;
                        if (fieldInfo != null)
                            fieldInfo.SetValue(injectable, GetInstanceValue(fieldInfo.FieldType));
                    }
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        System.Reflection.PropertyInfo info = member as PropertyInfo;
                        if (info != null)
                            info.SetValue(injectable, GetInstanceValue(info.PropertyType));
                    }
                    else
                    {
                        Debug.LogError($"Unsupport member type {member.MemberType}");
                    }
                }
            }

            object GetInstanceValue(Type t)
            {
                if (t.IsSubclassOf(typeof(MonoBehaviourLifeCycle)))
                {
                    return GetApplicationLifeCycle(t);
                }
                if (t.IsSubclassOf(typeof(ScriptableObjectLifeCycle)))
                {
                    return GetGameSystem(t);
                }
                if (t.IsSubclassOf(typeof(GamePlayData)))
                {
                    return GetGamePlayController().GetGamePlayData();
                }
                return null;
            }
        }
        #endregion
    }

    public interface IApplicationInjectable
    {

    }

    /// <summary>
    /// Mark a Property or Field inside IApplicationInjectable that can be Injected by ApplicationController
    /// Remember the member needs to be accessable to make the Inject work.
    /// 
    /// e.g. In the case while trying to inject ChildClass the inject will has below result 
    /// 
    /// class BaseClass : IApplicationInjectable{
    ///     SomeClass canNotBeInject;
    ///     protected SomeClass canBeInject;
    ///     public SomeClass alsoCanBeInject;
    /// }
    /// 
    /// class ChildClass : BaseClass{
    ///     
    /// }
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : Attribute { }
}