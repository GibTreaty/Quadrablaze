using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Workshop")]
    public class WorkshopMenu : ScriptableMenu {

        public override void Close(MenuItem item, UIManager manager) {
            if(uMyGUI_PopupManager.IsInstanceSet) 
               uMyGUI_PopupManager.Instance.HidePopup("steam_ugc_browse");
        }

        public override void Open(MenuItem item, UIManager manager) {
            Debug.Log("Workshop Open");
            var steamWorkshopBrowsePopup = (SteamWorkshopPopupBrowse)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_browse");
            var closeTransform = steamWorkshopBrowsePopup.transform.Find("BG/Title/Close");

            if(closeTransform) {
                var closeButton = closeTransform.GetComponent<Button>();

                closeButton.onClick.AddListener(UIManager.Current.GoToParentMenu);
            }

            SelectFirstElement(item, manager);
        }
    }
}