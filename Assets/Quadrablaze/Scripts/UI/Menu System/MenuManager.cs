using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Quadrablaze.Menu {
    public class MenuManager : MonoBehaviour {

        HashSet<Menu> allMenus;
        LinkedList<Menu> openMenus = new LinkedList<Menu>();

        //[SerializeField]
        //Menu _rootMenu;

        [SerializeField]
        Menu _startMenu;

        //List<Menu> menuList = new List<Menu>();
        //MenuHierarchy menuHierarchy = null;

        #region Properties
        public LinkedListNode<Menu> CurrentMenu {
            get { return openMenus.Last; }
        }

        public LinkedListNode<Menu> RootMenu {
            get { return openMenus.First; }
        }
        #endregion

        void Awake() {
            Initialize();
        }

        public void Initialize() {
            allMenus = new HashSet<Menu>(FindObjectsOfType<Menu>());

            foreach(var menu in allMenus)
                menu.Initialize();

            if(_startMenu)
                Open(_startMenu);
        }

        public void CloseAll() {
            foreach(var menu in allMenus)
                menu.Close();

            openMenus.Clear();
        }

        public void Open(params Menu[] menus) {
            if(menus.Length == 0) return;

            foreach(var menu in menus)
                openMenus.AddLast(menu);

            CurrentMenu.Value.Open();
        }

        public void OpenSingleMenuAdditive(Menu menu) {
            Open(menu);
        }

        public void OpenSingleMenu(Menu menu) {
            CloseAll();
            Open(menu);
        }

        //void Awake() {
        //    if(!_rootMenu) enabled = false;
        //    else {
        //        openMenus.AddFirst(_rootMenu);
        //        menuHierarchy = new MenuHierarchy(_rootMenu, 0);

        //        AddToMenuStructure(menuHierarchy);
        //    }
        //}

        //void AddToMenuStructure(MenuHierarchy menuHierarchy) {
        //    //foreach(var childMenu in menuHierarchy.menu.ChildMenus) {
        //    //    var childMenuHierarchy = new MenuHierarchy(childMenu, menuHierarchy.menu, menuHierarchy.depth + 1);

        //    //    menuHierarchy.subMenuHierarchies.Add(childMenuHierarchy);
        //    //    AddToMenuStructure(childMenuHierarchy);
        //    //}
        //}

        //public void CloseMenu() {
        //    if(CurrentMenu != RootMenu)
        //        GoToMenu(CurrentMenu.Previous.Value);
        //}

        //public List<MenuHierarchy> GetMenuStructure(Menu from, Menu to) {
        //    List<MenuHierarchy> list = new List<MenuHierarchy>();

        //    //var fromHierarchy = GetMenuFromHierarchy(from);
        //    //var toHierarchy = GetMenuFromHierarchy(to);

        //    //if(fromHierarchy == null || toHierarchy == null) return list;

        //    //if(fromHierarchy.depth == toHierarchy.depth) {
        //    //    list.Add(fromHierarchy);
        //    //    list.Add(toHierarchy);
        //    //}
        //    //else {
        //    //    var currentHierarchy = fromHierarchy.depth < toHierarchy.depth ? fromHierarchy : toHierarchy;
        //    //    var findMenu = fromHierarchy.depth < toHierarchy.depth ? to : from;
        //    //    int menuIndex = currentHierarchy.menu.IndexOfChildContainigMenu(findMenu);

        //    //    while(menuIndex > -1) {
        //    //        currentHierarchy = currentHierarchy.subMenuHierarchies[menuIndex];
        //    //        list.Add(currentHierarchy);
        //    //        menuIndex = currentHierarchy.menu.IndexOfChildContainigMenu(findMenu);
        //    //    }
        //    //}

        //    return list;
        //}

        //public MenuHierarchy GetMenuFromHierarchy(Menu menu) {
        //    if(menuHierarchy.Contains(menu))
        //        return GetMenuHierarchyFromHierarchy(menuHierarchy, menu);

        //    return null;
        //}
        //MenuHierarchy GetMenuHierarchyFromHierarchy(MenuHierarchy menuHierarchy, Menu searchForMenu) {
        //    for(int i = 0; i < menuHierarchy.subMenuHierarchies.Count; i++) {
        //        var subMenuHierarcy = menuHierarchy.subMenuHierarchies[i];

        //        if(subMenuHierarcy.menu != searchForMenu)
        //            subMenuHierarcy = GetMenuHierarchyFromHierarchy(subMenuHierarcy, searchForMenu);

        //        if(subMenuHierarcy.menu == searchForMenu) return subMenuHierarcy;
        //    }

        //    return null;
        //}

        //public void GoToMenu(Menu menu) {
        //    if(menu.Manager != this) return;

        //    if(!openMenus.Contains(menu)) { // Open menu
        //        //openMenus
        //    }
        //    else { //Close to menu

        //    }
        //}

        //void OnGUI() {
        //    if(menuHierarchy != null)
        //        VisualizeStructure(menuHierarchy);
        //}

        //public void OpenMenu(int childIndex) {
        //    var length = CurrentMenu.Value.ChildMenus.Length;

        //    if(length > -1 && childIndex < length)
        //        GoToMenu(CurrentMenu.Value.ChildMenus[childIndex]);
        //}
        //public void OpenMenu(string childMenuName) {
        //    for(int i = 0; i < CurrentMenu.Value.ChildMenus.Length; i++)
        //        if(CurrentMenu.Value.ChildMenus[i].MenuName == childMenuName) {
        //            GoToMenu(CurrentMenu.Value.ChildMenus[i]);
        //            return;
        //        }
        //}

        //void VisualizeStructure(MenuHierarchy menuHierarcy) {
        //    if(menuHierarcy.subMenuHierarchies != null) {
        //        GUILayout.BeginVertical(GUI.skin.box);
        //        {
        //            GUILayout.BeginHorizontal();
        //            {
        //                GUILayout.Space(20 * menuHierarcy.depth);
        //                GUILayout.Label(menuHierarcy.name);
        //            }
        //            GUILayout.EndHorizontal();

        //            foreach(var value in menuHierarcy.subMenuHierarchies)
        //                VisualizeStructure(value);
        //        }
        //        GUILayout.EndVertical();
        //    }
        //}

        //public class MenuHierarchy {
        //    public string name;
        //    public int depth;
        //    public Menu menu;
        //    public Menu parentMenu;
        //    public List<MenuHierarchy> subMenuHierarchies;

        //    public MenuHierarchy(Menu menu, Menu parentMenu, int depth) {
        //        this.menu = menu;
        //        this.parentMenu = parentMenu;
        //        this.depth = depth;
        //        name = this.menu.MenuName;
        //        subMenuHierarchies = new List<MenuHierarchy>();
        //    }
        //    public MenuHierarchy(Menu menu, int depth) : this(menu, null, depth) { }

        //    public bool Contains(Menu menu) {
        //        for(int i = 0; i < subMenuHierarchies.Count; i++)
        //            if(subMenuHierarchies[i].menu == menu) return true;
        //            else if(subMenuHierarchies[i].Contains(menu)) return true;

        //        return false;
        //    }
        //}
    }
}
