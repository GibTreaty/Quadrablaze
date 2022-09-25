using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OrbCreationExtensions;

public class TownCreator : MonoBehaviour {

	public TextAsset xmlFile;
	public GameObject wallPrototype;
	public GameObject roofPrototype;

	private List<GameObject> houses = new List<GameObject>();

	void Start () {
		SetupTownFromFile();

		// Here are some example that use an external file.
//		SetupTownFromServer("http://myserver.com/town.xml");  // from webserver
//		SetupTownFromServer("file:///path1/path2/town.xml");  // on mac
//		SetupTownFromServer("file://C:/path1/path2/town.xml");  // on windows (i believe)
	}

	void OnGUI() {
		GUI.Label(new Rect(2,2,400,400), "Very simple example of generating objects that are defined by XML files at runtime.\n\nIt uses 2 prototype of a house and a roof (ok, they are simple cubes in this example, but thats not the point).\n\nAn XML file is downloaded, the XML is parsed and gameobject are created and configured.\nHave a look in the file TownCreator.cs and Town.xml");
	}

	private void SetupTownFromFile() {
		// read xml file
		Hashtable townDefinition = SimpleXmlImporter.Import( xmlFile.text );
		// build town
		SetupTown(townDefinition);
	}

	private IEnumerator SetupTownFromServer(string url) {
		string xmlString = null;

		yield return StartCoroutine( DownloadFile ( url, retval => xmlString = retval) );

		if(xmlString!=null && xmlString.Length>0) {
			Hashtable townDefinition = SimpleXmlImporter.Import( xmlString );
			SetupTown( townDefinition );
		}
	}

	private void SetupTown(Hashtable townDefinition) {
		// get house definitions
		ArrayList houseDefinitions = townDefinition.GetArrayList( "town" );

		// process each house definition
		for(int i=0;i<houseDefinitions.Count;i++) {
			Hashtable houseDefinition = houseDefinitions.GetHashtable(i);

			Debug.Log("building house:" + houseDefinition.JsonString());

			// create house gameObject
			GameObject house = new GameObject( houseDefinition.GetString( "name" ) );

			// get elements
			ArrayList elementDefinitions = houseDefinition.GetArrayList( "archelements" );
			for(int j=0;j<elementDefinitions.Count;j++) {
				Hashtable elementDefinition = elementDefinitions.GetHashtable(j);
				GameObject prototype = null;
				if(elementDefinition.GetString("type") == "wall") {
					prototype = wallPrototype;
				} else if(elementDefinition.GetString("type") == "roof") {
					prototype = roofPrototype;
				}

				if(prototype != null) {
					// create element
					GameObject element = (GameObject)GameObject.Instantiate( prototype );
//					element.transform.SetParent( house.transform );
					element.transform.parent = house.transform;
					element.transform.localPosition = elementDefinition.GetVector3( "offset" );
					element.GetComponent<MeshRenderer>().material.color = elementDefinition.GetColor( "color" );
				}
			}

			// position the house
			house.transform.position = houseDefinition.GetVector3( "position" );

			// scale the house
			house.transform.localScale = houseDefinition.GetVector3( "scale" );

			// add to houses array for future access
			houses.Add( house );
		}
	}
	
	private IEnumerator DownloadFile(string url, System.Action<string> result) {
		Debug.Log("Downloading "+url);
        WWW www = new WWW(url);
        yield return www;
        if(www.error!=null) {
        	Debug.Log(www.error);
        } else {
        	Debug.Log("Downloaded "+www.bytesDownloaded+" bytes");
        }
       	result(www.text);
	}

}
