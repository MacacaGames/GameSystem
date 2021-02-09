using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace MacacaGames.GameSystem
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        /// <summary>
        /// Overrides GUI drawing for the attribute.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var requiredAttribute = this.attribute as RequireInterfaceAttribute;

            GUILayout.Label($"Requier Type: {requiredAttribute.requiredType.ToString()}", EditorStyles.boldLabel);
            // Check if this is reference type property.
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Get attribute parameters.
                // Begin drawing property field.
                EditorGUI.BeginProperty(position, label, property);
                // Draw property field.
                var result = EditorGUILayout.ObjectField(label, property.objectReferenceValue, requiredAttribute.requiredType, true);
                if (result != null && result.GetType().GetInterfaces().Contains(requiredAttribute.requiredType))
                {
                    property.objectReferenceValue = result;
                }
                else if (result != null)
                {
                    Debug.LogError($"Target is not a {requiredAttribute.requiredType.ToString()}");
                    property.objectReferenceValue = null;
                }
                else
                {
                    property.objectReferenceValue = null;
                }
                // Finish drawing property field.
                EditorGUI.EndProperty();
            }
            else
            {
                // If field is not reference, show error message.
                // Save previous color and change GUI to red.
                var previousColor = GUI.color;
                GUI.color = Color.red;
                // Display label with error message.
                EditorGUI.LabelField(position, label, new GUIContent("Property is not a reference type"));
                // Revert color change.
                GUI.color = previousColor;
            }
        }

        // public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        // {
        //     return EditorGUIUtility.singleLineHeight * 2;
        // }

    }
}
