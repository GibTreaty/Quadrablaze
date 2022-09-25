using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

using Steamworks;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// This class manages the uGUI of the Steam Workshop upload item menu.
	/// It registers to events of SteamWorkshopMain class, e.g. SteamWorkshopMain.OnUploaded.
	/// SteamWorkshopUIUpload also receives UI callbacks from uGUI buttons and other elements.
	/// You can replace this class with your own UI e.g. NGUI.
	/// In this case you need to take care of registering to events and calling methods of the SteamWorkshopMain class from your new implementation.
	/// </summary>
	public class SteamWorkshopUIUpload : MonoBehaviour
	{
		protected static SteamWorkshopUIUpload s_instance;
		/// <summary>
		/// You can use the static Instance property to access the SteamWorkshopUIUpload class from wherever you need it in your code.
		/// Use this property only if you know that you have a static SteamWorkshopUIUpload uGUI in your scene.
		/// If you use the SteamWorkshopPopupUpload, then there is no guarantee that the SteamWorkshopUIUpload was already created.
		/// </summary>
		public static SteamWorkshopUIUpload Instance
		{
			get
			{
				// try to find an existing instance
				if (s_instance == null)
				{
					s_instance = FindObjectOfType<SteamWorkshopUIUpload>();
				}
				return s_instance;
			}
		}

		[SerializeField]
		protected int ICON_WIDTH = 512;
		[SerializeField]
		protected int ICON_HEIGHT = 512;

		[SerializeField]
		protected InputField NAME_INPUT = null;
		[SerializeField]
		protected InputField DESCRIPTION_INPUT = null;
		[SerializeField]
		protected RawImage ICON = null;
		[SerializeField]
		protected Button SCREENSHOT_BUTTON = null;
		[SerializeField]
		protected Button UPLOAD_BUTTON = null;
		[SerializeField]
		protected bool m_improveNavigationFocus = true;

		protected bool m_isUploading = false;
		protected WWW m_pendingImageDownload = null;
		protected WorkshopItemUpdate m_itemData = new WorkshopItemUpdate();

		/// <summary>
		/// Invoked when the name InputField (NAME_INPUT) is committed.
		/// </summary>
		public event System.Action<string> OnNameSet;
		/// <summary>
		/// Invoked when the description InputField (DESCRIPTION_INPUT) is committed.
		/// </summary>
		public event System.Action<string> OnDescriptionSet;
		/// <summary>
		/// Invoked when the item icon was rendered and is available on disk.
		/// </summary>
		public event System.Action<string> OnIconFilePathSet;
		/// <summary>
		/// Invoked when the item icon was rendered and the Textured2D instance is ready to be used.
		/// </summary>
		public event System.Action<Texture2D> OnIconTextureSet;
		/// <summary>
		/// Invoked when the upload process is started.
		/// </summary>
		public event System.Action<WorkshopItemUpdateEventArgs> OnStartedUpload;
		/// <summary>
		/// Invoked when the upload process is finished successfully.
		/// </summary>
		public event System.Action<WorkshopItemUpdateEventArgs> OnFinishedUpload;

		/// <summary>
		/// Call SetItemData to refresh the item UI.
		/// </summary>
		/// <param name="p_itemData">item update data to be visualized.</param>
		public virtual void SetItemData(WorkshopItemUpdate p_itemData)
		{
			// make sure that m_itemData is not null (if p_itemData is null, then create an empty data object)
			m_itemData = p_itemData != null ? p_itemData : new WorkshopItemUpdate();

			// make sure that name and description are not null
			if (m_itemData.Name == null) { m_itemData.Name = ""; }
			if (m_itemData.Description == null) { m_itemData.Description = ""; }

			// update name and description in UI
			if (NAME_INPUT != null)
			{
				NAME_INPUT.text = m_itemData.Name;
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: SetItemData: NAME_INPUT is not set in inspector!");
			}
			if (DESCRIPTION_INPUT != null)
			{
				if (!string.IsNullOrEmpty(m_itemData.Description))
				{
					// A Unity bug going through various Unity versions does not allow to set the text of a multiline InputField directly after creation -> Coroutine
					StartCoroutine(SetDescriptionSafe(m_itemData.Description));
				}
				else
				{
					DESCRIPTION_INPUT.text = "";
				}
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: SetItemData: DESCRIPTION_INPUT is not set in inspector!");
			}
			if (ICON != null)
			{
				if (!string.IsNullOrEmpty(m_itemData.IconPath))
				{
					StartCoroutine(LoadIcon(m_itemData.IconPath));
				}
				else
				{
					ICON.texture = null;
				}
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: SetItemData: ICON is not set in inspector!");
			}
		}

		protected virtual void Start()
		{
			// show success popup when upload is done
			SteamWorkshopMain.Instance.OnUploaded += ShowSuccessMessage;
			// show error popups when something goes wrong
			SteamWorkshopMain.Instance.OnError += ShowErrorMessage;

			// register uGUI callbacks
			if (NAME_INPUT != null)
			{
				NAME_INPUT.onEndEdit.AddListener(OnEditName);	
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: NAME_INPUT is not set in inspector!");
			}
			if (DESCRIPTION_INPUT != null)
			{
				DESCRIPTION_INPUT.onEndEdit.AddListener(OnEditDescription);	
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: DESCRIPTION_INPUT is not set in inspector!");
			}
			if (SCREENSHOT_BUTTON != null)
			{
				SCREENSHOT_BUTTON.onClick.AddListener(OnScreenshotButtonClick);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: SCREENSHOT_BUTTON is not set in inspector!");
			}
			if (UPLOAD_BUTTON != null)
			{
				UPLOAD_BUTTON.onClick.AddListener(OnUploadButtonClick);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: UPLOAD_BUTTON is not set in inspector!");
			}
		}

		protected virtual void LateUpdate()
		{
			if (m_improveNavigationFocus)
			{
				EventSystem eventSys = EventSystem.current;
				if (eventSys != null)
				{
					if (eventSys.currentSelectedGameObject == null || !eventSys.currentSelectedGameObject.activeInHierarchy)
					{
						// if selection is lost, then ...
						if (eventSys.lastSelectedGameObject != null && eventSys.lastSelectedGameObject.activeInHierarchy)
						{
							// select last selected if it is still active
							eventSys.SetSelectedGameObject(eventSys.lastSelectedGameObject);
						}
						else if (NAME_INPUT != null)
						{
							// select the name input
							NAME_INPUT.Select();
						}
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (ICON != null)
			{
				Destroy(ICON.texture); // release image space
			}
			if (m_pendingImageDownload != null)
			{
				m_pendingImageDownload.Dispose(); // cancel download and release memory
				m_pendingImageDownload = null;
			}
			if (SteamWorkshopMain.IsInstanceSet)
			{
				// unregister events
				SteamWorkshopMain.Instance.OnUploaded -= ShowSuccessMessage;
				SteamWorkshopMain.Instance.OnError -= ShowErrorMessage;
			}
		}

		protected virtual void OnEditName(string p_name)
		{
			m_itemData.Name = p_name;
			InvokeEventHandlerSafely(OnNameSet, p_name);
		}

		protected virtual void OnEditDescription(string p_description)
		{
			m_itemData.Description = p_description;
			InvokeEventHandlerSafely(OnDescriptionSet, p_description);
		}

		protected virtual void OnScreenshotButtonClick()
		{
			if (string.IsNullOrEmpty(m_itemData.ContentPath))
			{
				m_itemData.ContentPath = System.IO.Path.Combine(Application.persistentDataPath, m_itemData.Name);
			}
			string iconFilePath = System.IO.Path.Combine(m_itemData.ContentPath, m_itemData.Name + ".png");
			SteamWorkshopMain.Instance.RenderIcon(Camera.main, ICON_WIDTH, ICON_HEIGHT, iconFilePath, (Texture2D p_renderedIcon) => 
			{
				// store the icon file path in the WorkshopItemUpdate.IconPath property
				m_itemData.IconPath = iconFilePath;
				InvokeEventHandlerSafely(OnIconFilePathSet, iconFilePath);
				// wait until the icon is rendered, then check again that the UI is still valid and assign it
				if (ICON != null)
				{
					ICON.texture = p_renderedIcon;
					InvokeEventHandlerSafely(OnIconTextureSet, p_renderedIcon);
				}
				else
				{
					Debug.LogError("SteamWorkshopUIUpload: OnScreenshotButtonClick: ICON is not set in inspector!");
				}
			});
		}

		protected virtual void OnUploadButtonClick()
		{
			// don't allow to upload items without a name
			if (string.IsNullOrEmpty(m_itemData.Name))
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Invalid Item Name", "Please give your item a non-empty name!")
					.ShowButton(uMyGUI_PopupManager.BTN_OK);
			}
			// don't allow to upload items without a description
			else if (string.IsNullOrEmpty(m_itemData.Description))
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Invalid Item Name", "Please give your item a non-empty description!")
					.ShowButton(uMyGUI_PopupManager.BTN_OK);
			}
			// don't allow to upload items without an icon
			else if (string.IsNullOrEmpty(m_itemData.IconPath))
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Invalid Item Icon", "Please provide an icon image for your item!")
					.ShowButton(uMyGUI_PopupManager.BTN_OK);
			}
			else
			{
				m_isUploading = true;
				// show progress popup
				StartCoroutine(ShowUploadProgress());
				// start upload to Steam Workshop
				SteamWorkshopMain.Instance.Upload(m_itemData, null);
				// notify listeners
				if (OnStartedUpload != null) { OnStartedUpload(new WorkshopItemUpdateEventArgs() { Item = m_itemData }); }
			}
		}

		protected virtual void ShowSuccessMessage(WorkshopItemUpdateEventArgs p_successArgs)
		{
			m_isUploading = false;
			// show the success message in a new popup
			if (!p_successArgs.IsError && p_successArgs.Item != null)
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Item Uploaded", "Item '" + p_successArgs.Item.Name +"' was successfully uploaded!")
					.ShowButton(uMyGUI_PopupManager.BTN_OK);
			}
			// notify listeners
			if (OnFinishedUpload != null) { OnFinishedUpload(p_successArgs); }
		}

		protected virtual void ShowErrorMessage(ErrorEventArgs p_errorArgs)
		{
			m_isUploading = false;
			// show the error message in a new popup
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Steam Error", p_errorArgs.ErrorMessage)
				.ShowButton(uMyGUI_PopupManager.BTN_OK);
		}

		protected virtual void InvokeEventHandlerSafely<T>(System.Action<T> p_handler, T p_data)
		{
			try
			{
				if (p_handler != null) { p_handler(p_data); }
			}
			catch (System.Exception ex)
			{
				Debug.LogError("SteamWorkshopUIUpload: your event handler ("+p_handler.Target+" - System.Action<"+typeof(T)+">) has thrown an excepotion!\n" + ex);
			}
		}

		protected virtual IEnumerator ShowUploadProgress()
		{
			while (m_itemData != null && m_isUploading)
			{
				float progress = SteamWorkshopMain.Instance.GetUploadProgress(m_itemData);
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Uploading Item", "<size=32>" + (int)(progress * 100f) + "%</size>");
				yield return new WaitForSeconds(0.4f);
			}
		}

		/// <summary>
		/// A Unity bug going through various Unity versions does not allow to set the text of a multiline InputField directly after creation...
		/// See: https://forum.unity3d.com/threads/inputfield-argumentoutofrangeexception.295840/
		/// </summary>
		protected virtual IEnumerator SetDescriptionSafe(string p_description)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			if (DESCRIPTION_INPUT != null)
			{
				DESCRIPTION_INPUT.text = p_description;
			}
		}

		protected virtual IEnumerator LoadIcon(string p_filePath)
		{
			if (!string.IsNullOrEmpty(p_filePath))
			{
				m_pendingImageDownload = new WWW("file:///" + p_filePath);
				yield return m_pendingImageDownload;
				if (m_pendingImageDownload != null)
				{
					if (m_pendingImageDownload.isDone && string.IsNullOrEmpty(m_pendingImageDownload.error))
					{
						if (ICON != null)
						{
							ICON.texture = m_pendingImageDownload.texture;
						}
					}
					else
					{
						Debug.LogError("SteamWorkshopUIUpload: LoadIcon: could not load icon at '" + p_filePath + "'\n" + m_pendingImageDownload.error);
					}
					m_pendingImageDownload = null;
				}
			}
		}
	}
}