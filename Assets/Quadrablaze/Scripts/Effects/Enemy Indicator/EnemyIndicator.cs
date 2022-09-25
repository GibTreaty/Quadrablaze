using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class EnemyIndicator : MonoBehaviour {

        [SerializeField]
        GameObject _enemyGameObject;

        [SerializeField]
        SpriteRenderer _indicatorSprite;

        Color _defaultColor = Color.white;

        #region Properties
        public GameObject EnemyGameObject {
            get { return _enemyGameObject; }
            set { _enemyGameObject = value; }
        }
        #endregion

        void Awake() {
            _defaultColor = _indicatorSprite.color;
        }

        void LateUpdate() {
            var child = transform.GetChild(0);
            var playerExists = PlayerSpawnManager.Current.CurrentPlayerEntityId > 0;

            if(child.gameObject.activeSelf) {
                if(!playerExists) child.gameObject.SetActive(false);
            }
            else {
                if(playerExists) child.gameObject.SetActive(true);
            }

            bool enemyActive = EnemyGameObject ? EnemyGameObject.activeInHierarchy : false;

            if(!enemyActive) {
                var poolUser = GetComponent<PoolUser>();

                if(poolUser) poolUser.Despawn();
            }
            else {
                if(playerExists) {
                    var playerEntity = PlayerSpawnManager.Current.GetCurrentEntity();

                    // TODO: Null reference after player death
                    transform.position = playerEntity.CurrentTransform.position;
                }

                transform.LookAt(EnemyGameObject.transform, Vector3.up);

                float arenaRadius = GameManager.Current.ArenaRadius;
                float halfArenaRadius = arenaRadius * .5f;

                float distance = Vector3.Distance(transform.position, EnemyGameObject.transform.position);
                float lerpAlphaOutside = Mathf.InverseLerp(160, halfArenaRadius, distance);
                float lerpColor = Mathf.InverseLerp(halfArenaRadius, halfArenaRadius + 15, distance);
                float lerpAlphaInside = Mathf.InverseLerp(halfArenaRadius - 7, halfArenaRadius - 2, distance);
                Color color = Color.Lerp(_defaultColor, Color.white, lerpColor);

                color.a = distance >= halfArenaRadius ? lerpAlphaOutside : lerpAlphaInside;
                _indicatorSprite.color = color;

                var objectOnScreen = EnemyGameObject.GetComponent<ObjectOnScreen>();

                if(objectOnScreen && objectOnScreen.OnScreen) {
                    var poolUser = GetComponent<PoolUser>();

                    if(poolUser) poolUser.Despawn();
                }
            }
        }
    }
}