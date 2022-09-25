using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization;

public class ComponentToXML : EditorWindow {

    public Component selectedComponent;

    public string displayString = "";

    int selectedIndex = -1;
    string selectedType = "";
    string[] typesSelection = new string[0];
    Type serializeType = null;

    [MenuItem("Window/ComponentToXML")]
    static void Init() {
        GetWindow<ComponentToXML>("Component To XML");
    }

    void OnGUI() {
        EditorGUILayout.BeginVertical();
        {
            EditorGUI.BeginChangeCheck();
            {
                selectedComponent = EditorGUILayout.ObjectField("Serialize Component", selectedComponent, typeof(Component), true) as Component;
            }
            if(EditorGUI.EndChangeCheck()) {
                var baseType = selectedComponent.GetType();
                var types = Assembly.GetAssembly(baseType).GetTypes().Where(s => baseType.IsSubclassOf(s) || s == baseType).ToArray();
                //var types = Assembly.GetAssembly(selectedComponent.GetType()).GetTypes().Where(s => s.IsSubclassOf(typeof(Component))).ToArray();
                //var types = selectedComponent.GetType();

                typesSelection = new string[types.Length];

                if(typesSelection.Length > 0) {
                    selectedType = types.Length == 0 ? "Component" : types[0].Name;
                    selectedIndex = types.Length == 0 ? -1 : 0;
                    serializeType = types[0];

                    for(int i = 0; i < typesSelection.Length; i++)
                        typesSelection[i] = types[i].Name;
                }

                Debug.Log("types:" + types.Length);
            }

            if(typesSelection.Length > 0) {
                GUILayout.Label("Type: " + selectedType);
                selectedIndex = EditorGUILayout.Popup(selectedIndex, typesSelection);

                EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
                {
                    if(GUILayout.Button("Serialize"))
                        Serialize();
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.TextArea(displayString, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        EditorGUILayout.EndVertical();
    }

    //void Deserialize() {

    //}

    void Serialize() {
        if(!selectedComponent) return;

        //NetDataContractSerializer serializer = new NetDataContractSerializer();
        XmlSerializer serializer = GetSerializer(serializeType);
        StringBuilder stringBuilder = new StringBuilder();
        XmlWriter writer = XmlWriter.Create(stringBuilder);
        //var writer = new StreamWriter(
        try {
            serializer.Serialize(writer, selectedComponent);
        }
        finally {
            writer.Close();
        }

        displayString = stringBuilder.ToString();
    }

    static XmlSerializer GetSerializer<T>() {
        return new XmlSerializer(typeof(T));
    }
    static XmlSerializer GetSerializer(Type type) {
        return new XmlSerializer(type);
    }
}