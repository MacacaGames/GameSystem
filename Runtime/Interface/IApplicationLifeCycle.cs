namespace MacacaGames.GameSystem
{
    interface IApplicationLifeCycle
    {
        /// <summary>
        /// Init the instance,
        /// </summary>
        void Init();

        /// <summary>
        /// Fire once between GameEnd and next GamePlay, also fire once after init. 
        /// </summary>
        void OnApplicationBeforeGamePlay();

        /// <summary>
        /// Fire every frame during the application is running
        /// </summary>
        void OnApplicationUpdate();

        /// <summary>
        /// Fire every frame during the gameplay loop is running
        /// (While ApplicationController.Instance.IsInGame == true)
        /// </summary>
        void OnGamePlayUpdate();
        /// <summary>
        /// Fire every frame during the gameplay loop is running
        /// (While ApplicationController.Instance.IsInGame == true)
        /// </summary>
        void OnUnPauseGamePlayUpdate();
    }

}