/*
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
using Amanotes.ContentReader;
using Rayark.Mast;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// Main ApplicationController for all games
/// </summary>
public class ApplicationController : UnitySingleton<ApplicationController>
{
    void Awake()
    {
        Init();
    }

    [SerializeField] GamePlayData gamePlayData;
    [SerializeField] GameSystemBase[] gameSystems;
    IApplicationLifyCycle[] applicationLifyCycles;
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

        foreach (var item in gameSystems)
        {
            var temp = Instantiate(item);
            temp.Init();
            applicationExecutor.Add(temp.OnUpdate());
            gameSystemInstances.Add(temp);
        }

        applicationLifyCycles = FindObjectsOfType<ApplicationLifeCycle>();
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
}