#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectName.Editor
{
    /// <summary>
    /// PauseFreezer のインスペクタ拡張。
    /// [SerializeReference] の List に対し、IPauseFreezable の具象型を
    /// メニューから選んで追加できるようにする。
    /// </summary>
    [CustomEditor(typeof(ProjectName.Core.Pause.PauseFreezer))]
    public class PauseFreezerEditor : UnityEditor.Editor
    {
        private SerializedProperty freezablesProperty;

        private void OnEnable()
        {
            freezablesProperty = serializedObject.FindProperty("freezables");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Freezables", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (freezablesProperty == null)
            {
                EditorGUILayout.HelpBox("freezables プロパティが見つかりません", MessageType.Error);
                return;
            }

            for (int i = 0; i < freezablesProperty.arraySize; i++)
            {
                var element = freezablesProperty.GetArrayElementAtIndex(i);
                string typeName = element.managedReferenceValue?.GetType().Name ?? "None";

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, typeName, true, EditorStyles.foldoutHeader);

                // 型を選択できるボタン
                if (GUILayout.Button("Change", GUILayout.Width(60)))
                {
                    ShowTypeSelectMenu(i);
                }

                if (GUILayout.Button("－", GUILayout.Width(24)))
                {
                    freezablesProperty.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (element.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    var child = element.Copy();
                    var end = element.GetEndProperty();
                    bool enterChildren = true;
                    while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
                    {
                        EditorGUILayout.PropertyField(child, true);
                        enterChildren = false;
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("＋ Add Freezable"))
            {
                ShowAddMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowAddMenu()
        {
            ShowTypeSelectMenu(-1);
        }

        private void ShowTypeSelectMenu(int elementIndex)
        {
            var pauseFreezableType = System.Type.GetType("ProjectName.Core.Pause.IPauseFreezable, Assembly-CSharp");
            if (pauseFreezableType == null)
            {
                EditorUtility.DisplayDialog("Error", "IPauseFreezable が見つかりません", "OK");
                return;
            }

            var types = TypeCache.GetTypesDerivedFrom(pauseFreezableType)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name)
                .ToList();

            var menu = new GenericMenu();
            foreach (var type in types)
            {
                var capturedType = type;
                var capturedIndex = elementIndex;

                if (elementIndex >= 0)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () => ChangeFreezableType(capturedIndex, capturedType));
                }
                else
                {
                    if (ContainsType(type))
                        menu.AddDisabledItem(new GUIContent(type.Name + " (追加済み)"));
                    else
                        menu.AddItem(new GUIContent(type.Name), false, () => AddFreezable(capturedType));
                }
            }

            menu.ShowAsContext();
        }

        private void ChangeFreezableType(int index, Type type)
        {
            if (index >= 0 && index < freezablesProperty.arraySize)
            {
                var element = freezablesProperty.GetArrayElementAtIndex(index);
                element.managedReferenceValue = Activator.CreateInstance(type);
                element.isExpanded = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private bool ContainsType(Type type)
        {
            for (int i = 0; i < freezablesProperty.arraySize; i++)
            {
                var value = freezablesProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                if (value != null && value.GetType() == type)
                    return true;
            }
            return false;
        }

        private void AddFreezable(Type type)
        {
            serializedObject.Update();
            int index = freezablesProperty.arraySize;
            freezablesProperty.arraySize++;
            var element = freezablesProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);
            element.isExpanded = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
