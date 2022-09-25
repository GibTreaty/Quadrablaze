using UnityEngine;

namespace Quadrablaze.Skills {
    public class SkillIconManager : MonoBehaviour {

        public static SkillIconManager Current { get; private set; }

        [SerializeField]
        SkillIconList _skillIconList;

        #region Properties
        public SkillIconList SkillIconList {
            get { return _skillIconList; }
            set { _skillIconList = value; }
        }
        #endregion

        void OnEnable() {
            Current = this;
        }

        public Sprite GetIcon(string skillName) {
            return SkillIconList.GetIcon(skillName);
        }
    }
}