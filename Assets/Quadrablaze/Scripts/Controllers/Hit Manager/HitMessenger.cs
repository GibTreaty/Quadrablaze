using UnityEngine;

//TODO: Is this even used?
namespace Quadrablaze {
    public class HitMessenger : MonoBehaviour {


        public void FilterHit(HealthEvent healthEvent) {
            //if(healthEvent.description.Contains(HealthHelper.ContinuousDamage)) {
            //    if(healthEvent.description.Contains(HealthHelper.HasShield))
            //        HitManager.Current.ContinuousShieldHit();
            //    else
            //        HitManager.Current.ContinuousHit();
            //}
            //else {
            //    if(healthEvent.description.Contains(HealthHelper.HasShield))
            //        HitManager.Current.ShieldHit();
            //    else {
            //        if(healthEvent.targetGameObject && healthEvent.targetGameObject.CompareTag("Player"))
            //            HitManager.Current.PlayerDamagedOneHitSound();
            //        else
            //            HitManager.Current.ContinuousHit();
            //    }
            //}
        }
    }
}