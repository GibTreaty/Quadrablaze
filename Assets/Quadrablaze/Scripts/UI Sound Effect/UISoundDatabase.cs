using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "UI Sound Database/UI Sound Database")]
    public class UISoundDatabase : ScriptableObject {

        [SerializeField]
        AudioClip _selectableEnter;

        [SerializeField]
        AudioClip _selectableExit;

        [SerializeField]
        AudioClip _selectableDown;

        [SerializeField]
        AudioClip _selectableUp;

        [SerializeField]
        AudioClip _selectableSelect;


        [SerializeField]
        AudioClip _selectableSubmit;

        [SerializeField]
        AudioClip _selectableScroll;

        [SerializeField]
        AudioClip _selectableMove;

        public AudioClip SelectableEnter {
            get { return _selectableEnter; }
        }

        public AudioClip SelectableExit {
            get { return _selectableExit; }
        }

        public AudioClip SelectableDown {
            get { return _selectableDown; }
        }

        public AudioClip SelectableUp {
            get { return _selectableUp; }
        }

        public AudioClip SelectableSelect {
            get { return _selectableSelect; }
        }

        public AudioClip SelectableScroll {
            get { return _selectableScroll; }
        }

        public AudioClip SelectableMove {
            get { return _selectableMove; }
        }

        public AudioClip SelectableSubmit {
            get { return _selectableSubmit; }
        }
    }
}