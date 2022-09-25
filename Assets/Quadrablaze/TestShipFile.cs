using Quadrablaze;
using System.IO;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class TestShipFile : MonoBehaviour {

    public CantTouchThis encrypter;
    public Transform shipObject;
    public Transform importedShipObject;
    public Material defaultMaterial;
    public ShipFile shipFile;

    void Awake() {
        CreateShipFile();
        Save();
        Load();
        ImportShip(shipFile);
    }

    void CreateShipFile() {
        shipFile = new ShipFile(shipObject);
    }

    void ImportShip(ShipFile file) {
        importedShipObject = file.CreateGameObject()?.transform;

        foreach(var renderer in importedShipObject.GetComponentsInChildren<Renderer>())
            renderer.sharedMaterial = defaultMaterial;
    }

    void Save() {
        string path = Path.Combine(Application.persistentDataPath, "ShipFileTest.qship");

        shipFile.SaveToFile(path, encrypter);        
    }

    void Load() {
        string path = Path.Combine(Application.persistentDataPath, "ShipFileTest.qship");

        shipFile = ShipFile.LoadFromFile(path, encrypter);
    }
}