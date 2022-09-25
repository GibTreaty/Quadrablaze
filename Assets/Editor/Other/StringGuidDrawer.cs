using UnityEngine;
 using UnityEditor;

 [CustomPropertyDrawer(typeof(StringGuid))]
 public class StringGuidDrawer : PropertyDrawer {
   bool DO_VALIDATION = true;
   public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
     EditorGUI.BeginProperty(position, label, property);
     position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
     var indent = EditorGUI.indentLevel;
     EditorGUI.indentLevel = 0;
 
     var storageProp = property.FindPropertyRelative("m_storage");
     if (DO_VALIDATION) {
       var oldval = storageProp.stringValue;
       var newval = EditorGUI.DelayedTextField(position, oldval);
       if (oldval != newval) {
         try {
           storageProp.stringValue = new System.Guid(newval).ToString("D");
         } catch (System.FormatException) {}
       }
     } else {
       EditorGUI.PropertyField(position, storageProp, GUIContent.none);
     }
     EditorGUI.indentLevel = indent;
     EditorGUI.EndProperty();
   }
 }