using UnityEngine;
using System.Linq;

namespace Quadrablaze {
    public class GlobalPlayerTarget : GlobalTargetController {

        public void RandomizeTarget() {
            var array = QuadrablazeSteamNetworking.Current.Players.TakeWhile(s => s.playerInfo.AttachedEntity.CurrentGameObject != null).ToArray();

            if(array.Length > 0)
                Target = array[Random.Range(0, array.Length)].playerInfo.AttachedEntity.CurrentTransform;
            else
                Target = null;
        }
    }
}