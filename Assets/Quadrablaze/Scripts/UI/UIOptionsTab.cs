using UnityEngine;
using Rewired;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Quadrablaze {
    public class UIOptionsTab : MonoBehaviour {

        public Selectable selectOnEnabled;

        public Transform toolContainer;

        [Header("Menu Buttons")]
        public Button saveButton;
        public Button cancelButton;
        public Button defaultButton;
        public Button closeButton;

        protected virtual void OnEnable() {
            if(selectOnEnabled)
                StartCoroutine("DoSelect");
        }

        IEnumerator DoSelect() {
            yield return new WaitForEndOfFrame();

            if(selectOnEnabled) { // TODO *** UI Element Highlighting Issue Fix Here
                selectOnEnabled.Select();
                selectOnEnabled.OnSelect(null);
            }
        }

        public virtual void Initialize() {
            saveButton.onClick.AddListener(SavePrefs);
            cancelButton.onClick.AddListener(LoadPrefs);
            defaultButton.onClick.AddListener(SetToDefault);
            closeButton.onClick.AddListener(() => UIManager.Current.GoToParentMenu());

            saveButton.transform.SetSiblingIndex(0);
            cancelButton.transform.SetSiblingIndex(1);
            defaultButton.transform.SetSiblingIndex(2);
            closeButton.transform.SetSiblingIndex(3);
        }

        public virtual void LoadPrefs() { }
        public virtual void SavePrefs() { }
        public virtual void SetToDefault() { }

        public void SetupMenuNavigation() {
            Selectable firstSelectable = null;
            Selectable lastSelectable = null;

            if(toolContainer != null) {
                List<Selectable> selectables = new List<Selectable>();

                foreach(Selectable selectable in toolContainer.GetComponentsInChildren<Selectable>())
                    if(selectable.CompareTag("Menu Control"))
                        selectables.Add(selectable);

                if(selectables.Count > 0) {
                    firstSelectable = selectables.First();
                    lastSelectable = selectables.Last();

                    for(int i = 0; i < selectables.Count; i++) {
                        var currentSelectable = selectables[i];

                        if(i == 0) { // First
                            currentSelectable.navigation = new Navigation() {
                                mode = Navigation.Mode.Explicit,
                                selectOnUp = saveButton,
                                selectOnDown = selectables.Count > 1 ? selectables[i + 1] : saveButton
                            };
                        }
                        else if(selectables.Count > 1) {
                            if(i == selectables.Count - 1) { // Last
                                currentSelectable.navigation = new Navigation() {
                                    mode = Navigation.Mode.Explicit,
                                    selectOnUp = selectables[i - 1],
                                    selectOnDown = saveButton
                                };
                            }
                            else if(selectables.Count > 2) { // Middle
                                currentSelectable.navigation = new Navigation() {
                                    mode = Navigation.Mode.Explicit,
                                    selectOnUp = selectables[i - 1],
                                    selectOnDown = selectables[i + 1]
                                };
                            }
                        }
                    }
                }
            }

            saveButton.navigation = new Navigation() {
                mode = Navigation.Mode.Explicit,
                selectOnUp = lastSelectable,
                selectOnDown = firstSelectable,
                selectOnLeft = closeButton,
                selectOnRight = cancelButton
            };

            cancelButton.navigation = new Navigation() {
                mode = Navigation.Mode.Explicit,
                selectOnUp = lastSelectable,
                selectOnDown = firstSelectable,
                selectOnLeft = saveButton,
                selectOnRight = defaultButton
            };

            defaultButton.navigation = new Navigation() {
                mode = Navigation.Mode.Explicit,
                selectOnUp = lastSelectable,
                selectOnDown = firstSelectable,
                selectOnLeft = cancelButton,
                selectOnRight = closeButton
            };

            closeButton.navigation = new Navigation() {
                mode = Navigation.Mode.Explicit,
                selectOnUp = lastSelectable,
                selectOnDown = firstSelectable,
                selectOnLeft = defaultButton,
                selectOnRight = saveButton
            };
        }
    }
}