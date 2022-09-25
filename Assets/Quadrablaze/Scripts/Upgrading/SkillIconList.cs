using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze.Skills {
    [CreateAssetMenu(fileName = "Skill Icon List", menuName = "Create Skill Icon List")]
    public class SkillIconList : ScriptableObject {

        [SerializeField]
        List<SkillIcon> _skillIconList = new List<SkillIcon>();

        public Sprite GetIcon(string skillName) {
            Sprite sprite = null;

            sprite = _skillIconList.Find(s => s.Name == skillName).Icon;

            return sprite;
        }

        [System.Serializable]
        public struct SkillIcon {

            [SerializeField]
            string _name;

            [SerializeField]
            Sprite _icon;

            #region Properties
            public Sprite Icon {
                get { return _icon; }
                set { _icon = value; }
            }

            public string Name {
                get { return _name; }
                set { _name = value; }
            }
            #endregion

            public SkillIcon(string name, Sprite icon) {
                _name = name;
                _icon = icon;
            }
        }
    }
}