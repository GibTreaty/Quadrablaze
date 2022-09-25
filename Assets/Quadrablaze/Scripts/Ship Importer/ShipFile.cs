using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Quadrablaze {
    [Serializable]
    public class ShipFile {

        [SerializeField]
        ShipImportSettings _importSettings;

        [SerializeField]
        uint _mainShipId = 0;

        [SerializeField]
        List<ShipMesh> _shipMeshes = new List<ShipMesh>();

        #region Properties
        public ShipImportSettings ImportSettings {
            get { return _importSettings; }
            set { _importSettings = value; }
        }

        public ShipMesh Ship {
            get { return _shipMeshes.Find(inputMesh => inputMesh.id == _mainShipId); }
            set { _mainShipId = value.id; }
        }
        #endregion

        public ShipFile() { }
        public ShipFile(Transform root) {
            SerializeShip(root);
        }

        public GameObject CreateGameObject() {
            if(Ship == null) return null;

            return CreateGameObject(Ship, null);
        }
        GameObject CreateGameObject(ShipMesh shipMesh, GameObject previous) {
            var gameObject = new GameObject(shipMesh.name);

            if(previous)
                gameObject.transform.SetParent(previous.transform, true);

            gameObject.transform.localPosition = previous ? shipMesh.position : Vector3.zero;
            gameObject.transform.localRotation = shipMesh.rotation;
            gameObject.transform.localScale = shipMesh.scale;

            if(shipMesh.mesh?.Length > 0) {
                gameObject.AddComponent<MeshFilter>();
                gameObject.AddComponent<MeshRenderer>();
                shipMesh.ApplyTransform(gameObject.transform);
            }

            foreach(var childId in shipMesh.childMeshIds) {
                var childMesh = _shipMeshes.Find(inputMesh => inputMesh.id == childId);

                CreateGameObject(childMesh, gameObject);
            }

            return gameObject;
        }

        public byte[] SaveToBytes() {
            try {
                var jsonData = JsonUtility.ToJson(this);

                return Encoding.ASCII.GetBytes(jsonData);
            }
            catch(Exception e) {
                Debug.LogError("Error Saving ShipFile to bytes: " + e.Message);
                return null;
            }
        }

        public void SaveToFile(string path, CantTouchThis encrypter) {
            GameDebug.Log("ShipFile.SaveToFile: " + path);

            try {
                using(var memoryStream = new MemoryStream()) {
                    Serialize(memoryStream, this);

                    using(var fileWriter = File.Open(path, FileMode.Create)) {
                        fileWriter.Position = 0;

                        var bytes = encrypter.Encrypt(memoryStream.ToArray());

                        fileWriter.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch(Exception e) {
                Debug.LogError("Error Saving ShipFile: " + e.Message);
            }
        }

        void SerializeShip(Transform root) {
            Ship = ForEachTransform(root, null, _shipMeshes);
        }

        public static ShipFile Deserialize(byte[] bytes) {
            var file = DeserializeFromJson(bytes);

            return file;
        }
        public static ShipFile Deserialize(MemoryStream memoryStream) {
            var file = DeserializeFromJson(memoryStream);

            return file;
        }

        static ShipFile DeserializeFromJson(byte[] bytes) {
            var fileString = Encoding.UTF8.GetString(bytes);
            var shipFile = JsonUtility.FromJson<ShipFile>(fileString);

            return shipFile;
        }
        static ShipFile DeserializeFromJson(MemoryStream memoryStream) {
            var fileString = Encoding.UTF8.GetString(memoryStream.ToArray());
            var shipFile = JsonUtility.FromJson<ShipFile>(fileString);

            return shipFile;
        }

        static ShipMesh ForEachTransform(Transform parent, ShipMesh parentMesh, List<ShipMesh> shipMeshes) {
            var newId = GenerateUniqueID(shipMeshes);
            var newMesh = new ShipMesh(newId, parent, parentMesh);

            shipMeshes.Add(newMesh);

            foreach(Transform transform in parent)
                ForEachTransform(transform, newMesh, shipMeshes);

            return newMesh;
        }

        static uint GenerateUniqueID(IEnumerable<ShipMesh> list) {
            var idList = new List<uint>();

            foreach(var value in list)
                idList.Add(value.id);

            return YounGenTech.IDHelper.GenerateUniqueID(idList);
        }

        public static ShipFile LoadFromFile(string path, CantTouchThis decrypter) {
            GameDebug.Log("ShipFile.LoadFromFile: " + path);

            try {
                byte[] bytes;
                using(var fileReader = File.OpenRead(path))
                using(var reader = new BinaryReader(fileReader)) {
                    bytes = reader.ReadBytes((int)reader.BaseStream.Length);
                    bytes = decrypter.Decrypt(bytes);
                }

                return Deserialize(bytes);
                //using(var memoryStream = new MemoryStream(bytes))
                //    return Deserialize(memoryStream);
            }
            catch(Exception e) {
                throw new Exception("Error Loading ShipFile: " + e.Message);
            }
        }

        public static ShipFile LoadFromBytes(byte[] bytes) {
            try {
                var jsonData = Encoding.ASCII.GetString(bytes);
                var shipFile = JsonUtility.FromJson<ShipFile>(jsonData);

                return shipFile;
            }
            catch(Exception e) {
                throw new Exception("Error Loading ShipFile from bytes: " + e.Message);
            }
        }

        public static void Serialize(MemoryStream memoryStream, ShipFile shipFile) {
            SerializeFromJson(memoryStream, shipFile);
        }

        public static void SerializeFromJson(MemoryStream memoryStream, ShipFile shipFile) {
            using(var writer = new StreamWriter(memoryStream))
                writer.Write(JsonUtility.ToJson(shipFile));
        }

        [Serializable]
        public class ShipMesh {
            public uint id;
            public string name = "";
            public byte[] mesh = null;

            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public List<uint> childMeshIds = new List<uint>();

            public ShipMesh(uint id) { this.id = id; }
            public ShipMesh(uint id, Transform transform) : this(id) {
                name = transform.name;

                position = transform.localPosition;
                rotation = transform.localRotation;
                scale = transform.localScale;

                var meshFilter = transform.GetComponent<MeshFilter>();

                if(meshFilter && meshFilter.sharedMesh)
                    mesh = MeshSerializer.WriteMesh(meshFilter.sharedMesh, false);
            }

            public ShipMesh(uint id, Transform transform, ShipMesh parent) : this(id, transform) {
                parent?.childMeshIds.Add(id);
            }

            public void ApplyTransform(Transform transform) {
                var meshFilter = transform.GetComponent<MeshFilter>();

                if(meshFilter) {
                    var importedMesh = MeshSerializer.ReadMesh(mesh);

                    importedMesh.name = name;
                    meshFilter.sharedMesh = importedMesh;
                }
            }
        }
    }
}

/*using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using LapinerTools.Steam.Data.Internal;
using UnityEngine;

namespace Quadrablaze {
    [Serializable]
    public class ShipFile {

        [SerializeField]
        ShipImportSettings _importSettings;

        [SerializeField]
        ShipMesh _ship = null;

        [NonSerialized]
        Dictionary<ulong, ShipMesh> _shipMeshes;

        ulong _currentId = 0;

        #region Properties
        public ShipImportSettings ImportSettings {
            get { return _importSettings; }
            set { _importSettings = value; }
        }

        public ShipMesh Ship {
            get { return _ship; }
            set { _ship = value; }
        }

        public Dictionary<ulong, ShipMesh> ShipMeshes {
            get { return _shipMeshes; }
            set { _shipMeshes = value; }
        }
        #endregion

        public ShipFile() { _shipMeshes = new Dictionary<ulong, ShipMesh>(); }
        public ShipFile(Transform root) : base() {
            SerializeShip(root);
        }

        public GameObject CreateGameObject() {
            if(Ship == null) return null;

            return CreateGameObject(Ship, null);
        }
        GameObject CreateGameObject(ShipMesh shipMesh, GameObject previous) {
            GameObject gameObject = new GameObject(shipMesh.name);

            if(previous)
                gameObject.transform.SetParent(previous.transform, true);

            gameObject.transform.localPosition = previous ? shipMesh.position : Vector3.zero;
            gameObject.transform.localRotation = shipMesh.rotation;
            gameObject.transform.localScale = shipMesh.scale;

            if(shipMesh.mesh?.Length > 0) {
                gameObject.AddComponent<MeshFilter>();
                gameObject.AddComponent<MeshRenderer>();
                shipMesh.ApplyTransform(gameObject.transform);
            }

            //foreach(var childId in shipMesh.childMeshIds) {
            //    var shipMeshChild = _shipMeshes[childId];

            //    CreateGameObject(shipMeshChild, gameObject);
            //}

            foreach(var child in shipMesh.childMeshes)
                CreateGameObject(child, gameObject);

            return gameObject;
        }

        public void SaveToFile(string path, CantTouchThis encrypter) {
            GameDebug.Log("ShipFile.SaveToFile: " + path);

            try {
                using(var memoryStream = new MemoryStream()) {
                    using(var xmlWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8)) {
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlWriter.Indentation = 4;
                        new XmlSerializer(typeof(ShipFile)).Serialize(xmlWriter, this);
                    }

                    using(var fileWriter = File.Open(path, FileMode.Create)) {
                        fileWriter.Position = 0;

                        var bytes = encrypter.Encrypt(memoryStream.ToArray());

                        fileWriter.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch(Exception e) {
                Debug.LogError("Error Saving ShipFile: " + e.Message);
            }
        }

        public byte[] SaveToBytes() {
            try {
                using(var memoryStream = new MemoryStream()) {
                    using(var xmlWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8)) {
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlWriter.Indentation = 4;
                        new XmlSerializer(typeof(ShipFile)).Serialize(xmlWriter, this);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch(Exception e) {
                Debug.LogError("Error Saving ShipFile to bytes: " + e.Message);
                return null;
            }
        }

        void SerializeShip(Transform root) {
            Ship = ForEachTransform(this, root, null);
        }

        public static ShipFile LoadFromFile(string path, CantTouchThis decrypter) {
            GameDebug.Log("ShipFile.LoadFromFile: " + path);

            try {
                byte[] bytes;
                using(var fileReader = File.OpenRead(path))
                using(var reader = new BinaryReader(fileReader)) {
                    bytes = reader.ReadBytes((int)reader.BaseStream.Length);
                    bytes = decrypter.Decrypt(bytes);
                }

                using(var memoryStream = new MemoryStream(bytes))
                using(var xmlReader = XmlReader.Create(memoryStream))
                    return (ShipFile)new XmlSerializer(typeof(ShipFile)).Deserialize(xmlReader);
            }
            catch(Exception e) {
                Debug.LogError("Error Loading ShipFile: " + e.Message);

                return null;
            }
        }

        public static ShipFile LoadFromBytes(byte[] bytes) {
            try {
                using(var memoryStream = new MemoryStream(bytes))
                using(var xmlReader = XmlReader.Create(memoryStream))
                    return (ShipFile)new XmlSerializer(typeof(ShipFile)).Deserialize(xmlReader);
            }
            catch(Exception e) {
                Debug.LogError("Error Loading ShipFile from bytes: " + e.Message);

                return null;
            }
        }

        static ShipMesh ForEachTransform(ShipFile file, Transform parent, ShipMesh parentMesh) {
            var newMesh = new ShipMesh(parent, parentMesh);
            //var newMesh = new ShipMesh(file._currentId++, parent, parentMesh);

            //file._shipMeshes.Add(newMesh.id, newMesh);

            foreach(Transform transform in parent)
                ForEachTransform(file, transform, newMesh);

            return newMesh;
        }

        [Serializable]
        public class ShipMesh {
            //public ulong id;
            public string name = "";
            public byte[] mesh = null;

            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public List<ShipMesh> childMeshes = new List<ShipMesh>();
            //public List<ulong> childMeshIds = new List<ulong>();

            public ShipMesh() { }
            //public ShipMesh(ulong id) { this.id = id; }
            //public ShipMesh(ulong id, Transform transform) : this(id) {
            //    name = transform.name;

            //    position = transform.localPosition;
            //    rotation = transform.localRotation;
            //    scale = transform.localScale;

            //    var meshFilter = transform.GetComponent<MeshFilter>();

            //    if(meshFilter && meshFilter.sharedMesh)
            //        mesh = MeshSerializer.WriteMesh(meshFilter.sharedMesh, false);
            //}
            //public ShipMesh(ulong id, Transform transform, ShipMesh parent) : this(id, transform) {
            //    parent?.childMeshIds.Add(id);
            //}
            public ShipMesh(Transform transform) : this() {
                name = transform.name;

                position = transform.localPosition;
                rotation = transform.localRotation;
                scale = transform.localScale;

                var meshFilter = transform.GetComponent<MeshFilter>();

                if(meshFilter && meshFilter.sharedMesh)
                    mesh = MeshSerializer.WriteMesh(meshFilter.sharedMesh, false);
            }
            public ShipMesh(Transform transform, ShipMesh parent) : this(transform) {
                parent?.childMeshes.Add(this);
            }

            public void ApplyTransform(Transform transform) {
                var meshFilter = transform.GetComponent<MeshFilter>();

                if(meshFilter) {
                    var importedMesh = MeshSerializer.ReadMesh(mesh);

                    importedMesh.name = name;
                    meshFilter.sharedMesh = importedMesh;
                }
            }
        }
    }
}*/
