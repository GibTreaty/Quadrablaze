using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;

namespace UIWidgets {
    /// <summary>
    /// TabSelectEvent.
    /// </summary>
    [Serializable]
    public class TabSelectEvent : UnityEvent<int> {
    }

    /// <summary>
    /// Tabs.
    /// http://ilih.ru/images/unity-assets/UIWidgets/Tabs.png
    /// </summary>
    [AddComponentMenu("UI/UIWidgets/Tabs")]
    public class Tabs : MonoBehaviour, IInitialize {
        /// <summary>
        /// The container for tab toggle buttons.
        /// </summary>
        [SerializeField]
        public Transform Container;

        /// <summary>
        /// The default tab button.
        /// </summary>
        [SerializeField]
        public Button DefaultTabButton;

        /// <summary>
        /// The active tab button.
        /// </summary>
        [SerializeField]
        public Button ActiveTabButton;

        [SerializeField]
        Tab[] tabObjects = new Tab[] { };

        /// <summary>
        /// Gets or sets the tab objects.
        /// </summary>
        /// <value>The tab objects.</value>
        public Tab[] TabObjects {
            get {
                return tabObjects;
            }
            set {
                tabObjects = value;
                UpdateButtons();
            }
        }

        /// <summary>
        /// The name of the default tab.
        /// </summary>
        [SerializeField]
        [Tooltip("Tab name which will be active by default, if not specified will be opened first Tab.")]
        public string DefaultTabName = string.Empty;

        /// <summary>
        /// If true does not deactivate hidden tabs.
        /// </summary>
        [SerializeField]
        [Tooltip("If true does not deactivate hidden tabs.")]
        public bool KeepTabsActive = false;

        /// <summary>
        /// OnTabSelect event.
        /// </summary>
        [SerializeField]
        public TabSelectEvent OnTabSelect = new TabSelectEvent();

        /// <summary>
        /// Gets or sets the selected tab.
        /// </summary>
        /// <value>The selected tab.</value>
        public Tab SelectedTab {
            get;
            protected set;
        }

        public UnityEvent OnTabSelectionChanged;

        List<Button> defaultButtons = new List<Button>();
        List<Button> activeButtons = new List<Button>();
        List<UnityAction> callbacks = new List<UnityAction>();

        public virtual void Initialize() {
            if(Container == null) {
                throw new NullReferenceException("Container is null. Set object of type GameObject to Container.");
            }
            if(DefaultTabButton == null) {
                throw new NullReferenceException("DefaultTabButton is null. Set object of type GameObject to DefaultTabButton.");
            }
            if(ActiveTabButton == null) {
                throw new NullReferenceException("ActiveTabButton is null. Set object of type GameObject to ActiveTabButton.");
            }

            if(OnTabSelectionChanged == null) OnTabSelectionChanged = new UnityEvent();


            DefaultTabButton.gameObject.SetActive(false);
            ActiveTabButton.gameObject.SetActive(false);

            UpdateButtons();
        }

        /// <summary>
        /// Start this instance.
        /// </summary>
        public virtual void Start() {
            Initialize();
        }

        /// <summary>
        /// Updates the buttons.
        /// </summary>
        protected virtual void UpdateButtons() {
            if(tabObjects.Length == 0) {
                throw new ArgumentException("TabObjects array is empty. Fill it.");
            }

            RemoveCallbacks();

            CreateButtons();

            AddCallbacks();

            if(!string.IsNullOrEmpty(DefaultTabName)) {
                if(IsExistsTabName(DefaultTabName)) {
                    SelectTab(DefaultTabName);
                }
                else {
                    Debug.LogWarning(string.Format("Tab with specified DefaultTabName \"{0}\" not found. Opened first Tab.", DefaultTabName), this);
                    SelectTab(tabObjects[0].Name);
                }
            }
            else {
                SelectTab(tabObjects[0].Name);
            }
        }

        /// <summary>
        /// Is exists tab with specified name.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <returns>true if exists tab with specified name; otherwise, false.</returns>
        protected virtual bool IsExistsTabName(string tabName) {
            return tabObjects.Any(x => x.Name == tabName);
        }

