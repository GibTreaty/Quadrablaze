using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
//using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;
using System.IO;

namespace YounGenTech.Entities {
    public class ScriptableEntityGenerator : EditorWindow {

        public TextAsset ScriptableEntityTemplateWithProperties;

        static Dictionary<Type, string> typeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

        //const string templateWithProperties = "Assets/Entity/Templates/ScriptableEntityTemplateProperties.cs.txt";
        const string entityNamespacesTag = "#NAMESPACES";
        const string entityMenuNameTag = "#MENUNAME";
        const string entityNameTag = "#NAME";
        const string entityClassNameTag = "#CLASSNAME";
        const string entityVariablesTag = "#VARIABLES";
        const string entitySetVariablesTag = "#SETVAR";

        Dictionary<Type, PropertyInfo[]> availableTypes;
        string entityName;
        string menuName;
        string namespaceUsingsText;

        string className;
        string outputClassName;
        bool classDropdown;
        Type selectedType;

        string outputText = "";

        bool initialized;

        [MenuItem("Window/Scriptable Entity Generator")]
        static void Init() {
            var window = GetWindow<ScriptableEntityGenerator>("Scriptable Entity Generator");

            window.Initialize();
        }

        void Initialize() {
            availableTypes = new Dictionary<Type, PropertyInfo[]>();

            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if(!assembly.FullName.Contains("Assembly-CSharp")) continue;

                foreach(var type in assembly.GetTypes()) {
                    if(!type.IsSubclassOf(typeof(Entity))) continue;

                    var properties = new List<PropertyInfo>();

                    foreach(var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        if(property.GetSetMethod() != null)
                            properties.Add(property);

                    availableTypes.Add(type, properties.ToArray());

                    //Debug.Log($"Adding {type.Name} with {availableTypes[type].Length} properties");

                    //foreach(var property in availableTypes[type])
                    //    Debug.Log($"Property: {property.Name}");
                }
            }

            outputText = ScriptableEntityTemplateWithProperties.text;
            initialized = true;
        }

        void OnGUI() {
            if(initialized)
                MainUI();
        }

