using Quadrablaze;
using UnityEngine;

[ExecuteInEditMode]
public class WeaponUpgradeShipRenderController : MonoBehaviour {

    public static WeaponUpgradeShipRenderController Current { get; private set; }

    public Material shipRenderMaterial;
    public float speed = .2f;

    public Transform weaponIcon;
    public Transform renderPivot;
    public GameObject currentShipRenderObject;

    float time;

    Material duplicateShipMaterial;

    void OnEnable() {
        Current = this;
    }

    void Update() {
        if(duplicateShipMaterial) {
            time = (time + (Time.unscaledDeltaTime * speed)) % 1;
            duplicateShipMaterial.SetFloat("_LineTime", time);
        }
    }

    public void CreateShipRenderObject(GameObject gameObject) {
        if(!duplicateShipMaterial) 
            duplicateShipMaterial = new Material(shipRenderMaterial);

        if(currentShipRenderObject) Destroy(currentShipRenderObject);

        currentShipRenderObject = Instantiate(gameObject, renderPivot, false);

        foreach(var meshRenderer in currentShipRenderObject.GetComponentsInChildren<MeshRenderer>())
            meshRenderer.sharedMaterial = duplicateShipMaterial;

        currentShipRenderObject.transform.SetParent(renderPivot, false);
        currentShipRenderObject.SetActive(true);

        currentShipRenderObject.transform.gameObject.layer = LayerMask.NameToLayer("Ship Render");

        GameHelper.ForEachChild(currentShipRenderObject.transform, s => { s.gameObject.layer = LayerMask.NameToLayer("Ship Render"); });
    }

    public void MoveIconToWeapon(Vector3 position, Quaternion rotation) {
        weaponIcon.position = position;
        weaponIcon.rotation = rotation;
    }
    public void MoveIconToWeapon(string transformPath) {
        Debug.Log("Transform Path:" + transformPath);
        var childTransform = currentShipRenderObject.transform.Find(transformPath);

        weaponIcon.position = childTransform.position;
        weaponIcon.rotation = childTransform.rotation;
    }
}