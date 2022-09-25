using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using UnityEngine;
using YounGenTech.Entities;

///TODO: Make the HealthHelper use HealthLayer and HealthGroup
//TODO: Use the Entity system

namespace Quadrablaze {
    public static class HealthHelper {
        //Health Event Tags
        //public const string ActorTypeBoss = "{Boss}";
        //public const string ActorTypeEnemy = "{Enemy}";
        //public const string ActorTypePlayer = "{Player}";
        //public const string ActorTypeSmallEnemy = "{SmallEnemy}";
        //public const string ContinuousDamage = "{Continuous}";
        //public const string IgnoreShield = "{IgnoreShield}";
        //public const string IsExplosion = "{IsExplosion}";
        //public const string IsProjectile = "{IsProjectile}";
        //public const string ShieldOnly = "{ShieldOnly}";
        //public const string HasShield = "{Shield}";

        //public const string IsSkill = "{Skill}";
        //public const string Executor_Barrage = IsSkill + ".{Barrage}";

        //public static bool DoHealthChange(this GameObject hit, float percentAmount, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
        //    statEvent = null;

        //    //if(hit.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot)
        //    //    if(GameManager.Current.GetActorEntity(hit, out ActorEntity hitEntity)) {
        //    //        if(hitEntity.CurrentUpgradeSet != null) {
        //    //            var element = hitEntity.CurrentUpgradeSet.CurrentSkillLayout.GetElement<Shield>();

        //    //            if(element != null)
        //    //                if(element.CurrentExecutor is Shield executor)
        //    //                    if(DoHealthChange(executor, amount, sourceObject, continuousEffect, out statEvent))
        //    //                        return true;
        //    //        }

        //    //        if(hitEntity.DoHealthChange(amount, hitSpot.HealthId, sourceObject, continuousEffect, out statEvent))
        //    //            return true;
        //    //    }

        //    return false;
        //}

        //public static bool DoHealthChange(this GameObject hit, float percentAmount, object sourceObject, bool continuousEffect, out StatEvent statEvent) {

        //}

        public static bool DoHealthChange(this GameObject hit, int amount, object sourceObject, bool continuousEffect) {
            return DoHealthChange(hit, amount, sourceObject, continuousEffect, out _);
        }
        public static bool DoHealthChange(this GameObject hit, int amount, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
            statEvent = null;

            if(hit.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot)
                if(GameManager.Current.GetActorEntity(hit, out ActorEntity hitEntity)) {
                    if(hitEntity.CurrentUpgradeSet != null) {
                        var element = hitEntity.CurrentUpgradeSet.CurrentSkillLayout.GetElement<Shield>();

                        if(element != null)
                            if(element.CurrentExecutor is Shield executor)
                                if(DoHealthChange(executor, amount, sourceObject, continuousEffect, out statEvent))
                                    return true;
                    }

                    if(hitEntity.DoHealthChange(amount, hitSpot.HealthId, sourceObject, continuousEffect, out statEvent))
                        return true;
                }

            return false;
        }

        public static bool DoHealthChange(this Shield shield, int amount, object sourceObject, bool continuousEffect) {
            return DoHealthChange(shield, amount, sourceObject, continuousEffect, out _);
        }
        public static bool DoHealthChange(this Shield shieldExecutor, int amount, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
            if(amount < 0) {
                if(shieldExecutor.CurrentActorEntity.InvincibilityTimeRemaining > 0) {
                    statEvent = null;
                    return false;
                }

                if(!continuousEffect) amount *= Shield.ShieldHealthMultiplier;
            }
            //if(!continuousEffect) amount *= Shield.ShieldHealthMultiplier;

            statEvent = shieldExecutor.Health.ModifyValue(shieldExecutor.Health.Value + amount, shieldExecutor, sourceObject, continuousEffect);
            //statEvent = shieldExecutor.Health.ModifyValue(shieldExecutor.Health.Value + amount, shieldExecutor.CurrentActorEntity, sourceObject, continuousEffect);
            var shieldMessage = statEvent.GetMessage<StatChangeValueMessage>();
            var statArgs = new StatEventArgs(statEvent);

            if(shieldMessage.AmountChanged != 0) {
                shieldExecutor.OnStatChanged(statEvent);
                ShieldProxy.Proxy.RaiseEvent(HealthActions.ChangedStat, statArgs);

                //if(shieldExecutor.CurrentActorEntity is PlayerEntity playerEntity)
                //    PlayerProxy.OnShieldChangedHealth.Invoke(playerEntity);

                HitManager.Current.FilterEvent(statEvent);

                if(shieldMessage.AmountChanged < 0) {
                    shieldExecutor.CurrentActorEntity.OnDamaged(statEvent);
                    ShieldProxy.Proxy.RaiseEvent(HealthActions.StatDamaged, statArgs);
                    GetActorEntity(sourceObject)?.OnCausedDamage(statEvent);
                }
                else {
                    shieldExecutor.CurrentActorEntity.OnHealed(statEvent);
                    ShieldProxy.Proxy.RaiseEvent(HealthActions.StatHealed, statArgs);
                    GetActorEntity(sourceObject)?.OnCausedHeal(statEvent);
                }

                return true;
            }

            statEvent = null;

            return false;
        }