        /// <summary>
        /// Add callback.
        /// </summary>
        /// <param name="tab">Tab.</param>
        /// <param name="index">Index.</param>
        protected virtual void AddCallback(Tab tab, int index) {
            var tabName = tab.Name;
            UnityAction callback = () => SelectTab(tabName);
            callbacks.Add(callback);

            defaultButtons[index].onClick.AddListener(callbacks[index]);
        }

        /// <summary>
        /// Add callbacks.
        /// </summary>
        protected virtual void AddCallbacks() {
            tabObjects.ForEach(AddCallback);
        }

        /// <summary>
        /// Remove callback.
        /// </summary>
        /// <param name="tab">Tab.</param>
        /// <param name="index">Index.</param>
        protected virtual void RemoveCallback(Tab tab, int index) {
            if((tab != null) && (index < callbacks.Count)) {
                defaultButtons[index].onClick.RemoveListener(callbacks[index]);
            }
        }

        /// <summary>
        /// Remove callbacks.
        /// </summary>
        protected virtual void RemoveCallbacks() {
            if(callbacks.Count > 0) {
                tabObjects.ForEach(RemoveCallback);
                callbacks.Clear();
            }
        }

        /// <summary>
        /// Remove listeners.
        /// </summary>
        protected virtual void OnDestroy() {
            RemoveCallbacks();
        }

        /// <summary>
        /// Selects the tab.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        public void SelectTab(string tabName) {
            var index = Array.FindIndex(tabObjects, x => x.Name == tabName);
            if(index == -1) {
                throw new ArgumentException(string.Format("Tab with name \"{0}\" not found.", tabName));
            }

            if(KeepTabsActive) {
                tabObjects[index].TabObject.transform.SetAsLastSibling();
            }
            else {
                tabObjects.ForEach(DeactivateTab);
                tabObjects[index].TabObject.SetActive(true);
            }

            defaultButtons.ForEach(ActivateButton);
            defaultButtons[index].gameObject.SetActive(false);

            activeButtons.ForEach(DeactivateButton);
            activeButtons[index].gameObject.SetActive(true);

            SelectedTab = tabObjects[index];
            OnTabSelect.Invoke(index);
        }

        /// <summary>
        /// Deactivate tab.
        /// </summary>
        /// <param name="tab">Tab.</param>
        protected virtual void DeactivateTab(Tab tab) {
            tab.TabObject.SetActive(false);
        }

