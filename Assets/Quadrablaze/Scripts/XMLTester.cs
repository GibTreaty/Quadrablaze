using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;

public class XMLTester : MonoBehaviour {

    public TestStruct myTestStruct;
    public TestStruct deserializedTestStruct;

    [ContextMenu("Save")]
    void Save() {
        Serialize(myTestStruct);
    }

    [ContextMenu("Load")]
    void Load() {
        Deserialize(ref deserializedTestStruct);
    }

    void Serialize(TestStruct value) {
        XmlSerializer serializer = GetSerializer<TestStruct>();
        string folderPath = Application.persistentDataPath + "/XML Data";
        string filePath = folderPath + "/TestSerialization.xml";

        if(!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, value);
        writer.Close();

        Debug.Log(folderPath + "/TestSerialization.xml");
    }

    void Deserialize(ref TestStruct value) {
        XmlSerializer serializer = GetSerializer<TestStruct>();
        string folderPath = Application.persistentDataPath + "/XML Data";
        string filePath = folderPath + "/TestSerialization.xml";

        if(!File.Exists(filePath)) return;

        var streamReader = new StreamReader(filePath);

        try {
            deserializedTestStruct = (TestStruct)serializer.Deserialize(streamReader);
        }
        catch(Exception e) {
            Debug.LogError(e.Message, this);
        }
        finally {
            streamReader.Close();
        }
    }

    XmlSerializer GetSerializer<T>() {
        return new XmlSerializer(typeof(T));
    }
}

[System.Serializable]
public struct TestStruct {

    [SerializeField]
    bool myPrivateBool;

    public int myInt;
    public bool myBool;

    public float MyFloatProperty { get; set; }

    public int MyIntProperty { get { return myInt; } set { myInt = value; } }

    public List<TestStruct> myList;

    public int newInt;
}