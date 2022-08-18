using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rayark.Mast;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    // <summary>
    /// Game state define
    /// Success : Player finished gameplay, usually means player wins.
    /// Lose : GamePlay is ended due to player is died. 
    /// Faild : Player is died, Player may died muilple times during one gameplay
    /// Quit : Player give up the gameplay, usually leave gameplay by UI.
    /// </summary>
    public interface IGamePlayData
    {
        GamePlayController gamePlayController { get; set; }
        ApplicationController applicationController { get; set; }

        /// <summary>
        /// Same as ApplicationController Init.
        /// </summary>
        void Init();

        /// <summary>
        /// Fire when change gameplay to this
        /// </summary>
        void OnChangeGamePlayData_Launch();

        /// <summary>
        /// Fire when change gameplay to other
        /// </summary>
        void OnChangeGamePlayData_Retire();

        /// <summary>
        /// Same as ApplicationController OnApplicationBeforeGamePlay.
        /// </summary>
        void OnEnterLobby();

        /// <summary>
        /// Reset all gameplay relalte value
        /// </summary>
        void OnGameValueReset();

        /// <summary>
        /// Excude before gameplay realy start
        /// </summary>
        Task OnEnterGame();

        /// <summary>
        /// Main logic of the game
        /// </summary>
        /// <returns></returns>
        IEnumerator GamePlay();


        /// <summary>
        /// Excude after GamePlay() is end, but before GameResult()
        /// </summary>
        void OnLeaveGame();

        /// <summary>
        /// Execude after GamePlay() is end, but before GameResult(), exclude Quitting & Failing
        /// </summary>
        void OnGameSuccess();

        /// <summary>
        /// Game Lose .
        /// </summary>
        void OnGameLose();

        /// <summary>
        /// Only work during play died or clear, not on quit
        /// </summary>
        Task GameResult();

        /// <summary>
        /// Game total end, no mater is died, clear or quit.
        /// </summary>
        void OnGameEnd();

        /// <summary>
        /// Game Failed.
        /// </summary>
        Task OnGameFaild();

        /// <summary>
        /// Implement the continue progress here and set result in result 
        /// </summary>
        /// <returns>True if the player would like to continue</returns>
        Task<bool> OnContinueFlow();

        /// <summary>
        /// To verify is the game play available to continue
        /// </summary>
        /// <value></value>
        bool IsContinueAvailable { get; }

        /// <summary>
        /// While the result in OnContinueFlow is true, do anything require to continue the gameplay on this callback.
        /// </summary>
        void OnContinue();

        void OnGUI();
    }
}