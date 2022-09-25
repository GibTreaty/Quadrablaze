using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "String List")]
public class StringList : ScriptableObject {

    [SerializeField, TextArea(true, true)]
    List<string> strings = new List<string>();

    #region Properties
    public int Count {
        get { return strings.Count; }
    }
    #endregion

    public void Add(string value) {
        strings.Add(value);
    }

    public bool Contains(string value) {
        return strings.Contains(value);
    }

    public string Get(int index) {
        return strings[index];
    }

    public string GetRandom() {
        return strings[Random.Range(0, Count)];
    }
    public string GetRandom(int seed) {
        return strings[new System.Random(seed).Next(0, Count)];
    }

    public void Insert(int index, string value) {
        strings.Insert(index, value);
    }

    public void Remove(string value) {
        strings.Remove(value);
    }

    public void RemoveAll(string value) {
        strings.RemoveAll(input => { return input == value; });
    }

    public void RemoveAt(int index) {
        strings.RemoveAt(index);
    }
}