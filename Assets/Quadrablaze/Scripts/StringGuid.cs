using UnityEngine;

[System.Serializable]
public struct StringGuid {
    [SerializeField]
    private string m_storage;

    public static implicit operator StringGuid(System.Guid rhs) {
        return new StringGuid { m_storage = rhs.ToString("D") };
    }

    public static implicit operator System.Guid(StringGuid rhs) {
        if(rhs.m_storage == null) return System.Guid.Empty;
        try {
            return new System.Guid(rhs.m_storage);
        }
        catch(System.FormatException) {
            return System.Guid.Empty;
        }
    }
}