using UnityEngine;
using System.Collections.Generic;

namespace LapinerTools.uMyGUI {
    public class uMyGUI_PopupManager : MonoBehaviour {
        public const string POPUP_LOADING = "loading";
        public const string POPUP_TEXT = "text";
        public const string POPUP_DROPDOWN = "dropdown";
        public const string BTN_OK = "ok";
        public const string BTN_YES = "yes";
        public const string BTN_NO = "no";

        // singleton pattern
        private static uMyGUI_PopupManager s_instance;
        public static uMyGUI_PopupManager Instance {
            get {
                // first try to find an existing instance
                if(s_instance == null) {
                    s_instance = FindObjectOfType<uMyGUI_PopupManager>();
                }
                // if no instance exists yet, then create a new one
                if(s_instance == null) {
                    s_instance = new GameObject(typeof(uMyGUI_PopupManager).Name).AddComponent<uMyGUI_PopupManager>();
                }
                return s_instance;
            }
        }
        public static bool IsInstanceSet { get { return s_instance != null; } }

        [SerializeField]
        private uMyGUI_Popup[] m_popups = new uMyGUI_Popup[0];
        public uMyGUI_Popup[] Popups {
            get { return m_popups; }
            set { m_popups = value; }
        }
        [SerializeField]
        private string[] m_popupNames = new string[0];
        public string[] PopupNames {
            get { return m_popupNames; }
            set { m_popupNames = value; }
        }
        [SerializeField]
        private CanvasGroup[] m_deactivatedElementsWhenPopupIsShown = new CanvasGroup[0];
        public CanvasGroup[] DeactivatedElementsWhenPopupIsShown {
            get { return m_deactivatedElementsWhenPopupIsShown; }
            set { m_deactivatedElementsWhenPopupIsShown = value; }
        }

        public uMyGUI_Popup ShowPopup(string p_name) {
            for(int i = 0; i < m_popupNames.Length && i < m_popups.Length; i++) {
                if(m_popupNames[i] == p_name) {
                    return ShowPopup(i);
                }
            }
            // requested popup is not yet available -> try to find it in the resources
            if(LoadPopupFromResources(p_name) != null) {
                return ShowPopup(p_name); // popup must be in the list now -> retry
            }
            return null;
        }

        public uMyGUI_Popup HidePopup(string p_name) {
            for(int i = 0; i < m_popupNames.Length && i < m_popups.Length; i++) {
                if(m_popupNames[i] == p_name) {
                    return HidePopup(i);
                }
            }
            return null;
        }

        public uMyGUI_Popup ShowPopup(int p_index) {
            if(p_index >= 0 && p_index < m_popups.Length) {
                m_popups[p_index].Show();
                return m_popups[p_index];
            }
            else {
                Debug.LogError("uMyGUI_PopupManager: ShowPopup: popup index '" + p_index + "' is out of bounds [0," + m_popups.Length + "]!");
                return null;
            }
        }

        public uMyGUI_Popup HidePopup(int p_index) {
            if(p_index >= 0 && p_index < m_popups.Length) {
                m_popups[p_index].Hide();
                return m_popups[p_index];
            }
            else {
                Debug.LogError("uMyGUI_PopupManager: HidePopup: popup index '" + p_index + "' is out of bounds [0," + m_popups.Length + "]!");
                return null;
            }
        }

        public bool IsPopupShown {
            get {
                for(int i = 0; i < m_popups.Length; i++) {
                    if(m_popups[i] != null && m_popups[i].IsShown) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HasPopup(string p_name) {
            for(int i = 0; i < m_popupNames.Length; i++) {
                if(m_popupNames[i] == p_name) {
                    return true;
                }
            }
            return false;
        }

        public bool AddPopup(uMyGUI_Popup p_popup, string p_name) {
            // find popup canvas
            Canvas canvas = null;
            if(m_popups.Length > 0 && m_popups[0] != null && m_popups[0].transform.parent != null) {
                canvas = m_popups[0].transform.parent.GetComponentInParent<Canvas>();
            }
            if(canvas == null) {
                canvas = GetComponentInParent<Canvas>();
            }
            if(canvas == null) {
                //canvas = FindObjectOfType<Canvas>();
                canvas = GameObject.Find("Menu UI").GetComponent<Canvas>();
            }
            if(canvas == null) {
                Debug.LogError("uMyGUI_PopupManager: AddPopup: there is no Canvas in this level!");
                return false;
            }

            // update internal storage
            uMyGUI_Popup[] bufferPopups = m_popups;
            string[] bufferNames = m_popupNames;
            m_popups = new uMyGUI_Popup[m_popups.Length + 1];
            m_popupNames = new string[m_popupNames.Length + 1];
            System.Array.Copy(bufferPopups, m_popups, bufferPopups.Length);
            System.Array.Copy(bufferNames, m_popupNames, bufferNames.Length);
            m_popups[m_popups.Length - 1] = p_popup;
            m_popupNames[m_popups.Length - 1] = p_name;

            // parent new popup
            p_popup.transform.SetParent(canvas.transform, false);

            // hide new popup
            HidePopup(m_popups.Length - 1);

            return true;
        }

        public bool RemovePopup(uMyGUI_Popup p_popup) {
            for(int i = 0; i < m_popups.Length; i++) {
                if(m_popups[i] == p_popup) {
                    List<uMyGUI_Popup> popups = new List<uMyGUI_Popup>(m_popups);
                    popups.RemoveAt(i);
                    m_popups = popups.ToArray();
                    List<string> popupNames = new List<string>(m_popupNames);
                    popupNames.RemoveAt(i);
                    m_popupNames = popupNames.ToArray();
                    return true;
                }
            }
            return false;
        }

        private uMyGUI_Popup LoadPopupFromResources(string p_name) {
            uMyGUI_Popup popupPrefab = (uMyGUI_Popup)Instantiate(Resources.Load<uMyGUI_Popup>("popup_" + p_name + "_root"));
            if(popupPrefab != null) {
                if(AddPopup(popupPrefab, p_name)) {
                    return popupPrefab;
                }
            }
            return null;
        }

        private void Awake() {
            if(m_popups.Length != m_popupNames.Length) {
                Debug.LogError("uMyGUI_PopupManager: m_popups and m_popupNames must have the same length (" + m_popups.Length + "!=" + m_popupNames.Length + ")!");
            }
            // hide all popups
            for(int i = 0; i < m_popups.Length; i++) {
                HidePopup(i);
            }
        }

        private void Update() {
            bool isNotPopupShown = !IsPopupShown;
            for(int i = 0; i < m_deactivatedElementsWhenPopupIsShown.Length; i++) {
                m_deactivatedElementsWhenPopupIsShown[i].interactable = isNotPopupShown;
            }
        }
    }
}
