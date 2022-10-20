using System.Threading.Tasks;

namespace MacacaGames.GameSystem
{
    /// <summary>
    /// The life cycle interface for GameSystem
    /// Use <see cref="ScriptableObjectLifeCycle"/> or  <see cref="MonoBehaviourLifeCycle"/> for more detail
    /// </summary>
    public interface IApplicationLifeCycle
    {
        /// <summary>
        /// Init the instance,
        /// </summary>
        Task Init();

        /// <summary>
        /// Fire once between GameEnd and next GamePlay, also fire once after init. 
        /// </summary>
        void OnEnterLobby();

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