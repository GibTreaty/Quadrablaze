using UnityEngine;
using System.Collections;

namespace DG
{
    public class rotate : MonoBehaviour
    {

        public float vel = 1;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.forward * vel * Time.deltaTime);
        }
    }
}