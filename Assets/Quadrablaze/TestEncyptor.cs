using UnityEngine;
using Quadrablaze;
using System.Text;

public class TestEncyptor : MonoBehaviour {

    public CantTouchThis what;

    public string inString;
    public string encryptedString;
    public string outString;

    public byte[] inBytes;
    public byte[] encryptedBytes;
    public byte[] outBytes;

    void Awake() {
        var stringBytes = Encoding.ASCII.GetBytes(inString);
        var encryptedStringBytes = what.Encrypt(stringBytes);

        encryptedString = Encoding.ASCII.GetString(encryptedStringBytes);
        stringBytes = what.Decrypt(encryptedStringBytes);
        outString = Encoding.ASCII.GetString(stringBytes);

        encryptedBytes = what.Encrypt(inBytes);
        outBytes = what.Decrypt(encryptedBytes);
    }
}