using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Steamworks;
using UnityEngine;

namespace Quadrablaze {
    [System.Serializable]
    public struct ShipImportSettings {
        public PublishedFileId_t workshopPublishedFileID;
        public Color primaryColor;
        public Color secondaryColor;
        public Color accessoryPrimaryColor;
        public Color accessorySecondaryColor;
        public Color glowColor;
        public Controls controlSettings;
        public List<string> jetPivots;
        public List<string> rotatingObjects;
        public List<string> weaponPivots;

        public byte[] ToBytes() {
            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream)) {
                writer.Write(workshopPublishedFileID.m_PublishedFileId);

                WriteColor(writer, primaryColor);
                WriteColor(writer, secondaryColor);
                WriteColor(writer, accessoryPrimaryColor);
                WriteColor(writer, accessorySecondaryColor);
                WriteColor(writer, glowColor);

                writer.Write((byte)controlSettings.movementStyle);

                WriteStringList(writer, jetPivots);
                WriteStringList(writer, rotatingObjects);
                WriteStringList(writer, weaponPivots);

                return stream.ToArray();
            }
        }

        public ShipImportSettings(byte[] bytes) {
            using(var stream = new MemoryStream(bytes))
            using(var reader = new BinaryReader(stream)) {
                workshopPublishedFileID = new PublishedFileId_t(reader.ReadUInt64());

                primaryColor = ReadColor(reader);
                secondaryColor = ReadColor(reader);
                accessoryPrimaryColor = ReadColor(reader);
                accessorySecondaryColor = ReadColor(reader);
                glowColor = ReadColor(reader);

                var movementStyle = (BaseMovementController.MovementType)System.Enum.Parse(typeof(BaseMovementController.MovementType), reader.ReadByte().ToString());

                controlSettings = new Controls() { movementStyle = movementStyle };

                jetPivots = ReadStringList(reader);
                rotatingObjects = ReadStringList(reader);
                weaponPivots = ReadStringList(reader);
            }
        }

        static Color ReadColor(BinaryReader reader) {
            var r = reader.ReadSingle();
            var g = reader.ReadSingle();
            var b = reader.ReadSingle();
            var a = reader.ReadSingle();

            return new Color(r, g, b, a);
        }

        static void WriteColor(BinaryWriter writer, Color color) {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        static List<string> ReadStringList(BinaryReader reader) {
            var count = reader.ReadInt32();
            var stringArray = new string[count];

            for(int i = 0; i < count; i++)
                stringArray[i] = reader.ReadString();

            return new List<string>(stringArray);
        }

        static void WriteStringList(BinaryWriter writer, List<string> list) {
            if(list != null) {
                writer.Write(list.Count);

                foreach(var value in list)
                    writer.Write(value);
            }
            else
                writer.Write(0);
        }
    }

    [System.Serializable]
    public struct Controls {
        public BaseMovementController.MovementType movementStyle;
    }

    [System.Serializable]
    public struct JetPivot {
        public string meshPath;
    }

    [System.Serializable]
    public struct RotatingObject {
        public string meshPath;
    }

    [System.Serializable]
    public struct WeaponPivot {
        public string meshPath;
    }
}