using UnityEngine;
using System.Collections;

namespace DG.Util
{
    // Custom Coroutine with return result functionality
    public class CustomCoroutine
    {
        public Coroutine coroutine { get; private set; }
        public object result;
        private IEnumerator target;
        public CustomCoroutine(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }
}