using System;
using System.Collections.Generic;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    [Obsolete("Turn this into a static class")]
    public class NetworkSkillController : NetworkBehaviour {

        [Client]
        public void TryUseAbility(ScriptableUpgradeSet upgradeSet, SkillLayoutElement skillLayoutElement) {
            var writer = new NetworkWriter();

            //if(upgradeSet.CurrentSkillLayout.HasSkill(skillLayoutElement)) {
            //    var index = upgradeSet.CurrentSkillLayout.SkillElements.IndexOf(skillLayoutElement);

            //    writer.StartMessage(NetMessageType.Server_UseAbility);
            //    writer.Write(gameObject);
            //    writer.Write(index);
            //    writer.FinishMessage();

            //    QuadrablazeSteamNetworking.Current.MyClient.connection.SendWriter(writer, Channels.DefaultReliable);
            //}
        }
    }
}