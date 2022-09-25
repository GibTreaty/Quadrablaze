using System.IO;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(fileName = "CantTouchThis", menuName = "Encryption/Encryption Key")]
public class CantTouchThis : ScriptableObject {

    [SerializeField]
    string _key = "";

    [SerializeField]
    byte[] _salt = new byte[0];


    #region Properties
    public string Key {
        get { return _key; }
        set { _key = value; }
    }

    public byte[] Salt {
        get { return _salt; }
        set { _salt = value; }
    }
    #endregion

    public byte[] Encrypt(byte[] toEncrypt) {
        using(var memory = new MemoryStream())
        using(var aesProvider = GetAlgorithm())
        using(var crypto = new CryptoStream(memory, aesProvider.CreateEncryptor(), CryptoStreamMode.Write)) {
            crypto.Write(toEncrypt, 0, toEncrypt.Length);
            crypto.Close();

            return memory.ToArray();
        }
    }

    public byte[] Decrypt(byte[] toDecrypt) {
        using(var memory = new MemoryStream())
        using(var aesProvider = GetAlgorithm())
        using(var crypto = new CryptoStream(memory, aesProvider.CreateDecryptor(), CryptoStreamMode.Write)) {
            crypto.Write(toDecrypt, 0, toDecrypt.Length);
            crypto.Close();

            return memory.ToArray();
        }
    }

    SymmetricAlgorithm GetAlgorithm() {
        var algorithm = Rijndael.Create();
        var rdb = new Rfc2898DeriveBytes(Key, Salt);

        algorithm.Padding = PaddingMode.ISO10126;
        algorithm.Key = rdb.GetBytes(32);
        algorithm.IV = rdb.GetBytes(16);

        return algorithm;
    }
}