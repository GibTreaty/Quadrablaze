/* SimpleCollada 1.2a                   */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* June 15, 2015                        */
/* games, components and freelance work */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OrbCreationExtensions;

public class DemoCtrl : MonoBehaviour {
//	private string pathSample1 = "file:///ontwikkel/AssetStore/test.xml";
	private string pathSample1 = "http://orbcreation.com/SimpleXml/Books.xml";
	private string pathSample2 = "http://orbcreation.com/SimpleXml/CdCatalog.xml";
	private string pathSample3 = "http://orbcreation.com/SimpleXml/Collada.xml";
	private string downloadedPath = "";
	private string url;
	private string tagName = "";
	private string[] path = new string[0];
	private string findKey = "";
	private string findValue = "";
	private Vector2 scrollPosition = Vector2.zero;
	private Vector2 scrollPositionLog = Vector2.zero;
	private string logMsgs = "";
	private string xmlString;

	private string xmlLogString="";
	private Hashtable xmlNode=null;
	private object selectedNode=null;

	public Texture2D bg;
	public bool caseInsensitive = false;

	private int screenshotCounter = 0;

	/* ------------------------------------------------------------------------------------- */
	/* ----------------------- Using the SimpleXml functions ------------------------------- */

	private Hashtable ImportXmlString(string aString) {
		AddToLog("Importing Xml string");
		return SimpleXmlImporter.Import(aString, caseInsensitive);
	}

	private Hashtable ImportFirstOccurenceOfTagFromXmlString(string aString, string aTagName) {
		AddToLog("Importing " + aTagName+ " in Xml string");
		return SimpleXmlImporter.Import(aString, aTagName, caseInsensitive);
	}

	private string ExportXml(Hashtable aHash) {
		AddToLog("Exporting Xml string");
		string xmlExportString = aHash.XmlString();
		ExportToFile(xmlExportString, "SimpleXmlExport.xml");
		return xmlExportString;
	}

	private void FindNodesWithTagPath(string[] aPath) {
		AddToLog("Looking for first node at path "+ PathToString(aPath));
		selectedNode = xmlNode.GetNodeAtPath(path);

		// the returned object can be of any type
		// only Hashtables and ArrayLists can be converted to a nice readable JSON format
		// so we have to test for the type first
		if(selectedNode==null) {
			AddToLog("Nothing found");
			xmlLogString= "";
		} else if(selectedNode.GetType() == typeof(Hashtable)) {
			AddToLog("Found Hashtable");
			xmlLogString = ((Hashtable)selectedNode).JsonString();;
		} else if(selectedNode.GetType() == typeof(ArrayList)) {
			AddToLog("Found ArrayList");
			xmlLogString = ((ArrayList)selectedNode).JsonString();
		} else {
			AddToLog("Found value");
			xmlLogString = ""+selectedNode;
		}
	}

	private void FindNodesWithProperty(string aKey, string aValue) {
		AddToLog("Looking for first node with \""+aKey + "\" = \"" + aValue + "\"");
		selectedNode = xmlNode.GetNodeWithProperty(findKey, findValue);
		if(selectedNode==null) {
			AddToLog("Nothing found");
		} else {
			AddToLog("Found Hashtable");
		}
		// GetNodeWithProperty always returns a Hashtable or null
		// so it is safe to cast it here
		xmlLogString = CreateXmlLogString((Hashtable)selectedNode);
	}

	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ---------------------------------- GUI stuff (not pretty) --------------------------- */

	void Start() {
		url = pathSample1;
	}

//	void Update() {
//		if(Input.GetKeyDown(KeyCode.P)) StartCoroutine(Screenshot());
//	}

