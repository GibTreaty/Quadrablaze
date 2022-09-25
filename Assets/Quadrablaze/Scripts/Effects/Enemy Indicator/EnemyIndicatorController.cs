using UnityEngine;
using YounGenTech.PoolGen;
using Quadrablaze.Boss;

namespace Quadrablaze {
    public class EnemyIndicatorController : MonoBehaviour {

        [SerializeField]
        PoolManager _indicatorPool;

        void Awake() {
            //ObjectOnScreen.OnObjectEnteredScreen.AddListener(RemoveIndicator);
            ObjectOnScreen.OnObjectExitedScreen.AddListener(AddIndicator);
        }

        //void RemoveIndicator(GameObject gameObject) {
        //    foreach(User user in _indicatorPool.PoolGenPrefabs[0].UnpooledObjects)
        //        if(user.GetComponent<EnemyIndicator>().EnemyGameObject == gameObject) {
        //            user.Pool(); break;
        //        }
        //}

        void AddIndicator(GameObject gameObject) {
            if(!gameObject) return;

            var boss = gameObject.GetComponentInParent<BossController>();
            var spawnedObject = _indicatorPool.Spawn(_indicatorPool.IndexFromPrefabID(boss ? 1 : 0));

            if(spawnedObject) {
                var enemyIndicator = spawnedObject.GetComponent<EnemyIndicator>();

                enemyIndicator.EnemyGameObject = boss ? boss.BossObject : gameObject;
            }
        }
    }
}