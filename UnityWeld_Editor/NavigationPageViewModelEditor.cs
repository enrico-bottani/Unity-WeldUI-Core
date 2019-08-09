using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;
using UnityWeld.UI;
using UnityWeld.UI.Messaging.Dispatcher;
using UnityWeld.UI.Paging;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(NavigationPageViewModel))]
    public class NavigationPageViewModelEditor : Editor
    {
        // We want to show all the messages found into the GameObject in a folder
        private static bool _foldOut = true;
        private NavigationPageViewModel targetScript;
        
        private void OnEnable()
        {
            targetScript = (NavigationPageViewModel) target;
            targetScript.AuditAllMessageDispatchers();
        }

        public override void OnInspectorGUI()
        {

            if (targetScript.DefaultBehaviourUnactiveMessages==null){
                targetScript.DefaultBehaviourUnactiveMessages = new DictionaryOfStringBool();
            }
            targetScript.AuditAllMessageDispatchers();
            if (targetScript.DefaultBehaviourUnactiveMessages != null)
            {
                
                if (targetScript.DefaultBehaviourUnactiveMessages.Count == 0)
                {
                    EditorGUILayout.HelpBox("No messages found", MessageType.Info, true);
                    return;
                }
                
                _foldOut = EditorGUILayout.Foldout(_foldOut, "Navigation messages found: "+targetScript.DefaultBehaviourUnactiveMessages.Count);
                if (_foldOut)
                {
                    EditorGUI.indentLevel++;
                    // The two lists of object are being transformed into a non-serializable dictionary
                    foreach (var dictRecord in targetScript.DefaultBehaviourUnactiveMessages.ToList())
                    {
                       
                            Rect drawZone = GUILayoutUtility.GetRect(0f, 16f);
                            var newValue = EditorGUI.Toggle(drawZone, dictRecord.Key,dictRecord.Value);
                            if (newValue != dictRecord.Value){
                                targetScript.DefaultBehaviourUnactiveMessages[dictRecord.Key] = newValue;
                            }

                    }
                    // The list is being transformed into 2 serializable lists. 
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                throw new PropertyNullException(GetType()+": "+nameof(targetScript.DefaultBehaviourUnactiveMessages)+" is null");
            }
        

        }
        

        
    }
}