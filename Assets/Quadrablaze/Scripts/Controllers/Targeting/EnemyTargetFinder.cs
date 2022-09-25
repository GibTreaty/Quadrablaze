using UnityEngine;

namespace Quadrablaze {
    public class EnemyTargetFinder : TargetController {

        [SerializeField]
        string[] _requireSkills = new string[0];

        public override Transform FindTarget() {
            int playerCount = QuadrablazeSteamNetworking.Current.Players.Count;

            if(playerCount > 0) {
                GameParty.Player player = null;
                float lowestDistance = Mathf.Min(SearchRadius * SearchRadius, float.MaxValue);

                foreach(var partyPlayer in QuadrablazeSteamNetworking.Current.Players) {
                    var playerEntity = partyPlayer.playerInfo.AttachedEntity;

                    if(playerEntity == null) continue;
                    if(playerEntity.CurrentGameObject == null) continue;

                    if(_requireSkills.Length > 0)
                        if(!playerEntity.HasAllSkills(_requireSkills)) continue;

                    float distance = (transform.position - playerEntity.CurrentTransform.position).sqrMagnitude;

                    if(distance < lowestDistance) {
                        lowestDistance = distance;
                        player = partyPlayer;
                    }
                }

                if(player != null) 
                    return player.playerInfo.AttachedEntity.CurrentTransform;
            }

            return null;
        }
    }
}