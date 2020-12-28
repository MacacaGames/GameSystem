

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.AnimatedValues;
// using System.Linq;
// using MacacaGames.GameSystem;
// namespace MacacaGames.GameSystem
// {
//     [CustomEditor(typeof(ApplicationController))]
//     public class ApplicationControllerEditor : Editor
//     {
//         private ApplicationController applicationController = null;

//         void OnEnable()
//         {
//             applicationController = (ApplicationController)target;

//         }

//         public override void OnInspectorGUI()
//         {
//             base.OnInspectorGUI();
//             using (var disable = new EditorGUI.DisabledGroupScope(true))
//             {
//                 foreach (var item in applicationController.scriptableObjectLifeCycleInstances)
//                 {

//                 }
//             }
//             // GUILayout.Label($"Pool Status");

//             // foreach (var item in runtimePool.GetDicts())
//             // {
//             //     var queue = item.Value;
//             //     if (queue == null)
//             //     {
//             //         continue;
//             //     }
//             //     GUILayout.Label($"{TryGetPoolNameByInstanceId(item.Key)} : {queue.Count}");
//             // }
//             // GUILayout.Label($"Recovery Queue Status");

//             // foreach (var item in runtimePool.GetRecycleQueue())
//             // {
//             //     var queue = item;
//             //     if (queue == null)
//             //     {
//             //         continue;
//             //     }
//             //     if (GUILayout.Button($"{queue.name}"))
//             //     {
//             //         EditorGUIUtility.PingObject(queue.gameObject);
//             //     }
//             // }

//         }


//     }
// }