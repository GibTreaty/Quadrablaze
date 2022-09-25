using Quadrablaze;
using Quadrablaze.Effects;
using UnityEngine;

public class ShipSelectionRenderController : MonoBehaviour {

    public static ShipSelectionRenderController Current { get; private set; }

    public Transform renderPivot;
    public GameObject currentShipRenderObject;

    Quaternion defaultRotation;

    void OnEnable() {
        Current = this;
    }

    public void Initialize() {
        Current = this;
        defaultRotation = renderPivot.rotation;
        //UpdateSelectedShip();
    }

    void Update() {
        if(currentShipRenderObject)
            renderPivot.transform.rotation *= Quaternion.Euler(0, Time.unscaledDeltaTime * 10, 0);
    }

    public void CreateShipRenderObject(int shipId) {
        if(currentShipRenderObject) Destroy(currentShipRenderObject);

        renderPivot.rotation = defaultRotation;

        currentShipRenderObject = Instantiate(ShipImporter.Current.localShipData[shipId].rootMeshObject, renderPivot, false);
        currentShipRenderObject.SetActive(true);

        ShipImporter.Current.ApplyJetParticles(currentShipRenderObject.transform, ShipImporter.Current.GetShipImportSettings(shipId));

        var animator = currentShipRenderObject.GetComponentInChildren<Animator>();

        if(animator)
            animator.StopPlayback();

        foreach(var audio in currentShipRenderObject.GetComponentsInChildren<AudioSource>())
            audio.mute = true;

        foreach(var jetParticles in currentShipRenderObject.GetComponentsInChildren<JetParticles>()) {
            jetParticles.JetControl = JetParticles.JetControlType.Manual;
            jetParticles.JetPower = 1;
            jetParticles.ForceSimulate = true;
        }

        currentShipRenderObject.layer = LayerMask.NameToLayer("Ship Render");
        GameHelper.ForEachChild(currentShipRenderObject.transform, s => { s.gameObject.layer = LayerMask.NameToLayer("Ship Render"); });
    }

    public void UpdateSelectedShip() {
        var selectedShipIndex = ShipSelectionUIManager.Current.CurrentShipSelection;
        
        CreateShipRenderObject(selectedShipIndex);
    }
}