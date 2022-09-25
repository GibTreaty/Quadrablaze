using UnityEngine;
using UnityEditor;
using System.Collections;
using YounGenTech.BulletPhysics;

namespace YounGenTech {
	public static partial class MenuItems {
		#region GameObject Menu
		[MenuItem("GameObject/Create YounGen Tech/Bullet Physics/Bullet", false, 0)]
		static GameObject CreateBullet() {
			GameObject go = new GameObject("Bullet", typeof(Bullet), typeof(BulletAudio));
			Undo.RegisterCreatedObjectUndo(go, "Bullet");

			FinalizeObject(go, Selection.activeTransform);

			return go;
		}
		#endregion
	}
}