        void MainUI() {
            if(selectedType != null) {
                EditorGUI.BeginChangeCheck();
                entityName = EditorGUILayout.TextField("Entity Name", entityName);
                menuName = EditorGUILayout.TextField("Menu Name", menuName);
                if(EditorGUI.EndChangeCheck())
                    GenerateOutputText();
            }

            EditorGUILayout.LabelField($"Class: {className} - Output class: {outputClassName}");

            if(selectedType != null) {
                EditorGUILayout.LabelField("Properties");

                EditorGUI.indentLevel++;
                foreach(var property in availableTypes[selectedType]) {
                    var propertyType = GetTypeNameOrAlias(property.PropertyType);

                    if(property.PropertyType.IsGenericType) {
                        propertyType = ToGenericTypeString(property.PropertyType).ToString();
                    }

                    EditorGUILayout.LabelField($"{property.Name}: {propertyType}");
                }
                EditorGUI.indentLevel--;
            }

            if(EditorGUILayout.DropdownButton(new GUIContent("Choose Class"), FocusType.Keyboard))
                classDropdown = !classDropdown;

            if(classDropdown) {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    foreach(var type in availableTypes) {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(32);

                            if(GUILayout.Button(type.Key.Name, GUILayout.ExpandWidth(false))) {
                                selectedType = type.Key;
                                className = type.Key.Name;
                                entityName = type.Key.Name.Replace("Entity", "");
                                menuName = entityName;
                                namespaceUsingsText = GetRequiredNamespaces(availableTypes[selectedType]);
                                outputClassName = $"Scriptable{entityName}Entity";


                                classDropdown = false;

                                GenerateOutputText();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            if(!string.IsNullOrEmpty(outputClassName))
                GUILayout.Label($"Output file: {outputClassName}.cs");

            GUI.enabled = false;
            EditorGUILayout.TextArea(outputText);
            GUI.enabled = true;

            if(selectedType != null && !string.IsNullOrEmpty(entityName))
                if(GUILayout.Button("Finish"))
                    GenerateFile();
        }

        void GenerateOutputText() {
            //return;
            outputText = ScriptableEntityTemplateWithProperties.text
                .Replace(entityNamespacesTag, namespaceUsingsText)
                .Replace(entityMenuNameTag, menuName)
                .Replace(entityNameTag, entityName)
                .Replace(entityClassNameTag, className)
                .Replace(entityVariablesTag, GenerateVariables())
                .Replace(entitySetVariablesTag, GenerateSetVariables());
        }

        string GenerateVariables() {
            string output = "";
            string attribute = "[SerializeField]";
            var properties = availableTypes[selectedType];

            for(int i = 0; i < properties.Length; i++) {
                var property = properties[i];

                var propertyName = property.Name;

                propertyName = propertyName.Remove(0, 1);
                propertyName = property.Name.Substring(0, 1).ToLower() + propertyName;

                output += $"        {attribute}\n" +
                          $"        {ToGenericTypeString(property.PropertyType)} _{propertyName};";

                if(i != properties.Length - 1)
                    output += "\n\n";
            }

            return output;
        }

        string GenerateSetVariables() {
            string output = "";
            var properties = availableTypes[selectedType];

            for(int i = 0; i < properties.Length; i++) {
                var property = properties[i];

                var propertyName = property.Name;

                propertyName = propertyName.Remove(0, 1);
                propertyName = property.Name.Substring(0, 1).ToLower() + propertyName;

                output += $"            entity.{property.Name} = _{propertyName};";

                if(i != properties.Length - 1)
                    output += "\n";
            }

            return output;
        }

        void GenerateFile() {
            var savePath = EditorUtility.SaveFolderPanel("Save To Folder", "Assets", "");

            if(!string.IsNullOrEmpty(savePath) && Directory.Exists(savePath)) {
                savePath = Path.Combine(savePath, $"{outputClassName}.cs");

                if(File.Exists(savePath)) {
                    if(!EditorUtility.DisplayDialog("File exists", $"File {outputClassName}.cs already exists. Overwrite?", "Overwrite", "Cancel")) {
                        Debug.LogError($"ScriptableEntity file exists\n{outputClassName}.cs");
                        return;
                    }
                }

                File.WriteAllText(savePath, outputText);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var file = AssetDatabase.LoadAssetAtPath(savePath, typeof(UnityEngine.Object));
                Selection.activeObject = file;
                EditorGUIUtility.PingObject(file);

                Close();
            }
        }

        static string GetTypeNameOrAlias(Type type) {
            if(typeAlias.TryGetValue(type, out string alias))
                return alias;

            return type.Name;
        }

        static string GetRequiredNamespaces(PropertyInfo[] properties) {
            string output = "";
            var hash = new HashSet<string>() {
                "UnityEngine"
            };

            foreach(var property in properties)
                AddPropertyNamespaces(property.PropertyType, hash);

            int i = 0;
            foreach(var property in hash.OrderBy(s => s)) {
                output += $"using {property};";

                if(i < hash.Count - 1)
                    output += "\n";

                i++;
            }

            return output;
        }
        static void AddPropertyNamespaces(Type type, HashSet<string> hash) {
            hash.Add(type.Namespace);

            if(type.IsGenericType)
                foreach(var argument in type.GetGenericArguments())
                    AddPropertyNamespaces(argument, hash);
        }

        public static string ToGenericTypeString(Type t) {
            if(!t.IsGenericType)
                return GetTypeNameOrAlias(t);

            string genericTypeName = t.GetGenericTypeDefinition().Name;

            genericTypeName = genericTypeName.Substring(0,
                genericTypeName.IndexOf('`'));

            string genericArgs = string.Join(",",
                t.GetGenericArguments()
                    .Select(ta => ToGenericTypeString(ta)).ToArray());

            return genericTypeName + "<" + genericArgs + ">";
        }
    }
}