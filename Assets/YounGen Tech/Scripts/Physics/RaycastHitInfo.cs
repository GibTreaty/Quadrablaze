using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YounGenTech {
	public struct RaycastHitInfo {
		public RaycastHit[] hits;
		public Vector3 startPosition;

		public RaycastHitInfo(RaycastHit[] hits, Vector3 startPosition) {
			this.hits = hits;
			this.startPosition = startPosition;
		}
	}

	public static class RaycastHitInfoHelper {
		public static RaycastHitInfo[] Raycast(this Ray[] rays, float maxDistance, LayerMask raycastMask, bool useRaycastAll) {
			List<RaycastHitInfo> list = new List<RaycastHitInfo>();

			if(rays != null)
				foreach(Ray ray in rays) {
					List<RaycastHit> hits = new List<RaycastHit>();

					if(useRaycastAll)
						hits.AddRange(Physics.RaycastAll(ray, maxDistance, raycastMask));
					else {
						RaycastHit hit;

						if(Physics.Raycast(ray, out hit, maxDistance, raycastMask))
							hits.Add(hit);
					}

					if(hits.Count == 0)
						hits.Insert(0, new RaycastHit() { normal = -ray.direction, distance = maxDistance, point = ray.GetPoint(maxDistance) });

					hits.Sort((x, y) => { return x.distance.CompareTo(y.distance); });

					RaycastHitInfo hitInfo = new RaycastHitInfo(hits.ToArray(), ray.origin);

					list.Add(hitInfo);
				}

			return list.ToArray();
		}
	}
}