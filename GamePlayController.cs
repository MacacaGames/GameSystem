/*
                         OnGameValueReset
                                | 
                                V
                           OnEnterGame
                                | 
                                V
                [Loop]currentGamePlayData.GamePlay()
                                | 
                                V
                           OnLeaveGame();
                                | 
                                V
                            OnGameEnd
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amanotes.ContentReader;
using System.IO;
using Rayark.Mast;
using System;

public class GamePlayController : IApplicationLifyCycle
{
    GamePlayData currentGamePlayData;
    private ApplicationController _applicationController;

    public GamePlayController(ApplicationController _applicationController, GamePlayData gamePlayData)
    {
        this._applicationController = _applicationController;
        currentGamePlayData = gamePlayData;
        currentGamePlayData.SetGamePlayController(this);
        currentGamePlayData.SetApplicationController(_applicationController);
    }

    public void Init()
    {
        currentGamePlayData?.Init();
    }

    public void OnApplicationBeforeGamePlay()
    {
        currentGamePlayData?.OnApplicationBeforeGamePlay();
    }


    public GamePlayData GetGamePlayData()
    {
        return currentGamePlayData;
    }

    public T GetGamePlayData<T>() where T : GamePlayData
    {
        return currentGamePlayData as T;
    }


    public Executor StartGamePlay()
    {
        ResetGamePlayValue();
        currentGamePlayData.OnGameValueReset();
        gamePlayUpdateExecuter.Clear();
        return new Executor
        {
            EnterGame()
        };
    }

    IEnumerator EnterGame()
    {
        yield return currentGamePlayData.OnEnterGame();
        isGaming = true;
        yield return GamePlayTask();
    }

    /// <summary>
    /// End and exit the gameplay,used on Game Clear
    /// </summary>
    public void EndGamePlay()
    {
        Rayark.Mast.Coroutine coroutine = new Rayark.Mast.Coroutine(GameOverFlow());
        AddToUnpauseUpdateExecuter(coroutine);
    }

    /// <summary>
    /// Failed and exit the gameplay, this will fire continue flow
    /// </summary>
    public void FailedGamePlay()
    {
        isFailed = true;
        Rayark.Mast.Coroutine coroutine = new Rayark.Mast.Coroutine(GameOverFlow());
        AddToUnpauseUpdateExecuter(coroutine);
    }

    /// <summary>
    /// Quit and exit the gameplay, usually used on UI to quit.
    /// </summary>
    public void QuitGamePlay()
    {
        isQuiting = true;
        isFailed = true;
        isPause = false;
        isGaming = false;
        isPlayerDied = false;
    }

    void EndGameDueToPlayerDied()
    {
        isPlayerDied = true;
        isGaming = false;
    }

    public void EnterPause()
    {
        isPause = true;
    }

    public void EndPause()
    {
        isPause = false;
    }

    public void ResetGamePlayValue()
    {
        isInResult = false;
        isQuiting = false;
        isFailed = false;
        isPause = false;
        isPlayerDied = false;
        alreadyContinue = false;
        isContinueing = false;
    }

    #region GamePlayLogic

    /// <summary>
    /// Is GamePlay Runing?
    /// </summary>
    /// <value></value>
    public bool isGaming { get; private set; }

    /// <summary>
    /// Is GamePlay Pause, value will keep until next gameplay start.
    /// </summary>
    /// <value></value>
    public bool isPause { get; private set; }

    /// <summary>
    /// Is GamePlay Continue progress working.
    /// </summary>
    /// <value></value>
    public bool isContinueing { get; private set; } = false;

    /// <summary>
    /// Is GamePlay Quiting, value will keep until next gameplay start.
    /// </summary>
    /// <value></value>
    public bool isQuiting { get; private set; } = false;

    /// <summary>
    /// Is GamePlay Pause, value will keep until next gameplay start.
    /// </summary>
    /// <value></value>
    public bool isFailed { get; private set; } = false;

    /// <summary>
    /// Is In Result , value will keep until next gameplay start.
    /// </summary>
    /// <value></value>
    public bool isInResult { get; private set; } = false;

    /// <summary>
    /// Is GamePlay PlayerDied, value will keep until next gameplay start.
    /// </summary>
    /// <value></value>
    public bool isPlayerDied { get; private set; } = false;

    /// <summary>
    /// Is GamePlay alreadyContinue, value will keep until next gameplay start.
    /// </summary>
    /// <value></value>
    public bool alreadyContinue { get; private set; } = false;

    IEnumerator GamePlayTask()
    {
        Rayark.Mast.Coroutine gamePlayCoroutine = new Rayark.Mast.Coroutine(currentGamePlayData.GamePlay());

        // while (!onEnterGameCoroutine.Finished)
        // {
        //     onEnterGameCoroutine.Resume(Rayark.Mast.Coroutine.Delta);
        //     yield return null;
        // }

        while (isQuiting == false && isGaming == true)
        {
            gamePlayCoroutine.Resume(Rayark.Mast.Coroutine.Delta);
            yield return null;
        }

        currentGamePlayData.OnLeaveGame();
        //Normal Game Ending, can be player died or clear
        if (isQuiting == false)
        {
            if (isFailed == true)
            {
                //Player died
                currentGamePlayData.OnGameFaild();
            }
            else
            {
                //Player clear
                currentGamePlayData.OnGameClear();
            }

            isInResult = true;
            Rayark.Mast.Coroutine gameResultCoroutine = new Rayark.Mast.Coroutine(currentGamePlayData.GameResult());
            while (!gameResultCoroutine.Finished)
            {
                gameResultCoroutine.Resume(Rayark.Mast.Coroutine.Delta);
                yield return null;
            }

            isInResult = false;
        }
        //Game Ending by external reason, e.g. Quit from UI
        else
        {
            isQuiting = false;
        }

        currentGamePlayData.OnGameEnd();
        isGaming = false;
    }

  

    public IEnumerator GameOverFlow()
    {
        //關卡結束，代表玩家獲勝
        if (isFailed == false)
        {
            isPlayerDied = false;
            isGaming = false;
        }
        //否則表示死亡，進接關流程
        else
        {
            EnterPause();

            //已經接關過
            if (alreadyContinue || !currentGamePlayData.IsContinueAvailable)
            {
                EndGameDueToPlayerDied();
            }
            //尚未接關過
            else
            {
                isContinueing = true;
                var m = new BlockMonad<bool>(r => currentGamePlayData.OnContinueFlow(r));
                yield return m.Do();

                // Continue success
                if (m.Result)
                {
                    currentGamePlayData.OnContinue();
                    isFailed = false;
                    isPlayerDied = false;
                }
                else
                {
                    isContinueing = false;
                    EndGameDueToPlayerDied();
                }

                yield return new WaitUntil(() => isContinueing == false);
            }

            EndPause();
        }
    }

    #endregion

    #region GameLifeCycle

    public Executor gamePlayUpdateExecuter = new Executor();
    public Executor gamePlayUnpauseUpdateExecuter = new Executor();

    public void AddToUnpauseUpdateExecuter(IResumable c)
    {
        if (!gamePlayUnpauseUpdateExecuter.Contains(c))
            gamePlayUnpauseUpdateExecuter.Add(c);
    }

    public void RemoveFromUnpauseUpdateExecuter(IResumable c)
    {
        if (gamePlayUnpauseUpdateExecuter.Contains(c))
            gamePlayUnpauseUpdateExecuter.Remove(c);
    }

    public void AddToUpdateExecuter(IResumable c)
    {
        if (!gamePlayUpdateExecuter.Contains(c))
            gamePlayUpdateExecuter.Add(c);
    }

    public void RemoveFromUpdateExecuter(IResumable c)
    {
        if (gamePlayUpdateExecuter.Contains(c))
            gamePlayUpdateExecuter.Remove(c);
    }

    #endregion
}