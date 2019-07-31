using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using UnityWeld.Binding.Internal;
using UnityWeld_Editor;

[CustomEditor(typeof(MessagesDispatcher))]
public class MessagesDispatcherEditor : BaseBindingEditor
{
    private MessagesDispatcher targetScript;
    private void OnEnable()
    {
        targetScript = (MessagesDispatcher)target;
    }

    private bool propertyPrefabModified;

    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
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

        targetScript.SelectedIndex = Array.IndexOf(
            availableMessages,
            targetScript.SelectedMessage
        );

        var defaultLabelStyle = EditorStyles.label.fontStyle;
        EditorStyles.label.fontStyle = propertyPrefabModified
               ? FontStyle.Bold
               : defaultLabelStyle;

        var newSelectedIndex = EditorGUILayout.Popup(
            new GUIContent("Root message", "Type of the view model that this template will be bound to when it is instantiated."),
            targetScript.SelectedIndex,
            availableMessages
                .Select(viewModel => new GUIContent(viewModel))
                .ToArray());

        EditorStyles.label.fontStyle = defaultLabelStyle;
        if (newSelectedIndex >= 0)
        {
            // Debug.Log(newSelectedIndex);
            targetScript.SelectedMessage = ((Type)availableMessagesTypes[newSelectedIndex]).ToString();
        }

        // Don't let the user set anything else until they've chosen a view property.
        var guiPreviouslyEnabled = GUI.enabled;
        if (targetScript.SelectedMessage == null)
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
                new GUIContent(targetScript.ViewModelMethodHandler),
                new GUIContent("View-model method", ""),
                m => m.ViewModelType + "/" + m.MemberName,
                m => true,
                m => m.ToString() == targetScript.ViewModelMethodHandler,
                m => UpdateProperty(
                    updatedValue => targetScript.ViewModelMethodHandler = updatedValue,
                    targetScript.ViewModelMethodHandler,
                    m.ToString(),
                    "Set bound view-model method"
                ),
                bindableMethods
                    .OrderBy(m => m.ViewModelTypeName)
                    .ThenBy(m => m.MemberName)
                    .ToArray()
            );
    }


    private void setValueToTarget(string val)
    {
        targetScript.ViewModelMethodHandler = val;
    }
}
