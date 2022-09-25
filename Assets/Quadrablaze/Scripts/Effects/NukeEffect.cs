using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeEffect : NetworkedEffectBehaviour {

    public Transform ring;

    public override void OnPlayEffect(string[] parameters) {
        var ringSize = float.Parse(parameters[0]);

        ring.localScale = Vector3.one * ringSize * 2;
        ring.gameObject.SetActive(true);
    }
}
