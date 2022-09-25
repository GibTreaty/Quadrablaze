using UnityEngine;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test Notify.
	/// </summary>
	public class TestNotify : MonoBehaviour
	{
		[SerializeField]
		Notify notifyPrefab;//gameobject in Hierarchy window, parent gameobject should have Layout component (recommended EasyLayout)

		/// <summary>
		/// Show notify.
		/// </summary>
		public void ShowNotify()
		{
			notifyPrefab.Template().Show("Achievement unlocked. Hide after 3 seconds.", customHideDelay: 3f);
		}
	}
}