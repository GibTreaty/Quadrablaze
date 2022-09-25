using System.Collections.Generic;
using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class MineController : MonoBehaviour, IActorEntityObjectInitialize {

        HashSet<Renderer> renderers;

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            renderers = new HashSet<Renderer>(GetComponentsInChildren<Renderer>());

            AnimateMineMaterial.OnColorUpdated += UpdateColor;
        }

        void UpdateColor(MaterialPropertyBlock propertyBlock) {
            foreach(var renderer in renderers)
                renderer.SetPropertyBlock(propertyBlock);
        }
    }
}