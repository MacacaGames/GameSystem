

using System.Collections.Generic;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    public static class ApplicationUtils
    {
        public static void GetInjectableMonoBehavioursUnderGameObject(
            GameObject gameObject, List<IApplicationInjectable> injectableComponents)
        {

            GetInjectableMonoBehavioursUnderGameObjectInternal(gameObject, injectableComponents);
        }

        static void GetInjectableMonoBehavioursUnderGameObjectInternal(
            GameObject gameObject, List<IApplicationInjectable> injectableComponents)
        {
            if (gameObject == null)
            {
                return;
            }

            var injectables = gameObject.GetComponents<IApplicationInjectable>();

            // Recurse first so it adds components bottom up though it shouldn't really matter much
            // because it should always inject in the dependency order
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if (child != null)
                {
                    GetInjectableMonoBehavioursUnderGameObjectInternal(child.gameObject, injectableComponents);
                }
            }

            for (int i = 0; i < injectables.Length; i++)
            {
                var injectable = injectables[i];

                // Can be null for broken component references
                if (injectable != null)
                {
                    injectableComponents.Add(injectable);
                }
            }
        }

    }
}