using UnityEngine;
using UnityEditor;
using System.Collections;
using YounGenTech.EnergyBasedObjects;

namespace YounGenTech {
	public static partial class MenuItems {
		#region GameObject Menu
		[MenuItem("GameObject/Create YounGen Tech/Energy Based Objects/Shooting Point", false, 0)]
		static GameObject CreateObjectShootingPoint() {
			GameObject go = new GameObject("Shooting Point", typeof(ObjectShootingPoint));
			Undo.RegisterCreatedObjectUndo(go, "Shooting Point");

			FinalizeObject(go, Selection.activeTransform);
			
			return go;
		}

		[MenuItem("GameObject/Create YounGen Tech/Energy Based Objects/Raycast Shooting Point", false, 1)]
		static void CreateRayacastTurret() {
			GameObject go = new GameObject("Raycast Shooting Point", typeof(ObjectRaycastTurret), typeof(OnWeaponRaycastSpawn));
			Undo.RegisterCreatedObjectUndo(go, "Raycast Shooting Point");

			FinalizeObject(go, Selection.activeTransform);
		}

		[MenuItem("GameObject/Create YounGen Tech/Energy Based Objects/Pusher Shooting Point", false, 2)]
		static void CreatePusher() {
			GameObject go = new GameObject("Pusher Shooting Point", typeof(ObjectPusher));
			Undo.RegisterCreatedObjectUndo(go, "Pusher Shooting Point");

			FinalizeObject(go, Selection.activeTransform);
		}

		[MenuItem("GameObject/Create YounGen Tech/Energy Based Objects/Spawner Pusher Shooting Point", false, 3)]
		static void CreateSpawnerPusher() {
			GameObject go = new GameObject("Spawner Pusher Shooting Point", typeof(ObjectSpawnerPusher));
			Undo.RegisterCreatedObjectUndo(go, "Spawner Pusher Shooting Point");

			FinalizeObject(go, Selection.activeTransform);
		}

		[MenuItem("GameObject/Create YounGen Tech/Energy Based Objects/Energy Container", false, 14)]
		static void CreateEnergyContainer() {
			GameObject go = new GameObject("Energy Container", typeof(Energy));
			Undo.RegisterCreatedObjectUndo(go, "Energy Container");

			FinalizeObject(go, Selection.activeTransform);
		}
		#endregion

		#region Context Menu
		[MenuItem("CONTEXT/EnergyUser/Add Energy")]
		static void AddEnergyContainer() {
			Energy energy = Selection.activeGameObject.AddComponent<Energy>();
			Undo.RegisterCreatedObjectUndo(energy, "Add Energy");
		}

		[MenuItem("GameObject/Create YounGen Tech/Energy Based Objects/Make Into Energy User", false, 14),
		MenuItem("CONTEXT/ObjectShootingPoint/Make Into Energy User")]
		static void MakeIntoEnergyUser() {
			GameObject selected = Selection.activeGameObject;

			if(!selected) selected = CreateObjectShootingPoint();

			EnergyUser user = selected.GetComponent<EnergyUser>();

			if(!user) {
				user = selected.AddComponent<EnergyUser>();
				Undo.RegisterCreatedObjectUndo(user, "Make Into Energy User");
			}

			if(Selection.gameObjects.Length > 0)
				foreach(GameObject a in Selection.gameObjects)
					if(a.GetComponent<Energy>()) {
						user.energyContainer = a;
						break;
					}

		}

		[MenuItem("CONTEXT/ObjectShootingPoint/Add Audio")]
		static void AddObjectShootAudio() {
			Object obj = Selection.activeGameObject.AddComponent<ObjectShootAudio>();
			Undo.RegisterCreatedObjectUndo(obj, "Add ObjectShootAudio");
		}

		[MenuItem("CONTEXT/ObjectRaycastTurret/Add OnWeaponRaycastSpawn")]
		static void AddOnWeaponRaycastSpawn() {
			Object obj = Selection.activeGameObject.AddComponent<OnWeaponRaycastSpawn>();
			Undo.RegisterCreatedObjectUndo(obj, "Add OnWeaponRaycastSpawn");
		}
		#endregion
	}
}