#if UNITY_EDITOR
using System;
using System.Linq;
using TechC.UI.Effects;
using UnityEditor;
using UnityEngine;

namespace TechC.Editor
{
    /// <summary>
    /// UIEffectController のインスペクタ拡張。
    /// [SerializeReference] の List に対し、UIEffectBase の具象型を
    /// メニューから選んで追加できるようにする。
    /// PauseFreezerEditor と違い、同じエフェクトの複数追加を許可する（周期違いのパルス等）
    /// </summary>
    [CustomEditor(typeof(UIEffectController))]
    public class UIEffectControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty effectsProperty;

        private void OnEnable()
        {
            effectsProperty = serializedObject.FindProperty("_effects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (effectsProperty == null)
            {
                EditorGUILayout.HelpBox("_effects プロパティが見つかりません", MessageType.Error);
                return;
            }

            for (int i = 0; i < effectsProperty.arraySize; i++)
            {
                var element = effectsProperty.GetArrayElementAtIndex(i);
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
                    effectsProperty.DeleteArrayElementAtIndex(i);
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
            if (GUILayout.Button("＋ Add Effect"))
            {
                ShowTypeSelectMenu(-1);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowTypeSelectMenu(int elementIndex)
        {
            var types = TypeCache.GetTypesDerivedFrom<UIEffectBase>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToList();

            var menu = new GenericMenu();
            foreach (var type in types)
            {
                var capturedType = type;
                var capturedIndex = elementIndex;

                if (elementIndex >= 0)
                    menu.AddItem(new GUIContent(type.Name), false, () => ChangeEffectType(capturedIndex, capturedType));
                else
                    menu.AddItem(new GUIContent(type.Name), false, () => AddEffect(capturedType));
            }

            menu.ShowAsContext();
        }

        private void ChangeEffectType(int index, Type type)
        {
            if (index >= 0 && index < effectsProperty.arraySize)
            {
                var element = effectsProperty.GetArrayElementAtIndex(index);
                element.managedReferenceValue = Activator.CreateInstance(type);
                element.isExpanded = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void AddEffect(Type type)
        {
            serializedObject.Update();
            int index = effectsProperty.arraySize;
            effectsProperty.arraySize++;
            var element = effectsProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);
            element.isExpanded = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
