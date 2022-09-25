using UnityEngine;

[CreateAssetMenu(fileName = "Json Test File", menuName = "Json Test File")]
public class JsonSerializationTest : ScriptableObject {

    public TestStruct testStruct;

    [TextArea(false, false)]
    public string outputString;

}