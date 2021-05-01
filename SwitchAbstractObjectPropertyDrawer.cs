using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SwitchAbstractObjectAttribute))]
public class SwitchAbstractObjectPropertyDrawer : PropertyDrawer
{

    int _spaceHeight = 18;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        string[] assemblySearch = property.managedReferenceFieldTypename.Split(new char[] { ' ' });
        Type managedType = Assembly.Load(assemblySearch[assemblySearch.Length-2]).GetType(assemblySearch[assemblySearch.Length - 1]);

        // Label
        Rect labelRect = new Rect(position.x, position.y, position.width * 0.5f, _spaceHeight);
        EditorGUI.LabelField(labelRect, property.displayName);

        //Type[] types = Array.FindAll(Assembly.GetAssembly(managedType).GetTypes(), x => x.BaseType == managedType);
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => managedType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();
        Type currentType = Array.Find(types, x => property.type.Contains(x.ToString()));

        Rect dropdownRect = new Rect(position.x + labelRect.width, position.y, position.width - labelRect.width, _spaceHeight);
        if (EditorGUI.DropdownButton(
            dropdownRect,
            new GUIContent(currentType != null ? currentType.ToString() : "null"),
            FocusType.Keyboard))
        {
            // Build drop down menu
            GenericMenu menu = new GenericMenu();
            foreach (Type type in types)
            {
                menu.AddItem(
                    new GUIContent(type.Name),
                    type == currentType,
                    () => {
                        SetType(property, type);
                        });
            }
            menu.ShowAsContext();
            return;
        }

        // Display property
        EditorGUI.PropertyField(position, property, true);

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

    private void SetType(SerializedProperty property, Type type)
    {
        property.managedReferenceValue = Activator.CreateInstance(type);
        property.serializedObject.ApplyModifiedProperties();
    }

}