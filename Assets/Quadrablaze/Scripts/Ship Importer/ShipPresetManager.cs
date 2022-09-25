using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Quadrablaze {
    public class ShipPresetManager : MonoBehaviour {
        public const string settingsFileName = "Presets.xml";

        public static ShipPresetManager Current { get; private set; }

        //[SerializeField]
        ShipPresetList _shipPresets = new ShipPresetList();

        #region Properties
        public string PresetFilePath {
            get { return PresetFolderPath + "/" + settingsFileName; }
        }

        public string PresetFolderPath {
            get { return Application.persistentDataPath + "/Custom Ships"; }
        }

        public List<ShipPreset> PresetList {
            get { return _shipPresets.PresetList; }
        }
        #endregion

        public void Initialize() {
            Current = this;

            CreatePresetFolder();
            Load();
        }

        public ShipPreset AddPreset() {
            //var preset = new ShipPreset("New Preset " + (PresetList.Count + 1), false, ShipImporter.defaultShipSettings);

            //preset.Guid = System.Guid.NewGuid();
            //PresetList.Add(preset);

            return AddPreset(null);
        }
        public ShipPreset AddPreset(ShipPreset defaultPreset) {
            string name = "New Preset " + (PresetList.Count + 1);

            var preset = defaultPreset != null ?
                new ShipPreset(name, false, defaultPreset) :
                new ShipPreset(name, false, ShipImporter.defaultShipSettings);

            preset.Guid = System.Guid.NewGuid();
            PresetList.Add(preset);

            return preset;
        }

        public void RemovePreset(int index) {
            PresetList.RemoveAt(index);
        }
        public bool RemovePreset(ShipPreset preset) {
            return PresetList.Remove(preset);
        }


        public void Clear() {
            PresetList.Clear();
        }

        void CreatePresetFolder() {
            if(!Directory.Exists(PresetFolderPath))
                Directory.CreateDirectory(PresetFolderPath);
        }

        public void DeletePreset(ShipPreset preset) {
            PresetList.Remove(preset);
        }

        public void Load() {
            Clear();

            if(Directory.Exists(PresetFolderPath))
                if(File.Exists(PresetFilePath)) {
                    using(var fileReader = File.OpenRead(PresetFilePath))
                    using(var xmlReader = XmlReader.Create(fileReader))
                        _shipPresets = (ShipPresetList)new XmlSerializer(typeof(ShipPresetList)).Deserialize(xmlReader);

                    //var serializer = new XmlSerializer(typeof(ShipPresetList));
                    //using(var reader = new StreamReader(PresetFilePath)) {
                    //    _shipPresets = (ShipPresetList)serializer.Deserializereader.BaseStream);
                    //}
                }
        }

        void ImportFile(string presetPath) {
            string presetFolderPath = Path.GetDirectoryName(presetPath);
            string shipName = Path.GetFileNameWithoutExtension(presetPath);
            string shipSettingsPath = presetFolderPath + "/" + settingsFileName;
        }

        public void Save() {
            CreatePresetFolder();

            using(var fileReader = File.Open(PresetFilePath, FileMode.Create))
            //using(var fileReader = File.OpenWrite(PresetFilePath))
            using(var xmlWriter = XmlWriter.Create(fileReader))
                new XmlSerializer(typeof(ShipPresetList)).Serialize(xmlWriter, _shipPresets);
        }
    }
}