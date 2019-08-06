using UnityEditor;
using UnityEngine;
using UnityWeld.UI.Paging;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(NavigationPageViewModel))]
    public class NavigationPageViewModelEditor:Editor
    {
        private static bool foldOut = true;
        SerializedProperty listProperty;
        public override void OnInspectorGUI()
        {
            SerializedProperty list = serializedObject.FindProperty("messagesUnderWatch");
            
            listProperty = list;
            serializedObject.Update();
            if (listProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No messages found", MessageType.Info, true);
            }
            else
            {
                ListIterator(listProperty);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void ListIterator(SerializedProperty listProperty)
        {

            foldOut = EditorGUILayout.Foldout(foldOut, "Navigation messages found:");
            if (foldOut)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                    Rect drawZone = GUILayoutUtility.GetRect(0f, 16f);

                    EditorGUI.LabelField(drawZone, elementProperty.stringValue);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}