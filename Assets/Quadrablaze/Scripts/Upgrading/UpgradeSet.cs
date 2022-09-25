using System.Collections.Generic;

namespace Quadrablaze.Skills {
    public class UpgradeSet {

        public string Id { get; set; }
        public int Lives { get; set; }
        public int LivesAccumulated { get; set; }
        public int SkillPoints { get; set; }
        public SkillLayout CurrentSkillLayout { get; protected set; }

        public UpgradeSet(string id, int lives, int livesAccumulated, int skillPoints, SkillLayout currentSkillLayout) {
            Id = id;
            Lives = lives;
            LivesAccumulated = livesAccumulated;
            SkillPoints = skillPoints;
            CurrentSkillLayout = currentSkillLayout;
        }
    }
}