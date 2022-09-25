using UnityEngine;

namespace UIWidgets
{
	/// <summary>
	/// Draggable UI object..
	/// </summary>
	[AddComponentMenu("UI/UIWidgets/Draggable")]
	[RequireComponent(typeof(RectTransform))]
	public class Draggable : MonoBehaviour
	{
		/// <summary>
		/// The handle.
		/// </summary>
		[SerializeField]
		GameObject handle;

		DraggableHandle handleScript;

		/// <summary>
		/// If specified, restricts dragging from starting unless the pointerdown occurs on the specified element.
		/// </summary>
		/// <value>The handler.</value>
		public GameObject Handle {
			get {
				return handle;
			}
			set {
				SetHandle(value);
			}
		}

		/// <summary>
		/// Init handle.
		/// </summary>
		protected virtual void Start()
		{
			SetHandle(handle!=null ? handle : gameObject);
		}

		/// <summary>
		/// Sets the handle.
		/// </summary>
		/// <param name="value">Value.</param>
		protected virtual void SetHandle(GameObject value)
		{
			if (handle!=null)
			{
				Destroy(handleScript);
			}
			handle = value;
			handleScript = handle.GetComponent<DraggableHandle>();
			if (handleScript==null)
			{
				handleScript = handle.AddComponent<DraggableHandle>();
			}
			handleScript.Drag(gameObject.transform as RectTransform);
		}
	}
}