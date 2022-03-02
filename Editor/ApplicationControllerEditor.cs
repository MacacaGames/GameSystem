

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using MacacaGames.GameSystem;
namespace MacacaGames.GameSystem
{
    [CustomEditor(typeof(ApplicationController))]
    public class ApplicationControllerEditor : Editor
    {
        private ApplicationController applicationController = null;

        void OnEnable()
        {
            applicationController = (ApplicationController)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            using (var disable = new EditorGUI.DisabledGroupScope(true))
            {
                GUILayout.Label($"Current ApplicationState: {applicationController.CurrentApplicationState}");
                GUILayout.Label($"Current GameState: {applicationController.CurrentGameState}");
                GUILayout.Label($"ScriptableObjectLifeCycle Runtime Instances");

                foreach (var item in applicationController.scriptableObjectLifeCycleInstances)
                {
                    EditorGUILayout.ObjectField(item, typeof(ScriptableObjectLifeCycle),false);
                }
            }
        }
    }
}