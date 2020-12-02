using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rayark.Mast;
using Coroutine = Rayark.Mast.Coroutine;

public abstract class GameSystemBase : ScriptableObject, IApplicationInjectable
{
    public abstract void Init();
    Coroutine logic;
    public Coroutine OnUpdate()
    {
        if (logic == null)
        {
            logic = new Coroutine(OnUpdateImpl());
        }
        return logic;
    }

    /// <summary>
    /// Implement Update logic here
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator OnUpdateImpl();

    /// <summary>
    /// Fire once between GameEnd and next GamePlay, also fire once after init. 
    /// </summary>
    public abstract void OnApplicationBeforeGamePlay();



}
