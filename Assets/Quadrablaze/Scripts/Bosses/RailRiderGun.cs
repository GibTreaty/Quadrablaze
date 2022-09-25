using Quadrablaze;
using UnityEngine;
using YounGenTech.EnergyBasedObjects;
using YounGenTech.PoolGen;
using YounGenTech.YounGenShooter;

//TODO: Fix RailRiderGun
public class RailRiderGun : MonoBehaviour {

    [SerializeField]
    byte _id;

    [SerializeField]
    bool _active;

    [SerializeField]
    GameObject _weaponPrefab;

    [SerializeField]
    Health _healthComponent;

    [SerializeField]
    GameObject _telegraphObject;

    [SerializeField]
    float _telegraphLength = .75f;

    [SerializeField]
    float _telegraphTimer;

    bool _deployed;
    bool _isShooting;
    bool _accuracyRegenDelay;
    bool _isReloading;

    #region Properties
    public Accuracy AccuracyComponent { get; set; }

    public bool Active {
        get { return _active; }
        set { _active = value; }
    }

    public bool Deployed => _deployed;

    public Health HealthComponent {
        get { return _healthComponent; }
        set { _healthComponent = value; }
    } // TODO: [Health]

    public int Id { get; set; }

    public float TelegraphLength {
        get { return _telegraphLength; }
        set { _telegraphLength = value; }
    }

    public float TelegraphTimer {
        get { return _telegraphTimer; }
        set { _telegraphTimer = value; }
    }

    public GameObject WeaponPrefab {
        get { return _weaponPrefab; }
        set { _weaponPrefab = value; }
    }
    #endregion

    public void Conceal() {
        HealthComponent.Reset();
        HealthComponent.Invincible = true;
        //ShootingPoint.AutoShoot = false;
        _telegraphTimer = 0;

        _deployed = false;
        _isShooting = false;
        _accuracyRegenDelay = false;
        _isReloading = false;

        if(_telegraphObject != null) {
            SetTelegraphState(false);
            TelegraphStateHandler.SendTelegraphState(transform.root.gameObject, false, _id);
        }
    }

    public void Deploy() {
        HealthComponent.Invincible = false;
        _telegraphTimer = 0;

        _deployed = true;
        _isShooting = false;
        _accuracyRegenDelay = false;
    }

    public void SetTelegraphState(bool enable) {
        _telegraphObject.SetActive(enable);
    }

    void Update() {
        if(_deployed) {
            if(_telegraphObject != null) {
                if(_isShooting)
                    if(AccuracyComponent != null) {
                        if(!_accuracyRegenDelay)
                            if(AccuracyComponent.AccuracyRegenTime == 1) {
                                _accuracyRegenDelay = true;
                                _isShooting = false;
                                //ShootingPoint.AutoShoot = false;
                            }
                    }
                    else {
                        //if(!_isReloading)
                        //    if(ShootingPoint.LoadTime > 0) {
                        //        _isReloading = true;
                        //        _isShooting = false;
                        //        //ShootingPoint.AutoShoot = false;
                        //    }
                    }

                if(!_isShooting && _accuracyRegenDelay) {
                    if(AccuracyComponent.AccuracyRegenTime == 0)
                        _accuracyRegenDelay = false;
                }
                else if(!_isShooting && _isReloading) {
                    //if(ShootingPoint.LoadTime == 0)
                    //    _isReloading = false;
                }
                else {
                    if(!_isShooting && _telegraphTimer == 0) {
                        _telegraphTimer = _telegraphLength;
                        SetTelegraphState(true);
                        TelegraphStateHandler.SendTelegraphState(transform.root.gameObject, true, _id);
                    }

                    if(_telegraphTimer > 0) {
                        _telegraphTimer = Mathf.Max(_telegraphTimer - Time.deltaTime, 0);

                        if(_telegraphTimer == 0) {
                            _isShooting = true;
                            //ShootingPoint.AutoShoot = true;
                            SetTelegraphState(false);
                            TelegraphStateHandler.SendTelegraphState(transform.root.gameObject, false, _id);
                        }
                    }
                }
            }
        }
    }
}