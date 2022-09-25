using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quadrablaze.Skills {
    public class UIAbilityBar : MonoBehaviour {

        public static UIAbilityBar Current { get; private set; }

        [SerializeField]
        UIAbilityBarIcon _originalIcon;

        [SerializeField]
        Transform _container;

        HashSet<UIAbilityBarIcon> _icons;

        System.Action<int, string> _onUpdateActionInputIcon;

        #region Properties
        public HashSet<UIAbilityBarIcon> Icons {
            get { return _icons; }
            protected set { _icons = value; }
        }
        #endregion

        void OnEnable() {
            Current = this;
            _originalIcon.gameObject.SetActive(false);
        }

        void Awake() {
            Icons = new HashSet<UIAbilityBarIcon>();

            _onUpdateActionInputIcon = UpdateActionInputIcons;

            AbilityWheel.OnUpdateActionInput += _onUpdateActionInputIcon;
        }

        public void AddIcon(SkillLayout layout, SkillLayoutElement element) {
            if(element.CurrentExecutor != null) {
                if(!(element.CurrentExecutor is ICooldownTimer)) return;

                var gameObject = Instantiate(_originalIcon.gameObject, _container, false) as GameObject;
                var icon = gameObject.GetComponent<UIAbilityBarIcon>();

                icon.Initialize(element);
                gameObject.SetActive(true);

                Icons.Add(icon);
            }
        }

        public bool ContainsIcon(SkillLayoutElement element) {
            return Icons.FirstOrDefault(s => s.Element == element) != null;
        }

        public void RemoveAllIcons() {
            foreach(var child in GetComponentsInChildren<UIAbilityBarIcon>(true))
                if(child != _originalIcon)
                    Destroy(child.gameObject);

            Icons.Clear();
        }

        [ContextMenu("Clear Action Input Icons")]
        void ClearActionInputIcons() {
            foreach(var icon in _icons)
                icon.UpdateActionGlyph(-1);
        }

        void UpdateActionInputIcons(int index, string skillId) {
            foreach(var icon in _icons)
                if(icon.Element.OriginalLayoutElement.name == skillId)
                    icon.UpdateActionGlyph(-1);

            if(index > -1)
                foreach(var icon in _icons)
                    if(icon.Element.OriginalLayoutElement.name == skillId) {
                        icon.UpdateActionGlyph(index);

                        return;
                    }
        }
    }
}