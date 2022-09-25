using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze.Skills {
    [CreateAssetMenu(fileName = "Upgrade Set", menuName = "Create Upgrade Set")]
    public class ScriptableUpgradeSet : ScriptableObject {
        //public static HashSet<ScriptableUpgradeSet> ScriptableUpgradeSets { get; private set; }

        [SerializeField]
        bool _isAsset;

        [SerializeField]
        int _lives;

        [SerializeField]
        int _livesAccumulated;

        [SerializeField]
        int _skillPoints;

        [SerializeField]
        ScriptableSkillLayout _originalSkillLayout;

        string _id = "";

        #region Properties
        public string Id {
            get { return _id; }
            set { _id = value; }
        }

        public bool IsAsset {
            get { return _isAsset; }
        }

        public int Lives {
            get { return _lives; }
            set { _lives = value; }
        }

        public int LivesAccumulated {
            get { return _livesAccumulated; }
            set { _livesAccumulated = value; }
        }

        public ScriptableSkillLayout OriginalSkillLayout {
            get { return _originalSkillLayout; }
            set { _originalSkillLayout = value; }
        }

        public int SkillPoints {
            get { return _skillPoints; }
            set { _skillPoints = value; }
        }
        #endregion

        public ScriptableUpgradeSet Clone() {
            var clone = Instantiate(this);
            //Debug.Log("Clone " + name);
            clone._isAsset = false;

            return clone;
        }

        public UpgradeSet CreateInstance() {
            var skillLayout = _originalSkillLayout.CreateInstance();

            return new UpgradeSet(Id, Lives, LivesAccumulated, SkillPoints, skillLayout);
        }
    }
}