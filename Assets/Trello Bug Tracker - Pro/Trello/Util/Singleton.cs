using UnityEngine;

namespace DG.Util
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        /**
           Returns the instance of this singleton.
        */
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    //if (_instance == null)
                    //{
                    //    Debug.LogError("An _instance of " + typeof(T) +
                    //       " is needed in the scene, but there is none.");
                    //}
                }

                return _instance;
            }
            protected set { _instance = value;}
        }

        //protected virtual void Awake()
        //{
        //    if (Instance != this) Destroy(gameObject);

        //    //Sets this to not be destroyed when reloading scene
        //    DontDestroyOnLoad(gameObject);
        //}
    }
}