using Quadrablaze;
using Quadrablaze.Effects;
using UnityEngine;

public class SteamWorkshopRenderController : MonoBehaviour {

    public static SteamWorkshopRenderController Current { get; private set; }

    public Camera renderCamera;
    public Transform renderPivot;
    public GameObject currentShipRenderObject;

    void OnEnable() {
        Current = this;
    }

    public void Initialize() {
        Current = this;
    }

    public void CreateShipRenderObject(int shipId) {
        RemoveModel();

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

    public void RemoveModel() {
        if(currentShipRenderObject) Destroy(currentShipRenderObject);
    }

    public void Render() {
        renderCamera.Render();
    }

    public void UpdateSelectedShip() {
        var selectedShipIndex = ShipSelectionUIManager.Current.CurrentShipSelection;

        CreateShipRenderObject(selectedShipIndex);
    }
}