using UnityEngine;

namespace Quadrablaze {
    [System.Serializable]
    public class ShipPreset {

        [SerializeField]
        StringGuid _guid;

        [SerializeField]
        string _name = "";

        [SerializeField, ColorUsage(false)]
        Color _primaryColor;

        [SerializeField, ColorUsage(false)]
        Color _secondaryColor;

        [SerializeField, ColorUsage(false)]
        Color _accessoryPrimaryColor;

        [SerializeField, ColorUsage(false)]
        Color _accessorySecondaryColor;

        [SerializeField, ColorUsage(false, true)]
        Color _glowColor;

        #region Properties
        public Color AccessoryPrimaryColor {
            get { return _accessoryPrimaryColor; }
            set { _accessoryPrimaryColor = value; }
        }

        public Color AccessorySecondaryColor {
            get { return _accessorySecondaryColor; }
            set { _accessorySecondaryColor = value; }
        }

        public Color GlowColor {
            get { return _glowColor; }
            set { _glowColor = value; }
        }

        public StringGuid Guid {
            get { return _guid; }
            set { _guid = value; }
        }

        public bool IsDefault { get; set; }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public Color PrimaryColor {
            get { return _primaryColor; }
            set { _primaryColor = value; }
        }

        public Color SecondaryColor {
            get { return _secondaryColor; }
            set { _secondaryColor = value; }
        }
        #endregion

        public ShipPreset() { }
        public ShipPreset(string name, bool isDefault) {
            Name = name;
            IsDefault = isDefault;
        }
        public ShipPreset(string name, bool isDefault, ShipPreset preset) : this(name, isDefault) {
            SetValues(preset);
        }
        public ShipPreset(string name, bool isDefault, ShipImportSettings settings) : this(name, isDefault) {
            SetValues(settings);
        }

        public Color GetValue(PresetColorType presetColorType) {
            switch(presetColorType) {
                default: return PrimaryColor;
                case PresetColorType.SecondaryColor: return SecondaryColor;
                case PresetColorType.AccessoryPrimaryColor: return AccessoryPrimaryColor;
                case PresetColorType.AccessorySecondaryColor: return AccessorySecondaryColor;
                case PresetColorType.GlowColor: return GlowColor;
            }
        }

        public void SetMaterialValues(Material material) {
            material.SetColor("_PrimaryColor", PrimaryColor);
            material.SetColor("_SecondaryColor", SecondaryColor);
            material.SetColor("_AccessoryPrimaryColor", AccessoryPrimaryColor);
            material.SetColor("_AccessorySecondaryColor", AccessorySecondaryColor);
            material.SetColor("_GlowColor", GlowColor);
        }

        public void SetValue(Color color, PresetColorType presetColorType) {
            switch(presetColorType) {
                case PresetColorType.PrimaryColor: PrimaryColor = color; break;
                case PresetColorType.SecondaryColor: SecondaryColor = color; break;
                case PresetColorType.AccessoryPrimaryColor: AccessoryPrimaryColor = color; break;
                case PresetColorType.AccessorySecondaryColor: AccessorySecondaryColor = color; break;
                case PresetColorType.GlowColor: GlowColor = color; break;
            }
        }

        public void SetValues(ShipPreset preset) {
            PrimaryColor = preset.PrimaryColor;
            SecondaryColor = preset.SecondaryColor;
            AccessoryPrimaryColor = preset.AccessoryPrimaryColor;
            AccessorySecondaryColor = preset.AccessorySecondaryColor;
            GlowColor = preset.GlowColor;
        }
        public void SetValues(ShipImportSettings settings) {
            PrimaryColor = settings.primaryColor;
            SecondaryColor = settings.secondaryColor;
            AccessoryPrimaryColor = settings.accessoryPrimaryColor;
            AccessorySecondaryColor = settings.accessorySecondaryColor;
            GlowColor = settings.glowColor;
        }
    }

    public enum PresetColorType {
        PrimaryColor = 0,
        SecondaryColor = 1,
        AccessoryPrimaryColor = 2,
        AccessorySecondaryColor = 3,
        GlowColor = 4
    }
}