        /// <summary>
        /// Activate button.
        /// </summary>
        /// <param name="button">Button.</param>
        protected virtual void ActivateButton(Button button) {
            button.gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivate button.
        /// </summary>
        /// <param name="button">Button.</param>
        protected virtual void DeactivateButton(Button button) {
            button.gameObject.SetActive(false);
        }

        /// <summary>
        /// Creates the buttons.
        /// </summary>
        protected virtual void CreateButtons() {
            defaultButtons.ForEach(x => x.interactable = true);
            activeButtons.ForEach(x => x.interactable = true);
            if(tabObjects.Length > defaultButtons.Count) {
                for(var i = defaultButtons.Count; i < tabObjects.Length; i++) {
                    var defaultButton = Instantiate(DefaultTabButton) as Button;
                    defaultButton.transform.SetParent(Container, false);

                    //Utilites.FixInstantiated(DefaultTabButton, defaultButton);

                    defaultButtons.Add(defaultButton);

                    var activeButton = Instantiate(ActiveTabButton) as Button;
                    activeButton.transform.SetParent(Container, false);

                    //Utilites.FixInstantiated(ActiveTabButton, activeButton);

                    activeButtons.Add(activeButton);
                }
            }
            //del existing ui elements if necessary
            if(tabObjects.Length < defaultButtons.Count) {
                for(var i = defaultButtons.Count - 1; i > tabObjects.Length - 1; i--) {
                    Destroy(defaultButtons[i].gameObject);
                    Destroy(activeButtons[i].gameObject);

                    defaultButtons.RemoveAt(i);
                    activeButtons.RemoveAt(i);
                }
            }

            defaultButtons.ForEach(SetButtonName);
            activeButtons.ForEach(SetButtonName);
        }


        public Button GetCurrentlyActiveButton() {
            Button button = null;

            for(int i = 0; i < activeButtons.Count; i++)
                if(activeButtons[i].gameObject.activeSelf) {
                    button = activeButtons[i];
                    break;
                }

            return button;
        }

        public int GetCurrentlyActiveButtonIndex() {
            int index = -1;

            for(int i = 0; i < activeButtons.Count; i++)
                if(activeButtons[i].gameObject.activeSelf) {
                    index = i;
                    break;
                }

            return index;
        }

        public void SelectActive() {
            Button currentButton = GetCurrentlyActiveButton();

            if(currentButton)
                EventSystem.current.SetSelectedGameObject(currentButton.gameObject);
        }

        public void SelectNextTab(bool selectElement = true) {
            SelectTab((GetCurrentlyActiveButtonIndex() + 1) % tabObjects.Length, selectElement);
        }
        public void SelectPreviousTab(bool selectElement = true) {
            int index = (GetCurrentlyActiveButtonIndex() - 1) % tabObjects.Length;

            if(index < 0) index += tabObjects.Length;

            SelectTab(index, selectElement);
        }

        /// <summary>
        /// Selects the tab.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        public void SelectTab(string tabName, bool selectElement = true) {
            var index = Array.FindIndex(tabObjects, x => x.Name == tabName);
            if(index == -1) {
                throw new ArgumentException(string.Format("Tab with name \"{0}\" not found.", tabName));
            }
            if(KeepTabsActive) {
                tabObjects[index].TabObject.transform.SetAsLastSibling();
            }
            else {
                tabObjects.ForEach(x => x.TabObject.SetActive(false));
                tabObjects[index].TabObject.SetActive(true);
            }

            defaultButtons.ForEach(x => x.gameObject.SetActive(true));
            defaultButtons[index].gameObject.SetActive(false);

            activeButtons.ForEach(x => x.gameObject.SetActive(false));
            activeButtons[index].gameObject.SetActive(true);

            GameDebug.Log("Select Tab:" + tabName);

            if(selectElement)
                SelectActive();

            OnTabSelectionChanged.Invoke();
        }
        public void SelectTab(int index, bool selectElement = true) {
            SelectTab(tabObjects[index].Name, selectElement);
        }


        /// <summary>
        /// Sets the name of the button.
        /// </summary>
        /// <param name="button">Button.</param>
        /// <param name="index">Index.</param>
        protected virtual void SetButtonName(Button button, int index) {
            var tab_button = button.GetComponent<TabButtonComponentBase>();
            if(tab_button == null) {
                button.gameObject.SetActive(true);

                var textComponent = button.GetComponentInChildren<Text>();

                if(textComponent)
                    textComponent.text = TabObjects[index].Name;

                var textMeshProComponent = button.GetComponentInChildren<TMPro.TMP_Text>();

                if(textMeshProComponent)
                    textMeshProComponent.text = TabObjects[index].Name;
            }
            else {
                tab_button.SetButtonData(TabObjects[index]);
            }
        }

        /// <summary>
        /// Disable the tab.
        /// </summary>
        /// <param name="tab">Tab.</param>
        public virtual void DisableTab(Tab tab) {
            var i = Array.IndexOf(TabObjects, tab);
            if(i != -1) {
                defaultButtons[i].interactable = false;
                activeButtons[i].interactable = false;
            }
        }

        /// <summary>
        /// Enable the tab.
        /// </summary>
        /// <param name="tab">Tab.</param>
        public virtual void EnableTab(Tab tab) {
            var i = Array.IndexOf(TabObjects, tab);
            if(i != -1) {
                defaultButtons[i].interactable = true;
                activeButtons[i].interactable = true;
            }
        }
    }
}