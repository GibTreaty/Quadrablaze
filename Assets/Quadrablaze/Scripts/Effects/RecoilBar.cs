using System;
using UnityEngine;
using YounGenTech.YounGenShooter;
using YounGenTech.Entities;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;

namespace Quadrablaze.Skills {
    public class RecoilBar : MonoBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill {

        [SerializeField]
        int _weaponIndex;

        [SerializeField]
        SpriteRenderer _aimLine;

        [SerializeField]
        Weapon _weaponExecutor;

        SpriteRenderer _spriteRenderer;

        #region Properties
        public int WeaponIndex {
            get { return _weaponIndex; }
            set { _weaponIndex = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            SetAccuracyAmount(0);
            _aimLine?.gameObject.SetActive(false);

            SetupWeaponConnection(null);
        }

        void ChangeRecoil(Accuracy accuracy) {
            SetAccuracyAmount(accuracy.AccuracyRegenTime);
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Weapon executor) {
                if(WeaponIndex == element.ElementTypeIndex) {
                    _weaponExecutor = executor;
                    element.Listener.Subscribe(EntityActions.SkillLevelChanged, SetupWeaponConnection);
                }
            }
        }

        public void SetAccuracyAmount(float accuracy) {
            _spriteRenderer.material.SetFloat("_Fade", accuracy);
        }

        void SetupWeaponConnection(EventArgs args) {
            //var skillArgs = (SkillArgs)args;

            _aimLine?.gameObject.SetActive(false);

            //if(skillArgs.EntityId.GetActorEntity() is ActorEntity actorEntity)
            //    if(actorEntity.CurrentUpgradeSet?.CurrentSkillLayout?.Elements[skillArgs.ElementIndex].CurrentExecutor is Weapon executor) {
            //        //executor.CurrentWeapon
            //    }

            //TODO: [RecoilBar] Redo the recoil bar to work with the new weapon system

            //if(_weaponExeuctor?.EquippedWeapon?.ShootingPoint != null) {
            //    var accuracy = _weaponExeuctor.EquippedWeapon.AccuracyComponent;

            //    if(accuracy)
            //        accuracy.OnAccuracyChanged.AddListener(s => ChangeRecoil(accuracy));

            //    if(_aimLine) {
            //        _aimLine.gameObject.SetActive(true);

            //        Color lineColor = _aimLine.color;
            //        lineColor.a = WeaponIndex == 0 ? .28f : .05f;
            //        _aimLine.color = lineColor;
            //    }
            //}
        }
    }
}