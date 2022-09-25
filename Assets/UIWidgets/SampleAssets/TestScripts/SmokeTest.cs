using UnityEngine;
using System.Collections;
using System.Linq;
using UIWidgets;

namespace UIWidgets.Tests
{
	/// <summary>
	/// Smoke test.
	/// </summary>
	public class SmokeTest : MonoBehaviour
	{
		[SerializeField]
		Accordion accordion;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			#if UNITY_STANDALONE
			if (System.Environment.GetCommandLineArgs().Contains("-smoke-test"))
			{
				StartCoroutine(SimpleTest());
			}
			#endif
		}

		/// <summary>
		/// Simple test.
		/// </summary>
		/// <returns>Coroutine.</returns>
		protected virtual IEnumerator SimpleTest()
		{
			yield return new WaitForSeconds(5f);
			
			var items = accordion.DataSource;
			if (!accordion.DataSource[0].Open || !accordion.DataSource[0].ContentObject.activeSelf)
			{
				throw new UnityException("Overview is not active!");
			}

			foreach (var item in items)
			{
				if (item.ToggleObject.name=="Exit")
				{
					continue ;
				}
				accordion.ToggleItem(item);
				yield return new WaitForSeconds(5f);
			}

			Application.Quit();
		}
	}
}