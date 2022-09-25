using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DG.Util
{
    public class DropdownFix : MonoBehaviour
    {
        //Fixes a Unity bug that makes the dropdown invisible
        IEnumerator Start()
        {
            Destroy(GetComponent<GraphicRaycaster>());
            yield return null;
            Destroy(GetComponent<Canvas>());
        }
    }
}