	void OnGUI() {
		float margin = 4f;
		float inputLineHeight = 25f;
		float x = margin;
		float y = margin;
		float xColumn2=x;
		float xColumn3=x;
		float xColumn4=x;
		float w = 160f;

		GUI.skin.label.normal.textColor = Color.black;
		GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height), bg);

		GUI.Label(new Rect(x,y,w,inputLineHeight), "Download XML file:");
		x+=w+margin;
		xColumn2 = x;

		w=300f;
		string oldValue = url;
		url = GUI.TextField(new Rect(x,y,w,inputLineHeight), url);
		if(url!=oldValue) {
			xmlNode=null;
			Reset();
		}
		x+=w+margin;
		xColumn3 = x;

		w=82;
		x=xColumn3;
		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Example 1")) {
			xmlNode=null;
			Reset();
			url = pathSample1;
		}
		x+=w+margin;
		w=30;
		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "2")) {
			xmlNode=null;
			Reset();
			url = pathSample2;
		}
		x+=w+margin;
		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "3")) {
			xmlNode=null;
			Reset();
			url = pathSample3;
		}
		y += inputLineHeight;
		xColumn4 = x+w+margin;

		if(downloadedPath != url) {
			x=xColumn3;
			w=150;
			if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Download file")) {
				xmlString = null;
				Reset();
				downloadedPath = url;
				StartCoroutine(DownloadXmlFile(url));
			}
			y += inputLineHeight;
		}



		if(url==pathSample3 && xmlString != null) {
			w = 160f;
			x = margin;
			y += inputLineHeight;
			GUI.Label(new Rect(x,y,w,inputLineHeight), "Only import tag:");

			x=xColumn2;
			w=300f;
			tagName = GUI.TextField(new Rect(x,y,w,inputLineHeight), tagName);
			x+=w+margin;
			w=73;
			if(GUI.Button(new Rect(x,y,w,inputLineHeight), "cameras")) {
				tagName = "library_cameras";
			}
			x+=w+margin;
			if(GUI.Button(new Rect(x,y,w,inputLineHeight), "geometry")) {
				tagName = "library_geometries";
			}
			y += inputLineHeight;
		} else tagName="";

		x=xColumn3;
		w=150;
		if(downloadedPath == url && xmlString != null && xmlString.Length > 0 && xmlNode == null) {
			y+=10;
			if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Import XML")) {
				if(tagName.Length>0) {
					xmlNode = ImportFirstOccurenceOfTagFromXmlString(xmlString, tagName);
					AddToLog("Xml imported for tag "+tagName);
				} else {
					xmlNode = ImportXmlString(xmlString);
				}

				if(xmlNode==null) {
					AddToLog("Nothing imported");
					xmlLogString = "";
				} else {
					AddToLog("Done importing");
					xmlLogString = CreateXmlLogString(xmlNode);
				}
			}
			y += inputLineHeight;
		}

		if(xmlNode!=null) {
			if(selectedNode == null || selectedNode.GetType() == typeof(Hashtable)) {
				y += 10;
				if(GUI.Button(new Rect(xColumn3,y,w,inputLineHeight), "Export XML")) {
					if(selectedNode != null && selectedNode.GetType() == typeof(Hashtable)) {
						xmlLogString = TruncateStringForEditor(ExportXml((Hashtable)selectedNode));
					} else {
						xmlLogString = TruncateStringForEditor(ExportXml(xmlNode));
					}
				}
				y += inputLineHeight;
			}

			w = 160f;
			x = margin;
			y += inputLineHeight;
			GUI.Label(new Rect(x,y,w,inputLineHeight), "Find by tag :");
			x = xColumn2;
			w=194f;
			int pathIdx = 0;
			if(path==null || path.Length <= 0) path = new string[0];

			if(url==pathSample1 || url==pathSample2 || url==pathSample3) {
				if(GUI.Button(new Rect(x+w+margin,y,75,inputLineHeight), "Example 1")) {
					if(url==pathSample1) {
						path = new string[2] {"catalog", "book"};
					} else if(url==pathSample2) {
						if(caseInsensitive) path = new string[2] {"catalog", "cd"};
						else path = new string[2] {"CATALOG", "CD"};
					} else if(url==pathSample3) {
						if(caseInsensitive) path = new string[3] {"collada", "library_cameras", "camera"};
						else path = new string[3] {"COLLADA", "library_cameras", "camera"};
					}
				}
			}
			if(url==pathSample3) {
				if(GUI.Button(new Rect(x+w+margin+75+margin,y,22,inputLineHeight), "2")) {
					if(caseInsensitive) path = new string[4] {"collada", "library_geometries", "geometry", "mesh"};
					else path = new string[4] {"COLLADA", "library_geometries", "geometry", "mesh"};
				}
			}

			for(pathIdx=0; pathIdx<path.Length && path[pathIdx].Length>0; pathIdx++) {
				path[pathIdx] = GUI.TextField(new Rect(x+(pathIdx * 10),y,w,inputLineHeight), path[pathIdx]);
				y += inputLineHeight;
			}
			string newPathEntry="";
			if(path.Length<10) {
				newPathEntry = GUI.TextField(new Rect(x+(pathIdx * 10),y,w,inputLineHeight), "");
				if(newPathEntry.Length>0) pathIdx++;
			}
			if(pathIdx!=path.Length) {
				string[] newPath = new string[pathIdx];
        		Array.Copy(path, newPath, Mathf.Min(path.Length, pathIdx));
        		if(newPathEntry.Length>0) newPath[pathIdx-1] = newPathEntry;
        		path = newPath;
			}
			x=xColumn3;
			w=150;
			if(path.Length>0) {
				if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Find 1st occurence")) {
					FindNodesWithTagPath(path);
					findKey = "";
					findValue = "";
				}
			}

			w = 160f;
			x = margin;
			y += inputLineHeight;
			y += inputLineHeight;
			GUI.Label(new Rect(x,y,w,inputLineHeight), "Find by property:");
			x = xColumn2;
			w=90f;
			findKey = GUI.TextField(new Rect(x,y,w,inputLineHeight), findKey);
			x+=w;
			GUI.Label(new Rect(x,y,15,inputLineHeight), " =");
			x+=15;
			findValue = GUI.TextField(new Rect(x,y,w,inputLineHeight), findValue);

			if(url==pathSample1 || url==pathSample2 || url==pathSample3) {
				if(GUI.Button(new Rect(x+w+margin,y,75,inputLineHeight), "Example 1")) {
					if(url==pathSample1) {
						findKey = "author";
						findValue = "Corets, Eva";
					} else if(url==pathSample2) {
						if(caseInsensitive) findKey = "year";
						else findKey = "YEAR";
						findValue = "1997";
					} else if(url==pathSample3) {
						findKey = "type";
						findValue = "NODE";
					}
				}
				if(GUI.Button(new Rect(x+w+margin+75+margin,y,22,inputLineHeight), "2")) {
					if(url==pathSample1) {
						findKey = "price";
						findValue = "36.95";
					} else if(url==pathSample2) {
						findKey = "COMPANY";
						findValue = "Polydor";
					} else if(url==pathSample3) {
						findKey = "sid";
						findValue = "location";
					}
				}
			}
			w=150f;
			x=xColumn3;
			if(findKey.Length>0 && findValue.Length>0) {
				if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Find 1st occurence")) {
					path=null;
					FindNodesWithProperty(findKey, findValue);
				}
			}

			y += inputLineHeight;
			if(GUI.Button(new Rect(xColumn3,y,w,inputLineHeight), "Reset")) Reset();
			y += inputLineHeight;
		}

		y += margin;
		x = margin;
		w = xColumn4-margin-x;
		float h = GUI.skin.label.CalcHeight(new GUIContent(logMsgs), w-25);
        scrollPositionLog = GUI.BeginScrollView(new Rect(x, y, w, Screen.height-y-margin), scrollPositionLog, new Rect(0, 0, w-25, h));
        GUI.Label(new Rect(0, 0, w-25, h), logMsgs);
        GUI.EndScrollView();

		x = xColumn4;
		y = margin;
		w = Mathf.Clamp(Screen.width-xColumn4-margin, 100, Screen.width);
		h = GUI.skin.label.CalcHeight(new GUIContent(xmlLogString), w-25);
        scrollPosition = GUI.BeginScrollView(new Rect(x, y, w, Screen.height-y-margin), scrollPosition, new Rect(0, 0, w-25, h));
        GUI.Label(new Rect(0, 0, w-25, h), xmlLogString);
        GUI.EndScrollView();

	}

	private void Reset() {
		xmlLogString = "";
		if(xmlNode!=null) xmlLogString = CreateXmlLogString(xmlNode);
		path = new string[0];
		findKey = "";
		findValue = "";
		selectedNode = null;
	}

	// To make the screenshots used for the Asset Store submission
	private IEnumerator Screenshot() {
		yield return new WaitForEndOfFrame(); // wait for end of frame to include GUI

		Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenshot.Apply(false);

		if(Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.WindowsPlayer && Application.platform!=RuntimePlatform.LinuxPlayer || Application.isEditor) {
			byte[] bytes = screenshot.EncodeToPNG();
			FileStream fs = new FileStream("Screenshot"+screenshotCounter+".png", FileMode.OpenOrCreate);
			BinaryWriter w = new BinaryWriter(fs);
			w.Write(bytes);
			w.Close();
			fs.Close();
		}
		screenshotCounter++;

	}
	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- Downloading files  ---------------------------------- */

	private IEnumerator DownloadXmlFile(string url) {
		xmlString = null;
		yield return StartCoroutine(DownloadFile(url, fileContents => xmlString = fileContents));
		xmlLogString = TruncateStringForEditor(xmlString);
	}

	private IEnumerator DownloadFile(string url, System.Action<string> result) {
		AddToLog("Downloading "+url);
        WWW www = new WWW(url);
        yield return www;
        if(www.error!=null) {
        	AddToLog(www.error);
        } else {
        	AddToLog("Downloaded "+www.bytesDownloaded+" bytes");
        }
       	result(www.text);
	}

	private void ExportToFile(string exportString, string path) {
		if(exportString == null || path == null) return;
		if(Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.WindowsPlayer && Application.platform!=RuntimePlatform.LinuxPlayer || Application.isEditor) {
			// Put a proper prolog in the file
			exportString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + exportString;

			// We use UTF-8 encoding
	        byte[] bytes = new UTF8Encoding(true).GetBytes(exportString);
	        if(File.Exists(path)) File.Delete(path); // delete if it exists
 			FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
			BinaryWriter w = new BinaryWriter(fs);
			w.Write(bytes);
			w.Close();
			fs.Close();
        }
    }


	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- Logging functions  ---------------------------------- */

	private void AddToLog(string msg) {
		Debug.Log(msg+"\n"+DateTime.Now.ToString("yyy/MM/dd hh:mm:ss.fff"));

		// for some silly reason the Editor will generate errors if the string is too long
		int lenNeeded = msg.Length + 1;
		if(logMsgs.Length + lenNeeded>4096) logMsgs = logMsgs.Substring(0,4096-lenNeeded);

		logMsgs = logMsgs + "\n" + msg;
	}
    private string PathToString(string[] aPath) {
   		string str = "{";
   		for(int i=0;i<aPath.Length;i++) {
   			str = str + aPath[i];
   			if(i<aPath.Length-1) str = str + ", ";
   		}
   		str = str + "}";
   		return str;
    }

 	private string CreateXmlLogString(Hashtable aNode) {
 		if(aNode == null) return "";
 		string aStr = aNode.JsonString().Replace("\t", "  ");
		return TruncateStringForEditor(aStr);
	}

	private string CreateXmlLogString(ArrayList aNodeArray) {
 		if(aNodeArray == null) return "";
 		string aStr = aNodeArray.JsonString().Replace("\t", "  ");
		return TruncateStringForEditor(aStr);
	}

    private string TruncateStringForEditor(string str) {
    	// for some silly reason the Editor will generate errors if the string is too long
		if(str.Length>4096) str = str.Substring(0,4000)+"\n .... display truncated ....\n";
		return str;
    }
	/* ------------------------------------------------------------------------------------- */

}
