using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using UnityWeld.Binding.Internal;
using UnityWeld.Messaging.Dispatcher;
using UnityWeld_Editor;

[CustomEditor(typeof(MessagesDispatcher))]
public class MessagesDispatcherEditor : BaseBindingEditor
{
    private MessagesDispatcher targetScript;
    private void OnEnable()
    {
        targetScript = (MessagesDispatcher)target;
    }

    // Whether or not the values on our target match its prefab.
    private bool viewMessagePrefabModified;
    private bool viewModelMethodPrefabModified;

    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        
        if (CannotModifyInPlayMode())
        {
            GUI.enabled = false;
        }
        
        UpdatePrefabModifiedProperties();        
        
        var availableMessagesTypes = TypeResolver.TypesWithMessageAttribute.OrderBy(message => message.ToString()).ToArray();
        var availableMessages = availableMessagesTypes.Select(type => type.ToString())
           .ToArray();

        foreach (Type t in availableMessagesTypes)
        {
            if (!t.IsSubclassOf(typeof(BaseMessage)))
            {
                throw new InvalidCastException("Message " + t.ToString() + " must be subclass of " + nameof(BaseMessage));
            }
        }

        targetScript.UnityEditorSelectedMessageTypeIndex = Array.IndexOf(
            availableMessages,
            targetScript.UnityEditorSelectedMessageName
        );

        var defaultLabelStyle = EditorStyles.label.fontStyle;
        EditorStyles.label.fontStyle = viewModelMethodPrefabModified
               ? FontStyle.Bold
               : defaultLabelStyle;

        var newSelectedIndex = EditorGUILayout.Popup(
            new GUIContent("MessageType", "Type of the view model that this template will be bound to when it is instantiated."),
            targetScript.UnityEditorSelectedMessageTypeIndex,
            availableMessages
                .Select(viewModel => new GUIContent(viewModel))
                .ToArray());

        EditorStyles.label.fontStyle = defaultLabelStyle;
        if (newSelectedIndex >= 0)
        {
            // Debug.Log(newSelectedIndex);
            targetScript.UnityEditorSelectedMessageName = ((Type)availableMessagesTypes[newSelectedIndex]).ToString();
        }

        // Don't let the user set anything else until they've chosen a view property.
        var guiPreviouslyEnabled = GUI.enabled;
        if (targetScript.UnityEditorSelectedMessageName == null)
        {
            GUI.enabled = false;
        }

        //Debug.Log("TypeResolver:" + TypeResolver.FindBindableProperties(targetScript).Length);
        /* ShowViewModelPropertyMenu(
             new GUIContent(
                 "View-model property",
                 "Property on the view-model to bind to."
             ),
             TypeResolver.FindBindableProperties(targetScript),
             value => { setValueToTarget(value); },
            targetScript.ViewModelPropertyName,
             property => property.PropertyType == (Type)availableMessagesTypes[selectedIndex]
         );
         */
        //Debug.Log( "Found "+TypeResolver.FindBindableMethods(targetScript,(Type)availableMessagesTypes[newSelectedIndex]).Length+" methods available");

        var bindableMethods = TypeResolver.FindBindableMethods(targetScript, (Type)availableMessagesTypes[newSelectedIndex]);
        InspectorUtils.DoPopup(
                new GUIContent(targetScript.UnityEditorViewModelMethodHandler),
                new GUIContent("View-model method", ""),
                m => m.ViewModelType + "/" + m.MemberName,
                m => true,
                m => m.ToString() == targetScript.UnityEditorViewModelMethodHandler,
                m => UpdateProperty(
                    updatedValue => targetScript.UnityEditorViewModelMethodHandler = updatedValue,
                    targetScript.UnityEditorViewModelMethodHandler,
                    m.ToString(),
                    "Set bound view-model method"
                ),
                bindableMethods
                    .OrderBy(m => m.ViewModelTypeName)
                    .ThenBy(m => m.MemberName)
                    .ToArray()
            );
    }

    /// <summary>
    /// Check whether each of the properties on the object have been changed 
    /// from the value in the prefab.
    /// </summary>
    private void UpdatePrefabModifiedProperties()
    {
        var property = serializedObject.GetIterator();
        // Need to call Next(true) to get the first child. Once we have it, 
        // Next(false) will iterate through the properties.
        property.Next(true);
        do
        {
            switch (property.name)
            {
                case "unityEditorSelectedMessageName":
                    viewMessagePrefabModified = property.prefabOverride;
                    break;

                case "unityEditorViewModelMethodHandler":
                    viewModelMethodPrefabModified = property.prefabOverride;
                    break;
            }
        }
        while (property.Next(false));
    }

}
