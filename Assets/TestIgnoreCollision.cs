using UnityEngine;

public class TestIgnoreCollision : MonoBehaviour {

    public EventTimer timer = new EventTimer(.5f) { Active = true, AutoReset = true };
    public GameObject shooter;
    public GameObject projectile;
    public Transform target;
    public float speed = 10;

    public GameObject playerShooter;
    public GameObject enemyShooter;

    void Awake() {
        timer.OnElapsed.AddListener(Shoot);
    }

    void Update() {
        if(shooter != null)
            timer.Update();

        if(Input.GetKeyDown(KeyCode.Space)) {
            var owner = projectile.GetComponent<Owner>();

            if(owner.OwnerObject)
                Physics.IgnoreCollision(projectile.GetComponent<Collider>(), owner.OwnerObject.GetComponent<Collider>(), false);

            shooter = shooter == playerShooter ? enemyShooter : playerShooter;
            target = shooter == playerShooter ? enemyShooter.transform : playerShooter.transform;
        }
    }

    public void Shoot() {
        var owner = projectile.GetComponent<Owner>();
        owner.OwnerObject = shooter;

        projectile.SetActive(false);
        projectile.transform.position = owner.OwnerObject.transform.position;
        projectile.SetActive(true);

        Physics.IgnoreCollision(projectile.GetComponent<Collider>(), owner.OwnerObject.GetComponent<Collider>(), true);

        projectile.GetComponentInChildren<TrailRenderer>().Clear();
        projectile.GetComponent<Rigidbody>().velocity = (target.position - owner.OwnerObject.transform.position).normalized * speed;
    }
}