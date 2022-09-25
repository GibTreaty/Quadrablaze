using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Quadrablaze.Menu {
    public class Menu : MonoBehaviour {

        static List<Menu> _menuList = new List<Menu>();

        [SerializeField]
        string _menuName = "";

        [SerializeField]
        bool _deactivateOnInitialize = true;

        //[SerializeField]
        //MenuManager _manager;

        [SerializeField]
        Menu _defaultActiveChildMenu;

        [SerializeField]
        Menu _rootMenu;

        [SerializeField]
        GameObject _backgroundGameObject;

        [SerializeField]
        GameObject _menuGameObject;

        [SerializeField]
        Selectable _defaultSelected;

        [SerializeField]
        GameObject _lastSelected;

        //[SerializeField]
        //Menu[] _childMenus;

        [SerializeField]
        UnityEvent _onOpened;

        [SerializeField]
        UnityEvent _onClosed;

        //LinkedListNode<Menu> _menuNode = null;

        #region Properties
        public GameObject BackgroundGameObject {
            get { return _backgroundGameObject; }
            set { _backgroundGameObject = value; }
        }

        //public Menu[] ChildMenus {
        //    get { return _childMenus; }
        //}

        public Selectable DefaultSelected {
            get { return _defaultSelected; }
            set { _defaultSelected = value; }
        }

        public GameObject LastSelected {
            get { return _lastSelected; }
            set { _lastSelected = value; }
        }

        //public MenuManager Manager {
        //    get { return _manager; }
        //}

        public GameObject MenuGameObject {
            get { return _menuGameObject; }
            set { _menuGameObject = value; }
        }

        public static List<Menu> MenuList {
            get { return _menuList; }
        }

        public string MenuName {
            get { return _menuName; }
            set { _menuName = value; }
        }

        //public LinkedListNode<Menu> MenuNode {
        //    get {
        //        if(_menuNode == null)
        //            _menuNode = new LinkedListNode<Menu>(this);

        //        return _menuNode;
        //    }
        //}

        public UnityEvent OnClosed {
            get { return _onClosed; }
        }

        public UnityEvent OnOpened {
            get { return _onOpened; }
        }

        #endregion

        public void Initialize() {
            if(_onClosed == null) _onClosed = new UnityEvent();
            if(_onOpened == null) _onOpened = new UnityEvent();

            if(_deactivateOnInitialize) Close();
        }

        void Update() {
            //if(Manager.CurrentMenu == null || Manager.CurrentMenu.Value != this) return;

            LastSelected = EventSystem.current.currentSelectedGameObject;
        }

        //public bool Contains(Menu menu) {
        //    for(int i = 0; i < ChildMenus.Length; i++)
        //        if(ChildMenus[i] == menu) return true;
        //        else if(ChildMenus[i].Contains(menu)) return true;

        //    return false;
        //}

        //public int IndexOfChildContainigMenu(Menu menu) {
        //    for(int i = 0; i < ChildMenus.Length; i++)
        //        if(ChildMenus[i] == menu || ChildMenus[i].IndexOfChildContainigMenu(menu) > -1)
        //            return i;

        //    return -1;
        //}

        public void Close() {
            Close(true);
        }
        public void Close(bool deactivateBackground) {
            if(deactivateBackground && BackgroundGameObject)
                BackgroundGameObject.SetActive(false);

            MenuGameObject.SetActive(false);
        }

        public void Open() {
            BackgroundGameObject.SetActive(true);
            MenuGameObject.SetActive(true);

            _rootMenu.Open();
            _defaultActiveChildMenu.Open();

            if(DefaultSelected)
                DefaultSelected.Select();
        }

        public static Menu GetMenu(string menuName) {
            return MenuList.Find(s => s.MenuName == menuName);
        }
    }
}