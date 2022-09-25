using UnityEngine;
using TMPro;
using Steamworks;
using Quadrablaze.Entities;

namespace Quadrablaze {
    public class SpectatorHUD : MonoBehaviour {

        public static SpectatorHUD Current { get; private set; }

        [SerializeField]
        TMP_Text _playerNameText;

        public bool IsOpen {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }

        public void Close() {
            gameObject.SetActive(false);
        }

        public void Initialize() {
            Current = this;

            Close();
            SetPlayerNameTest("");
        }

        public void Open() {
            gameObject.SetActive(true);
        }

        public void SetPlayerNameTest(string value) {
            _playerNameText.text = value;
        }


        public static void ChangeSpectatedPlayer(int direction) {
            if(direction != 0) {
                direction = (int)Mathf.Sign(direction);

                if(PlayerProxy.Players.Count > 1) {
                    var list = PlayerProxy.GetOrderedPlayers();
                    PlayerEntity targetEntity = null;

                    if(GameManager.Current.ShooterCameraComponent.Target == null)
                        targetEntity = list[0];
                    else {
                        foreach(var player in list)
                            if(player.CurrentGameObject == GameManager.Current.ShooterCameraComponent.Target) {
                                targetEntity = player;
                                break;
                            }
                    }

                    if(targetEntity == null)
                        targetEntity = list[0];
                    else {
                        var indexOf = list.IndexOf(targetEntity);

                        if(indexOf > -1) {
                            indexOf += direction;
                            indexOf %= list.Count;

                            if(indexOf < 0)
                                indexOf += list.Count;
                        }

                        targetEntity = list[indexOf];
                    }

                    SetSpectatedPlayer(targetEntity);
                }
            }
        }

        public static void SetSpectatedPlayer(PlayerEntity entity) {
            if(entity != null && entity.CurrentGameObject != null) {
                GameManager.Current.ShooterCameraComponent.Target = entity.CurrentTransform;
                Current.SetPlayerNameTest(SteamFriends.GetFriendPersonaName((CSteamID)entity.PlayerInfo.SteamId));
            }
            else {
                GameManager.Current.ShooterCameraComponent.Target = null;
                Current.SetPlayerNameTest("");
            }
        }
    }
}