        public static bool DoHealthChange(this ActorEntity hitEntity, int amount, int slotId, object sourceObject, bool continuousEffect) {
            return DoHealthChange(hitEntity, amount, slotId, sourceObject, continuousEffect, out _);
        }
        public static bool DoHealthChange(this ActorEntity hitEntity, int amount, int slotId, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
            if(amount < 0)
                if(hitEntity.InvincibilityTimeRemaining > 0) {
                    statEvent = null;
                    return false;
                }

            statEvent = hitEntity.HealthSlots[slotId].ModifyValue(hitEntity.HealthSlots[slotId].Value + amount, hitEntity, sourceObject, continuousEffect);

            var healthMessage = statEvent.GetMessage<StatChangeValueMessage>();

            if(healthMessage.AmountChanged != 0) {
                hitEntity.ChangedHealth(statEvent);

                return true;
            }

            statEvent = null;

            return false;
        }


        #region Percent-based
        public static bool DoHealthChange(this GameObject hit, float maxHealthPercent, int slotId, object sourceObject, bool continuousEffect) {
            return DoHealthChange(hit, maxHealthPercent, slotId, sourceObject, continuousEffect, out _);
        }
        public static bool DoHealthChange(this GameObject hit, float maxHealthPercent, int slotId, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
            statEvent = null;

            if(hit.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot)
                if(GameManager.Current.GetActorEntity(hit, out ActorEntity hitEntity)) {
                    if(hitEntity.CurrentUpgradeSet != null) {
                        var element = hitEntity.CurrentUpgradeSet.CurrentSkillLayout.GetElement<Shield>();

                        if(element != null)
                            if(element.CurrentExecutor is Shield executor)
                                if(DoHealthChange(executor, maxHealthPercent, sourceObject, continuousEffect, out statEvent))
                                    return true;
                    }

                    if(hitEntity.DoHealthChange(maxHealthPercent, hitSpot.HealthId, sourceObject, continuousEffect, out statEvent))
                        return true;
                }

            return false;
        }

        public static bool DoHealthChange(this Shield shield, float maxHealthPercent, object sourceObject, bool continuousEffect) {
            return DoHealthChange(shield, maxHealthPercent, sourceObject, continuousEffect);
        }
        public static bool DoHealthChange(this Shield shield, float maxHealthPercent, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
            int amount = SetMagnitudeOne(shield.Health.MaxValue * maxHealthPercent);

            return DoHealthChange(shield, amount, sourceObject, continuousEffect, out statEvent);
        }

        public static bool DoHealthChange(this ActorEntity hitEntity, float maxHealthPercent, int slotId, object sourceObject, bool continuousEffect) {
            return DoHealthChange(hitEntity, maxHealthPercent, slotId, sourceObject, continuousEffect, out _);
        }
        public static bool DoHealthChange(this ActorEntity hitEntity, float maxHealthPercent, int slotId, object sourceObject, bool continuousEffect, out StatEvent statEvent) {
            int amount = SetMagnitudeOne(hitEntity.HealthSlots[slotId].MaxValue * maxHealthPercent);

            return DoHealthChange(hitEntity, amount, slotId, sourceObject, continuousEffect, out statEvent);
        }
        #endregion

        static ActorEntity GetActorEntity(object sourceObject) {
            ActorEntity sourceEntity;

            switch(sourceObject) {
                case ActorEntity actorEntity: sourceEntity = actorEntity; break;
                case GameObject gameObject: GameManager.Current.GetActorEntity(gameObject, out sourceEntity); break;
                case MonoBehaviour behaviour: GameManager.Current.GetActorEntity(behaviour.gameObject, out sourceEntity); break;
                case SkillExecutor executor: sourceEntity = executor.CurrentActorEntity; break;
                default: sourceEntity = null; break;
            }

            return sourceEntity;
        }

        static int SetMagnitudeOne(float value) {
            var absoluteAmount = Mathf.Abs(value);
            int output = 0;

            if(absoluteAmount > 0 && absoluteAmount < 1)
                output = 1 * (int)Mathf.Sign(value);
            else
                output = (int)(absoluteAmount * Mathf.Sign(value));

            return output;
        }
    }
}