using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    public class ApplicationAutoInjecter : MonoBehaviour
    {
        // Start is called before the first frame update
        bool injectFinish = false;
        void Awake()
        {
            if (injectFinish)
                return;

            List<IApplicationInjectable> injectables = new List<IApplicationInjectable>();
            ApplicationUtils.GetInjectableMonoBehavioursUnderGameObject(gameObject, injectables);
            foreach (var item in injectables)
            {
                ApplicationController.Instance.ResolveInjection(item);
            }
            injectFinish = true;
        }
    }
}
