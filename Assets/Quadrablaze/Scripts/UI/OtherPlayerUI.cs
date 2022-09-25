using System.Collections.Generic;
using Quadrablaze.Entities;
using StatSystem;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class OtherPlayerUI : MonoBehaviour {

        public static HashSet<OtherPlayerUI> otherPlayers = null;

        [SerializeField]
        Slider _healthSlider;

        [SerializeField]
        Image _playerAvatarImage;

        [SerializeField]
        TMP_Text _playerNameText;

        CSteamID _playerSteamId;

        #region Properties
        public CSteamID PlayerSteamId {
            get { return _playerSteamId; }
            set { _playerSteamId = value; }
        }
        #endregion

        void OnDisable() {
            otherPlayers.Remove(this);
            PlayerProxy.Proxy.Unsubscribe(EntityActions.ChangedStat, PlayerProxy_ChangedStat);
            PlayerProxy.Proxy.Unsubscribe(EntityActions.Despawned, PlayerProxy_Despawned);
        }

        void OnEnable() {
            if(otherPlayers == null) otherPlayers = new HashSet<OtherPlayerUI>();

            otherPlayers.Add(this);

            PlayerProxy.Proxy.Subscribe(EntityActions.ChangedStat, PlayerProxy_ChangedStat);
            PlayerProxy.Proxy.Subscribe(EntityActions.Despawned, PlayerProxy_Despawned);
        }

        void PlayerProxy_ChangedStat(System.EventArgs args) {
            var stat = ((StatEventArgs)args).Stat;

            if(stat.AffectedStat.Id == 0)
                UpdateHealthSlider((PlayerEntity)stat.AffectedObject);
        }

        void PlayerProxy_Despawned(System.EventArgs args) {
            RemoveSelf(((EntityArgs)args).GetEntity<PlayerEntity>());
        }

        void RemoveSelf(PlayerEntity entity) {
            if(entity.PlayerInfo.SteamId == (ulong)_playerSteamId)
                Destroy(gameObject);
        }

        public void SetAvatar(Sprite sprite) {
            _playerAvatarImage.sprite = sprite;
        }

        public void SetName(string name) {
            _playerNameText.text = name;
        }

        void UpdateHealthSlider(PlayerEntity entity) {
            if(entity != null && entity.PlayerInfo && entity.PlayerInfo.SteamId == (ulong)_playerSteamId)
                _healthSlider.value = entity.HealthSlots[0].NormalizedValue;
        }
    }
}