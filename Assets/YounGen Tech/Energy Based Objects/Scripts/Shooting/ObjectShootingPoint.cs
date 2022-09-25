using UnityEngine;
using UnityEngine.Events;
using System;
using YounGenTech.ComponentInterface;

namespace YounGenTech.EnergyBasedObjects {

    /// <summary>
    /// The base script from which objects can shoot from
    /// </summary>
    [AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Shooting Point")]
    public class ObjectShootingPoint : MonoBehaviour {
        /// <summary>
        /// Called when the weapon is shot
        /// Requires ObjectShootPoint as a parameter
        /// </summary>
        public event Action<ObjectShootingPoint> OnWeaponShoot;

        /// <summary>
        /// Called when the weapon is loaded
        /// Requires ObjectShootPoint as a parameter
        /// </summary>
        public event Action<ObjectShootingPoint> OnWeaponLoad;

        /// <summary>
        /// Called when the weapon is shot
        /// </summary>
        public event Action OnWeaponShoot_NoParameters;

        /// <summary>
        /// Called when the weapon is loaded
        /// </summary>
        public event Action OnWeaponLoad_NoParameters;

        public ShootingEvent onWeaponShoot;
        public ShootingEvent onWeaponLoad;


        /// <summary>
        /// Acts as an array of functions/delegates to check if this weapon can shoot
        /// It is only checked when loading the weapon
        /// </summary>
        public event Func<bool> CanShoot;

        [SerializeField]
        /// <summary>
        /// Automatically shoot
        /// </summary>
        private bool autoShoot;
        /// <summary>
        /// Automatically load when you shoot
        /// </summary>
        public bool autoLoadOnShoot = true;
        /// <summary>
        /// Automatically load
        /// </summary>
        public bool autoLoad;

        [SerializeField]
        /// <summary>
        /// Delay between each shot
        /// </summary>
        private float shootDelay = .25f;

        /// <summary>
        /// Time left until you can shoot again
        /// </summary>
        private float shootTime;

        /// <summary>
        /// Has the weapon been loaded
        /// </summary>
        public bool shotLoaded { get { return loadCount > 0 && isLoadCountAtMin; } }
        /// <summary>
        /// How many times you can load the weapon
        /// </summary>
        public int loadLimit = 1;
        /// <summary>
        /// How many times you have loaded the weapon
        /// </summary>
        public int loadCount;

        [SerializeField]
        /// <summary>
        /// Delay between loading
        /// </summary>
        private float loadDelay;

        /// <summary>
        /// Time left until you can load again
        /// </summary>
        private float loadTime;

        /// <summary>
        /// Load this amount of times before allowing the weapon to shoot
        /// </summary>
        public int minLoadCountToShoot;

        /// <summary>
        /// If the loadTime is above zero, don't shoot
        /// </summary>
        public bool shootOnLoadTimerZero = true;
        /// <summary>
        /// Reset loadTime to zero when shot
        /// </summary>
        public bool resetLoadTimeOnShoot;

        /// <summary>
        /// Is shootTime zero
        /// </summary>
        public bool isShootTimerZero {
            get { return ShootTime == 0; }
        }
        /// <summary>
        /// Is loadTime zero
        /// </summary>
        public bool isLoadTimerZero {
            get { return LoadTime == 0; }
        }
        /// <summary>
        /// Is loadCount greater or equal to minLoadCountToShoot
        /// </summary>
        public bool isLoadCountAtMin {
            get { return loadCount >= minLoadCountToShoot; }
        }
        /// <summary>
        /// Is this weapon ready to Load
        /// </summary>
        public bool canLoad {
            get { return isShootTimerZero && isLoadTimerZero && loadCount < loadLimit && CheckCanShootFunctions(); }
        }

        public bool AutoShoot {
            get { return autoShoot; }
            set { autoShoot = value; }
        }

        public float LoadDelay {
            get { return loadDelay; }
            set { loadDelay = value; }
        }

        public float LoadTime {
            get { return loadTime; }
            set { loadTime = value; }
        }

        public float ShootDelay {
            get { return shootDelay; }
            set { shootDelay = value; }
        }

        public float ShootTime {
            get { return shootTime; }
            set { shootTime = value; }
        }

        public float TotalDelay {
            get { return ShootDelay + LoadDelay; }
        }

        /// <summary>
        /// Sends a message up through the hierarchy that it was enabled
        /// </summary>
        void OnEnable() {
            SendMessage("OnComponentWasEnabled", this, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Sends a message up through the hierarchy that it was disabled
        /// </summary>
        void OnDisable() {
            SendMessage("OnComponentWasDisabled", this, SendMessageOptions.DontRequireReceiver);
        }

        void Update() {
            if(ShootTime > 0) {
                ShootTime -= Time.deltaTime;

                if(ShootTime < 0) ShootTime = 0;
            }

            if(LoadTime > 0) {
                LoadTime -= Time.deltaTime;

                if(LoadTime < 0) LoadTime = 0;
            }

            if(autoLoad)
                if(isLoadTimerZero)
                    Load();

            if(autoShoot)
                if(isShootTimerZero)
                    Shoot();
        }

        [ContextMenu("Shoot")]
        public virtual void Shoot() {
            if(!shotLoaded && autoLoadOnShoot) Load();

            if(shotLoaded && (shootOnLoadTimerZero ? isLoadTimerZero : true)) {
                if(OnWeaponShoot_NoParameters != null) OnWeaponShoot_NoParameters();
                if(OnWeaponShoot != null) OnWeaponShoot(this);
                if(onWeaponShoot != null) onWeaponShoot.Invoke(this);

                if(resetLoadTimeOnShoot) LoadTime = 0;

                ShootTime = shootDelay;
                loadCount = 0;
            }
        }

        [ContextMenu("Load")]
        public virtual void Load() {
            if(canLoad) {
                loadCount++;
                LoadTime = loadDelay;

                if(OnWeaponLoad_NoParameters != null) OnWeaponLoad_NoParameters();
                if(OnWeaponLoad != null) OnWeaponLoad(this);
                if(onWeaponLoad != null) onWeaponLoad.Invoke(this);
            }
        }

        public bool CheckCanShootFunctions() {
            if(CanShoot == null) return true;

            foreach(Func<bool> shootingEvent in CanShoot.GetInvocationList()) {
                if(!shootingEvent()) return false;
            }

            return true;
        }

        public static Vector3 GetRandomDirection(float scale) {
            Vector3 random = Vector3.ClampMagnitude(new Vector3(1 - (UnityEngine.Random.value * 2), 1 - (UnityEngine.Random.value * 2), 1 - (UnityEngine.Random.value * 2)), 1);

            return random * scale;
        }
        public static Vector3 GetRandomDirection(Vector3 scale) {
            Vector3 random = Vector3.ClampMagnitude(new Vector3(1 - (UnityEngine.Random.value * 2), 1 - (UnityEngine.Random.value * 2), 1 - (UnityEngine.Random.value * 2)), 1);

            return Vector3.Scale(random, scale);
        }

        void OnComponentWasEnabled(Component component) {
            this.ConnectComponentEventTo(component);
        }

        void OnComponentWasDisabled(Component component) {
            this.DisconnectComponentEventFrom(component);
        }

        [Serializable]
        public class ShootingEvent : UnityEvent<ObjectShootingPoint> { }
    }
}