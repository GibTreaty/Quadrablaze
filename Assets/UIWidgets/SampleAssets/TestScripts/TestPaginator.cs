using UnityEngine;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test Paginator.
	/// </summary>
	public class TestPaginator : MonoBehaviour
	{
		[SerializeField]
		ScrollRectPaginator paginator;

		/// <summary>
		/// Test.
		/// </summary>
		public void Test()
		{
			// pages count
			Debug.Log(paginator.Pages);

			// navigate to page
			paginator.CurrentPage = 2;
		}
	}
}