﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rayark.Mast;
using Coroutine = Rayark.Mast.Coroutine;
using System.Threading.Tasks;

namespace MacacaGames.GameSystem
{
    /// <summary>
    /// ScriptableObject based ApplicationLifeCycle object
    /// </summary>
    public abstract class ScriptableObjectLifeCycle : ScriptableObject, IApplicationLifeCycle
    {
        /// <summary>
        /// Init the object, fire once during ApplicationController.Init()
        /// </summary>
        public abstract Task Init();

        /// <summary>
        /// Fire every frame during the application is running, likes UnityEngine.Update() but controls by ApplicationController
        /// </summary>
        public virtual void OnApplicationUpdate() { }

        /// <summary>
        /// Fire once between GameEnd and next GamePlay, also fire once after init. 
        /// </summary>  
        public virtual void OnEnterLobby() { }

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