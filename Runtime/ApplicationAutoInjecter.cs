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

            Component[] injectables = GetComponentsInChildren<Component>();
            if (injectables.Length == 0)
            {
                Debug.LogError("No IApplicationInjectable found on this GameObject or its children, remember add IApplicationInjectable on the class which you wish to Inject", gameObject);
            }
            foreach (var item in injectables)
            {
                ApplicationController.Instance.ResolveInjection(item);
            }
            injectFinish = true;
        }
    }
}
