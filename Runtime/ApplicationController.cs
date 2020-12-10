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
        |                       |   [GamePlayData.StartGamePlay]
        |                       V
        |                       |
        |                       |  
        |                       |  
        |                       |
        |                       |   [GamePlayData.GamePlay()]
        |                       | 
        |                       |
        |                       |                   
        -------------------------   [ApplicitionController.OnApplicationAfterGamePlay]
                                    [GameSystemBase.OnApplicationAfterGamePlay]


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
        [SerializeField] GameSystemBase[] gameSystems;
        ApplicationLifeCycle[] applicationLifyCycles;
        List<GameSystemBase> gameSystemInstances = new List<GameSystemBase>();


        /// <summary>
        /// Fire while application init, only fire once
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
                gameSystemInstances.Add(temp);
            }
            applicationLifyCycles = FindObjectsOfType<ApplicationLifeCycle>();

            //Inject Dependency
            InjectByClass(gamePlayData);

            foreach (var item in gameSystemInstances)
            {
                InjectByClass(item);
            }

            foreach (var item in applicationLifyCycles)
            {
                InjectByClass(item);
            }

            //Init
            foreach (var item in gameSystemInstances)
            {
                item.Init();
                applicationExecutor.Add(item.OnUpdate());
            }
            foreach (var item in applicationLifyCycles)
            {
                item.Init();
            }


            applicationExecutor.Add(ApplicationTask());
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

        Executor applicationExecutor;
        Executor gamePlayTaskExecutor;
        public bool isInGame { get; private set; } = false;

        IEnumerator ApplicationTask()
        {
            while (true)
            {
                isInGame = false;
                OnApplicationBeforeGamePlay?.Invoke();
                gamePlayController.OnApplicationBeforeGamePlay();
                foreach (var item in gameSystemInstances)
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

                foreach (var item in applicationLifyCycles)
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
                gamePlayTaskExecutor = gamePlayController.StartGamePlay();
                while (!gamePlayTaskExecutor.Finished)
                {
                    gamePlayController.gamePlayUnpauseUpdateExecuter.Resume(Rayark.Mast.Coroutine.Delta);

                    if (gamePlayController.isPause == false && gamePlayController.isContinueing == false)
                    {
                        gamePlayController.gamePlayUpdateExecuter.Resume(Rayark.Mast.Coroutine.Delta);
                        gamePlayTaskExecutor.Resume(Rayark.Mast.Coroutine.Delta);
                    }

                    yield return null;
                }
            }
        }

        public void RegistApplicationExecuter(IResumable c)
        {
            if (!applicationExecutor.Contains(c))
                applicationExecutor.Add(c);
        }

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
        public T GetGameSystem<T>() where T : GameSystemBase
        {
            T result;
            result = gameSystemInstances.SingleOrDefault(m => m is T) as T;
            return result;
        }
        public object GetGameSystem(Type t)
        {
            object result;
            result = gameSystemInstances.SingleOrDefault(m => m.GetType() == t);
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
        public T GetApplicationLifeCycle<T>() where T : ApplicationLifeCycle
        {
            T result;
            result = applicationLifyCycles.SingleOrDefault(m => m is T) as T;
            return result;
        }

        public object GetApplicationLifeCycle(Type t)
        {
            object result;
            result = applicationLifyCycles.SingleOrDefault(m => m.GetType() == t);
            return result;
        }

        public void ResolveInjection(IApplicationInjectable injectable)
        {
            InjectByClass(injectable);
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

        public void ExitGame()
        {
            gamePlayController.QuitGamePlay();
        }

        void OnGUI()
        {
            gamePlayData.OnGUI();
        }

        private void InjectByClass(IApplicationInjectable injectable)
        {
            Type contract = injectable.GetType();

            MemberInfo[] members = contract.FindMembers(
                MemberTypes.Property | MemberTypes.Field,
                BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
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
                if (t.IsSubclassOf(typeof(ApplicationLifeCycle)))
                {
                    return GetApplicationLifeCycle(t);
                }
                if (t.IsSubclassOf(typeof(GameSystemBase)))
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
    }
    public interface IApplicationInjectable
    {

    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : Attribute { }
}