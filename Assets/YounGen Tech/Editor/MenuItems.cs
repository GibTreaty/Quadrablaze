using UnityEngine;
using UnityEditor;
using System.Collections;

namespace YounGenTech {
	public static partial class MenuItems {
		static void FinalizeObject (GameObject go, Transform parentTo) {
			if(parentTo) {
				go.transform.parent = parentTo;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
			}

			Selection.activeGameObject = go;
			EditorGUIUtility.PingObject(go);
		}
	}
}