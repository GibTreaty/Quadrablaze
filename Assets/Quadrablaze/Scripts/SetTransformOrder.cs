using System.Collections;
using UnityEngine;

namespace Quadrablaze {
    public class SetTransformOrder : MonoBehaviour {

        [SerializeField]
        int _orderIndex = -1;

        [SerializeField]
        TransformOrderType _orderType = TransformOrderType.First;

        [SerializeField]
        bool _useUpdate;

        void OnEnable() {
            StartCoroutine("DelayedUpdate");
        }

        //void Update() {
        //    if(_useUpdate) UpdateOrder();
        //}

        IEnumerator DelayedUpdate() {
            yield return null;
            UpdateOrder();
        }

        public void UpdateOrder() {
            var currentIndex = transform.GetSiblingIndex();

            switch(_orderType) {
                case TransformOrderType.First:
                    //if(currentIndex != 0)
                    transform.SetAsFirstSibling();

                    break;
                case TransformOrderType.Last:
                    //if(currentIndex != transform.childCount)
                    transform.SetAsLastSibling();
                    //Debug.Log("Last sibling index:" + transform.GetSiblingIndex() + " Count:" + transform.childCount, this);

                    break;
                case TransformOrderType.Number:
                    //if(currentIndex != _orderIndex)
                    transform.SetSiblingIndex(_orderIndex);

                    break;
            }
        }

        [System.Serializable]
        public enum TransformOrderType {
            Number = 0,
            First = 1,
            Last = 2
        }
    }
}