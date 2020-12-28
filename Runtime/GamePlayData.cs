using System.Collections;
using System.Collections.Generic;
using Rayark.Mast;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    /// <summary>
    /// Game state define
    /// Success : Player finished gameplay, usually means player wins.
    /// Lose : GamePlay is ended due to player is died. 
    /// Faild : Player is died, Player may died muilple times during one gameplay
    /// Quit : Player give up the gameplay, usually leave gameplay by UI.
    /// </summary>
    public abstract class GamePlayData : ScriptableObject, IApplicationInjectable
    {
        GamePlayController _gamePlayController;
        ApplicationController _applicationController;

        public void SetGamePlayController(GamePlayController _gamePlayController)
        {
            this._gamePlayController = _gamePlayController;
        }

        public GamePlayController gamePlayController
        {
            get { return _gamePlayController; }
        }

        public void SetApplicationController(ApplicationController _applicationController)
        {
            this._applicationController = _applicationController;
        }

        protected ApplicationController applicationController
        {
            get { return _applicationController; }
        }


        /// <summary>
        /// Main logic of the game
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator GamePlay();

        /// <summary>
        /// Only work during play died or clear, not on quit
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator GameResult();

        /// <summary>
        /// Reset all gameplay relalte value
        /// </summary>
        public abstract void OnGameValueReset();

        /// <summary>
        /// Excude before gameplay realy start
        /// </summary>
        public abstract IEnumerator OnEnterGame();

        /// <summary>
        /// Excude after GamePlay() is end, but before GameResult()
        /// </summary>
        public abstract void OnLeaveGame();

        /// <summary>
        /// Game total end, no mater is died, clear or quit.
        /// </summary>
        public abstract void OnGameEnd();

        /// <summary>
        /// Execude after GamePlay() is end, but before GameResult(), exclude Quitting & Failing
        /// </summary>
        public abstract void OnGameSuccess();
        /// <summary>
        /// Game Lose .
        /// </summary>
        public abstract void OnGameLose();

        /// <summary>
        /// Game Failed.
        /// </summary>
        public abstract void OnGameFaild();

        /// <summary>
        /// Implement the continue progress here and set result in result 
        /// </summary>
        /// <param name="result"> the reture value for the continue progress</param>
        /// <returns></returns>
        public abstract IEnumerator OnContinueFlow(IReturn<bool> result);

        /// <summary>
        /// To verify is the game play available to continue
        /// </summary>
        /// <value></value>
        public abstract bool IsContinueAvailable { get; }

        /// <summary>
        /// While the result in OnContinueFlow is true, do anything require to continue the gameplay on this callback.
        /// </summary>
        public abstract void OnContinue();

        /// <summary>
        /// Same as ApplicationController Init.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Same as ApplicationController OnApplicationBeforeGamePlay.
        /// </summary>
        public abstract void OnApplicationBeforeGamePlay();
        public abstract void OnGUI();

    }
}