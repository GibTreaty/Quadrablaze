using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SSAA;

public class SuperSampling_SSAA : MonoBehaviour
{
    public float Scale = 0f;

    public bool unlocked = false;

    public SSAAFilter Filter = SSAAFilter.BilinearDefault;

    public bool UseDynamicOutputResolution = false;


    void OnEnable()
    {
        SSAA.internal_SSAA aa = gameObject.AddComponent<SSAA.internal_SSAA>();
        //aa.hideFlags = HideFlags.HideAndDontSave;
        aa.hideFlags = (HideFlags)((int)HideFlags.HideAndDontSave + (int)HideFlags.HideInInspector);
        SSAA.internal_SSAA.UseDynamicOutputResolution = UseDynamicOutputResolution;
        SSAA.internal_SSAA.Filter = Filter;
        SSAA.internal_SSAA.ChangeScale(Scale);
    }

    void OnDisable()
    {
        SSAA.internal_SSAA aa = gameObject.GetComponent<SSAA.internal_SSAA>();
        if(aa != null)
        {
            Destroy(aa);
        }
    }
}