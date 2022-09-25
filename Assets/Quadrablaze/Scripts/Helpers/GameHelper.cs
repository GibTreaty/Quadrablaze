using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze {
    public static class GameHelper {
        public static void ForEachChild(Transform parent, Action<Transform> action) {
            foreach(Transform transform in parent) {
                action(transform);

                ForEachChild(transform, action);
            }
        }

        public static Transform FindChildRecurvisely(string name, Transform parent) {
            foreach(Transform transform in parent)
                if(transform.name.Contains(name))
                    return transform;
                else {
                    Transform found = FindChildRecurvisely(name, transform);

                    if(found) return found;
                }

            return null;
        }

        public static void GetAllChildrenRecurvisely(string name, Transform parent, ICollection<Transform> children) {
            foreach(Transform transform in parent) {
                if(transform.name.Contains(name))
                    children.Add(transform);

                GetAllChildrenRecurvisely(name, transform, children);
            }
        }

        public static string GetTransformPath(this Transform from, Transform to) {
            if(!to.IsChildOf(from)) return "";

            Transform currentTransform = to;
            string path = currentTransform.name;
            
            while(currentTransform != from) {
                currentTransform = currentTransform.parent;
                path = currentTransform.name + "/" + path;
            }

            return path;
        }
    }
}