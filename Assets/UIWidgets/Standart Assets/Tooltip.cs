using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets
{
	/// <summary>
	/// Tooltip.
	/// http://ilih.ru/images/unity-assets/UIWidgets/Tooltip.png
	/// </summary>
	[AddComponentMenu("UI/UIWidgets/Tooltip")]
	[RequireComponent(typeof(RectTransform))]
	public class Tooltip : MonoBehaviour,
		IPointerEnterHandler,
		IPointerExitHandler,
		ISelectHandler,
		IDeselectHandler
	{
		/// <summary>
		/// Tooltip object.
		/// </summary>
		[SerializeField]
		protected GameObject tooltipObject;

		/// <summary>
		/// Seconds before tooltip shown after pointer enter.
		/// </summary>
		[SerializeField]
		public float ShowDelay = 0.3f;

		/// <summary>
		/// Use unscaled time.
		/// </summary>
		[SerializeField]
		public bool UnscaledTime = true;

		/// <summary>
		/// The tooltip object.
		/// </summary>
		public GameObject TooltipObject {
			get {
				return tooltipObject;
			}
			set {
				tooltipObject = value;
				if (tooltipObject!=null)
				{
					tooltipObjectParent = tooltipObject.transform.parent;
				}
			}
		}

		/// <summary>
		/// Anchored position.
		/// </summary>
		[HideInInspector]
		protected Vector2 anchoredPosition;

		/// <summary>
		/// Canvas transform.
		/// </summary>
		[HideInInspector]
		protected Transform canvasTransform;

		/// <summary>
		/// Tooltip parent object.
		/// </summary>
		[HideInInspector]
		protected Transform tooltipObjectParent;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			TooltipObject = tooltipObject;

			if (TooltipObject!=null)
			{
				canvasTransform = Utilites.FindTopmostCanvas(tooltipObjectParent);
				TooltipObject.SetActive(false);
			}
		}

		/// <summary>
		/// Current coroutine.
		/// </summary>
		protected IEnumerator currentCoroutine;

		/// <summary>
		/// Show coroutine.
		/// </summary>
		/// <returns>Coroutine.</returns>
		protected virtual IEnumerator ShowCoroutine()
		{
			if (UnscaledTime)
			{
				yield return StartCoroutine(Utilites.WaitForSecondsUnscaled(ShowDelay));
			}
			else
			{
				yield return new WaitForSeconds(ShowDelay);
			}

			if (canvasTransform!=null)
			{
				anchoredPosition = (TooltipObject.transform as RectTransform).anchoredPosition;
				tooltipObjectParent = tooltipObject.transform.parent;
				TooltipObject.transform.SetParent(canvasTransform);
			}
			TooltipObject.SetActive(true);
		}

		/// <summary>
		/// Show this tooltip.
		/// </summary>
		public void Show()
		{
			if (TooltipObject==null)
			{
				return ;
			}

			currentCoroutine = ShowCoroutine();
			StartCoroutine(currentCoroutine);
		}

		/// <summary>
		/// Hide coroutine.
		/// </summary>
		/// <returns>Coroutine.</returns>
		protected virtual IEnumerator HideCoroutine()
		{
			if (currentCoroutine!=null)
			{
				StopCoroutine(currentCoroutine);
			}
			if (TooltipObject!=null)
			{
				TooltipObject.SetActive(false);
				yield return null;
				if (canvasTransform!=null)
				{
					TooltipObject.transform.SetParent(tooltipObjectParent);
					(TooltipObject.transform as RectTransform).anchoredPosition = anchoredPosition;
				}
			}
		}

		/// <summary>
		/// Hide this tooltip.
		/// </summary>
		public void Hide()
		{
			StartCoroutine(HideCoroutine());
		}

		/// <summary>
		/// Raises the pointer enter event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnPointerEnter(PointerEventData eventData)
		{
			Show();
		}

		/// <summary>
		/// Raises the pointer exit event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnPointerExit(PointerEventData eventData)
		{
			Hide();
		}

		/// <summary>
		/// Raises the select event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnSelect(BaseEventData eventData)
		{
			Show();
		}

		/// <summary>
		/// Raises the deselect event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnDeselect(BaseEventData eventData)
		{
			Hide();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Create tooltip object.
		/// </summary>
		protected virtual void CreateTooltipObject()
		{
			TooltipObject = Utilites.CreateWidgetFromAsset("Tooltip");
			TooltipObject.transform.SetParent(transform);

			var tooltipRectTransform = TooltipObject.transform as RectTransform;

			tooltipRectTransform.anchorMin = new Vector2(1, 1);
			tooltipRectTransform.anchorMax = new Vector2(1, 1);
			tooltipRectTransform.pivot = new Vector2(1, 0);
			
			tooltipRectTransform.anchoredPosition = new Vector2(0, 0);
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		protected virtual void Reset()
		{
			if (TooltipObject==null)
			{
				CreateTooltipObject();
			}
		}
#endif
	}
}