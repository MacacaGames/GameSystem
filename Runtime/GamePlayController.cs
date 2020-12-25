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
namespace MacacaGames.GameSystem
{
    public class GamePlayController
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

        /// <summary>
        /// Get the current GamePlayData 
        /// </summary>
        /// <returns>GamePlayData</returns>
        public GamePlayData GetGamePlayData()
        {
            return currentGamePlayData;
        }

        /// <summary>
        /// Get the current GamePlayData and convert to target case
        /// </summary>
        /// <typeparam name="T">Target Type</typeparam>
        /// <returns>GamePlayData</returns>
        public T GetGamePlayData<T>() where T : GamePlayData
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
            yield return currentGamePlayData.OnEnterGame();
            isGaming = true;
            yield return GamePlayTask();
        }

        /// <summary>
        /// End and exit the gameplay,used on Game Clear
        /// </summary>
        public void SuccessGamePlay()
        {
            isGaming = false;
        }

        /// <summary>
        /// Failed and exit the gameplay, this will fire continue flow(if continue is available)
        /// <see cref="GameOverFlow()"/> for the contniue behaviour
        /// or <see cref="GamePlayData.OnContinueFlow(IReturn{bool})()"/> for your continue implement 
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
                if (!gamePlayCoroutine.Finished)
                    gamePlayCoroutine.Resume(Rayark.Mast.Coroutine.Delta);
                else
                {
                    Debug.LogError("gamePlayCoroutine is finished");
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
                    //Player died
                    currentGamePlayData.OnGameFaild();
                }
                else
                {
                    //Player clear
                    currentGamePlayData.OnGameSuccess();
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

        /// <summary>
        /// Fire after calling <see cref="FailedGamePlay()"/>
        /// Player continue flow is runing in the flow too.
        /// </summary>
        /// <returns></returns>
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
                        isContinueing = false;
                        alreadyContinue = true;
                    }
                    else
                    {
                        isContinueing = false;
                        EndGameDueToPlayerDied();
                    }

                    yield return new WaitUntil(() => isContinueing == false);
                }

                ResumePause();
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
}