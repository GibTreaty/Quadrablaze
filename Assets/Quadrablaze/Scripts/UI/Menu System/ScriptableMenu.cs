using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Basic Menu")]
    public class ScriptableMenu : ScriptableObject {

        [SerializeField]
        string _menuName = "";

        [SerializeField]
        MenuOptions _options;

        public string MenuName => _menuName;

        public MenuOptions Options => _options;

        public virtual void Close(MenuItem item, UIManager manager) {

            if(item.MainGameObject != null)
                item.MainGameObject.SetActive(false);

            foreach(var gameObject in item.SubGameObjects)
                gameObject.SetActive(false);

            if(_options.hideBackgroundOnClose)
                manager.LerpBackgroundAlpha(false);

            OnClose(item, manager);
        }

        protected virtual void OnClose(MenuItem item, UIManager manager) { }

        protected virtual void OnOpen(MenuItem item, UIManager manager) { }

        public virtual void Open(MenuItem item, UIManager manager) {
            manager.DeactivateTitle();

            if(item.MainGameObject != null)
                item.MainGameObject.SetActive(true);

            foreach(var gameObject in item.SubGameObjects)
                gameObject.SetActive(true);

            var activeSelectable = item.GetActiveSelectable();

            if(activeSelectable != null && activeSelectable.isActiveAndEnabled)
                activeSelectable.Select();
            else
                SelectFirstElement(item, manager);

            if(_options.showBackground)
                manager.LerpBackgroundAlpha(true);

            if(_options.hideEndMessageOnOpen) {
                manager.ShowGameOver(false);
                manager.ShowGoalCompleted(false);
            }

            OnOpen(item, manager);
        }

        public virtual void SelectFirstElement(MenuItem item, UIManager manager) {
            if(item.MainGameObject != null) {
                var firstSelectable = item.MainGameObject.GetComponentInChildren<Selectable>(false);

                if(firstSelectable != null) {
                    EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
                    firstSelectable.Select();
                }
            }
        }
    }
}