﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    public abstract class MonoBehaviourLifeCycle : MonoBehaviour, IApplicationLifeCycle, IApplicationInjectable
    {
        /// <summary>
        /// <see cref="IApplicationInjectable.Init()"/>
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// <see cref="IApplicationInjectable.OnApplicationUpdate()"/>
        /// </summary>
        public virtual void OnApplicationUpdate(){}

        /// <summary>
        /// <see cref="IApplicationInjectable.OnApplicationBeforeGamePlay()"/>
        /// </summary>       
        public virtual void OnApplicationBeforeGamePlay(){}

        /// <summary>
        /// <see cref="IApplicationInjectable.OnGamePlayUpdate()"/>
        /// </summary>      
        public virtual void OnGamePlayUpdate(){}

        /// <summary>
        /// <see cref="IApplicationInjectable.OnUnPauseGamePlayUpdate()"/>
        /// </summary>
        public virtual void OnUnPauseGamePlayUpdate(){}
    }
}