using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rayark.Mast;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    public abstract class ScriptableObjectGamePlayData : ScriptableObject, IGamePlayData
    {
        [Inject]
        public GamePlayController gamePlayController
        {
            get;
            set;
        }

        [Inject]
        public ApplicationController applicationController
        {
            get;
            set;
        }

        /// <summary>
        /// Same as ApplicationController Init.
        /// </summary>
        public abstract void Init();


        /// <summary>
        /// Same as ApplicationController OnApplicationBeforeGamePlay.
        /// </summary>
        public abstract void OnEnterLobby();

        /// <summary>
        /// Reset all gameplay relalte value
        /// </summary>
        public abstract void OnGameValueReset();

        /// <summary>
        /// Excude before gameplay realy start
        /// </summary>
        public abstract Task OnEnterGame();

        /// <summary>
        /// Main logic of the game
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator GamePlay();


        /// <summary>
        /// Excude after GamePlay() is end, but before GameResult()
        /// </summary>
        public abstract void OnLeaveGame();

        /// <summary>
        /// Execude after GamePlay() is end, but before GameResult(), exclude Quitting & Failing
        /// </summary>
        public abstract void OnGameSuccess();

        /// <summary>
        /// Game Lose .
        /// </summary>
        public abstract void OnGameLose();

        /// <summary>
        /// Only work during play died or clear, not on quit
        /// </summary>
        /// <returns></returns>
        public abstract Task GameResult();

        /// <summary>
        /// Game total end, no mater is died, clear or quit.
        /// </summary>
        public abstract void OnGameEnd();

        /// <summary>
        /// Game Failed.
        /// </summary>
        public abstract void OnGameFaild();

        /// <summary>
        /// Implement the continue progress here and set result in result 
        /// </summary>
        /// <param name="result"> the reture value for the continue progress</param>
        /// <returns></returns>
        public abstract Task<bool> OnContinueFlow();

        /// <summary>
        /// To verify is the game play available to continue
        /// </summary>
        /// <value></value>
        public abstract bool IsContinueAvailable { get; }



        /// <summary>
        /// While the result in OnContinueFlow is true, do anything require to continue the gameplay on this callback.
        /// </summary>
        public abstract void OnContinue();

        public abstract void OnGUI();

        public abstract void OnChangeGamePlayData_Launch();

        public abstract void OnChangeGamePlayData_Retire();
    }
}