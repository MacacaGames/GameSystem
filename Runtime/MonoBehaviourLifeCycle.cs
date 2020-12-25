using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    /// <summary>
    /// MonoBehaviour based ApplicationLifeCycle object
    /// </summary>
    public abstract class MonoBehaviourLifeCycle : MonoBehaviour, IApplicationLifeCycle, IApplicationInjectable
    {
        /// <summary>
        /// Init the object, fire once during ApplicationController.Init()
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Fire every frame during the application is running, likes UnityEngine.Update() but controls by ApplicationController
        /// </summary>
        public virtual void OnApplicationUpdate() { }

        /// <summary>
        /// Fire once between GameEnd and next GamePlay, also fire once after init. 
        /// </summary>  
        public virtual void OnApplicationBeforeGamePlay() { }

        /// <summary>
        /// Fire every frame during the gameplay loop is running and will also pause while GamePlay is paused
        /// (While ApplicationController.Instance.IsInGame == true)
        /// </summary> 
        public virtual void OnGamePlayUpdate() { }

        /// <summary>
        /// Fire every frame during the gameplay loop is running, keeps running during GamePlay is pause
        /// (While ApplicationController.Instance.IsInGame == true)
        /// </summary>
        public virtual void OnUnPauseGamePlayUpdate() { }
    }
}