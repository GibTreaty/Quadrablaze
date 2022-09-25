using System;
using System.Collections.Generic;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class GlobalTargetController : BaseTargetController {

        static List<GlobalTargetController> _controllers = new List<GlobalTargetController>();

        //User User { get; set; }

        void OnEnable() {
            _controllers.Add(this);
        }

        protected override void OnDisable() {
            base.OnDisable();

            _controllers.Remove(this);
        }

        //void Awake() {
        //    onTargetChanged.AddListener(SetupTargetConnection);
        //}

        //public void SetPooledTarget(PoolManager pool, GameObject gameObject) {
        //    if(gameObject) Target = gameObject.transform;
        //}

        //void SetupTargetConnection(Transform target) {
        //    User = Target ? Target.GetComponent<User>() : null;

        //    if(User) User.onPooled.AddListener(ConnectPoolUser);
        //}

        void ConnectPoolUser() {
            Target = null;

            //User.onPooled.RemoveListener(ConnectPoolUser);
        }

        public static GlobalTargetController GetController(string name) {
            return _controllers.Find(s => s.name == name);
        }
    }
}