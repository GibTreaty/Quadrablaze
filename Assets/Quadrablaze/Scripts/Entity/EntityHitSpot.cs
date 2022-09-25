using UnityEngine;

public class EntityHitSpot : MonoBehaviour {

    //[SerializeField]
    //bool _affectsMainHealth = true;

    [SerializeField]
    int _healthId;

    [SerializeField]
    bool _isShield;

    //public bool AffectsMainHealth => _affectsMainHealth;
    public int HealthId => _healthId;
    public bool IsShield => _isShield;

}