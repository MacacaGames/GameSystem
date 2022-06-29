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
using System.IO;
using Rayark.Mast;
using System;
using System.Threading.Tasks;

namespace MacacaGames.GameSystem
{
    public class GamePlayController
    {
        IGamePlayData currentGamePlayData;
        private ApplicationController _applicationController;

        public GamePlayController(ApplicationController _applicationController, IGamePlayData gamePlayData)
        {
            this._applicationController = _applicationController;
            ChangeGamePlayData(gamePlayData);
            currentGamePlayData = gamePlayData;
        }

        public void ChangeGamePlayData(IGamePlayData iGameplayData)
        {
            if (currentGamePlayData != null)
            {
                currentGamePlayData.OnChangeGamePlayData_Retire();
            }

            currentGamePlayData = iGameplayData;
            iGameplayData.OnChangeGamePlayData_Launch();
        }

        public void Init()
        {
        }

        public void OnEnterLobby()
        {
            currentGamePlayData?.OnEnterLobby();
        }

        /// <summary>
        /// Get the current GamePlayData 
        /// </summary>
        /// <returns>GamePlayData</returns>
        public IGamePlayData GetGamePlayData()
        {
            return currentGamePlayData;
        }

        /// <summary>
        /// Get the current GamePlayData and convert to target case
        /// </summary>
        /// <typeparam name="T">Target Type</typeparam>
        /// <returns>GamePlayData</returns>
        public T GetGamePlayData<T>() where T : class, IGamePlayData
        {
            return currentGamePlayData as T;
        }

        public Executor GamePlayControllerCoreLoop(IEnumerator gamePlayUpdate, IEnumerator unPauseGamePlayUpdate)
        {
            ResetGamePlayValue();
            currentGamePlayData.OnGameValueReset();
            gamePlayUpdateExecuter.Clear();
            gamePlayUnpauseUpdateExecuter.Clear();
            gamePlayUpdateExecuter.Add(gamePlayUpdate);
            gamePlayUnpauseUpdateExecuter.Add(unPauseGamePlayUpdate);
            return new Executor
            {
                EnterGame()
            };
        }

        IEnumerator EnterGame()
        {
            // yield return currentGamePlayData.OnEnterGame();
            var enterGameTask = currentGamePlayData.OnEnterGame();
            yield return new WaitForTaskCompletion(enterGameTask);
            isGaming = true;
            yield return GamePlayTask();
        }

        /// <summary>
        /// End and exit the gameplay,used on Game Clear
        /// </summary>
        public void SuccessGamePlay()
        {
            //Since the normal gameplay loop coroutine end means the game is finish normally
            //just endthegame
            EndTheGame();
        }

        /// <summary>
        /// Failed and exit the gameplay, this will fire continue flow(if continue is available)
        /// <see cref="GameFaildFlow()"/> for the contniue behaviour
        /// or <see cref="ScriptableObjectGamePlayData.OnContinueFlow(IReturn{bool})()"/> for your continue implement 
        /// </summary>
        public async void FailedGamePlay()
        {
            isFailed = true;
            await GameFaildFlow();
        }

        /// <summary>
        /// Quit and exit the gameplay, usually used on UI to quit.
        /// </summary>
        public void QuitGamePlay()
        {
            isQuiting = true;
            EndTheGame();
        }

        void EndTheGame()
        {
            isGaming = false;
        }

        /// <summary>
        /// Make the game Pause, use <see cref="ResumePause()"/> to Resume from Pause
        /// </summary>
        public void EnterPause()
        {
            isPause = true;
        }

        /// <summary>
        /// Resume the game Pause, use <see cref="EnterPause()"/> to Enter pause
        /// </summary>
        public void ResumePause()
        {
            isPause = false;
        }

        void ResetGamePlayValue()
        {
            isQuiting = false;
            isFailed = false;
            isPause = false;
            // alreadyContinue = false;
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
        /// Should be only modify the value by <see cref="EnterPause()"/> or  <see cref="ResumePause()"/>
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
        /// Is GamePlay Failed, value will keep until next gameplay start.
        /// </summary>
        /// <value></value>
        public bool isFailed { get; private set; } = false;

        /// <summary>
        /// Is In Result , value will keep until next gameplay start.
        /// </summary>
        /// <value></value>
        public bool isInResult
        {
            get
            {
                return gameResultCoroutine != null && !gameResultCoroutine.Finished;
            }
        }
        Rayark.Mast.Coroutine gameResultCoroutine = null;

        // /// <summary>
        // /// Is GamePlay alreadyContinue, value will keep until next gameplay start.
        // /// </summary>
        // /// <value></value>
        // public bool alreadyContinue { get; private set; } = false;

        IEnumerator GamePlayTask()
        {
            Rayark.Mast.Coroutine gamePlayCoroutine = new Rayark.Mast.Coroutine(currentGamePlayData.GamePlay());

            while (isQuiting == false && isGaming == true)
            {
                if (!gamePlayCoroutine.Finished)
                    gamePlayCoroutine.Resume(Rayark.Mast.Coroutine.Delta);
                else
                {
                    Debug.LogError("gamePlayCoroutine is finished, Remember to call   gamePlayController.SuccessGamePlay(); in the end of GamePlay IEnumrator");
                    Debug.Break();
                }
                yield return null;
            }

            currentGamePlayData.OnLeaveGame();
            //Normal Game Ending, can be player died or clear
            if (isQuiting == false)
            {
                if (isFailed == true)
                {
                    //Game Faild
                    currentGamePlayData.OnGameLose();
                }
                else
                {
                    //Game Success
                    currentGamePlayData.OnGameSuccess();
                }

                var resultTask = currentGamePlayData.GameResult();
                yield return new WaitForTaskCompletion(resultTask);
            }
            //Game Ending by external reason, e.g. Quit from UI
            else
            {
                isQuiting = false;
            }

            currentGamePlayData.OnGameEnd();
            EndTheGame();
        }

        /// <summary>
        /// Fire after calling <see cref="FailedGamePlay()"/>
        /// Will proccess continue logic during this phase.
        /// Note. during GameFaildFlow the GamePlay is still running but keeps in pause status.
        /// This flow may fire mutilple times during one gameplay.
        /// </summary>
        /// <returns></returns>
        async Task GameFaildFlow()
        {
            EnterPause();
            currentGamePlayData.OnGameFaild();
            //已經接關過
            if (!currentGamePlayData.IsContinueAvailable)
            {
                EndTheGame();
            }
            //尚未接關過
            else
            {
                isContinueing = true;
                var result = await currentGamePlayData.OnContinueFlow();

                // Continue success
                if (result)
                {
                    currentGamePlayData.OnContinue();
                    isFailed = false;
                    isContinueing = false;
                    // alreadyContinue = true;
                }
                else
                {
                    isContinueing = false;
                    EndTheGame();
                }
            }

            ResumePause();
        }

        #endregion

        #region GameLifeCycleExcuter

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
}

/// <summary>
/// WaitForStandardYieldInstruction is a helper to make a YieldInstruction into CustomYieldInstruction
/// Therefore you can iterate it with a third-party Coroutine implement, e.g. Rayark.Mast
/// </summary>
public class WaitForTaskCompletion : CustomYieldInstruction
{
    bool isRunning;

    Task _task;

    public WaitForTaskCompletion(Task task)
    {
        _task = task;
        WaitForTaskCompletionImplement();
    }

    public async void WaitForTaskCompletionImplement()
    {
        isRunning = true;
        await _task;
        isRunning = false;
    }

    public override bool keepWaiting
    {
        get
        {
            return isRunning;
        }
    }
}