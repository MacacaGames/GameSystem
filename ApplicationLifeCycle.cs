using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ApplicationLifeCycle : MonoBehaviour, IApplicationLifeCycle
{
    public abstract void Init();
    public abstract void OnApplicationBeforeGamePlay();
}

interface IApplicationLifeCycle
{
    void Init();
    /// <summary>
    /// Fire once between GameEnd and next GamePlay, also fire once after init. 
    /// </summary>
    void OnApplicationBeforeGamePlay();
}