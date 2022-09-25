/* SimpleXML 2.0                        */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* March 31, 2015                       */
/* games, components and freelance work */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OrbCreationExtensions;

public class SimpleXmlImporter {

	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- Public Import interface ----------------------------- */
	// simple import of the full xml string
	// importing case insensitive will turn all tags into lowercase
	public static Hashtable Import(string xml, bool caseInsensitive = false) {
		return Import(xml, null, caseInsensitive);
	}

	// Only import a specific tag
	public static Hashtable Import(string xml, string tagName, bool caseInsensitive = false) {
		int end = xml.Length;
		int idx = FindEndOfXmlIdentifier(xml);

		if(tagName != null && tagName.Length > 0) {
			idx = xml.IndexOf("<"+tagName, idx);
			if(idx >= 0) {
				end = xml.LastIndexOf("</"+tagName+">");
				if(end >= 0) end += 4 + tagName.Length;
			}
			if(end < idx) {
				end = xml.IndexOf("/>", idx);
				if(end >= 0) end += 3;
			}
			if(idx < 0 || end < idx) {
//				Debug.Log("Tag "+tagName+" not found");
				return null;
			}
		}

		ArrayList result = new ArrayList();
		int line = 1;
		while(idx<end) {
			string key = "";
			int idxPrev = idx;
			ArrayList node = ReadNode(xml, out key, ref idx, end, ref line, caseInsensitive);
			SetPropertyValue(result, "SimpleXmlImport", key, node, caseInsensitive);
			if(idx <= idxPrev) {
				Debug.LogWarning("SimpleXmlImporter: empty node at line "+line);
				break; // little safety precaution against infinite loops
			}
		}
		if(result.Count <= 1) {
//			Debug.Log(result.GetHashtable(0).JsonString());
			return result.GetHashtable(0);
		}
		Hashtable wrapper = new Hashtable();
		if(tagName == null || tagName.Length == 0) tagName = "SimpleXmlImport";
		wrapper[tagName] = result;
		Debug.Log(wrapper.JsonString());
		return wrapper;
	}


	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- private Import functions ---------------------------- */
	private static ArrayList ReadNode(string xml, out string nodeKey, ref int begin, int end, ref int line, bool caseInsensitive) {
		nodeKey = "";
		ArrayList node = new ArrayList();
		string propertyKey = "";
		string propertyValue = "";
		int idx = begin;
		bool isReadingNodeKey = false;
		bool isReadingValue = false;
		bool isReadingPropertyKey = false;
		bool isReadingPropertyValue = false;
		bool isNodeClosed = false;
		bool inQuotes = false;
		string ignoreChars = "\r\n\t ";
		int startLine = line;
		char c = '\0';

		while(idx<end) {
			char cPrev = c;
			char cNext = '\0';
			c = xml[idx++];
			if(idx < end) cNext = xml[idx];
			if(c == '\n') line++;

			if(inQuotes) {
				if(c == '"' && cPrev != '\\') {
					inQuotes = false;
					if(isReadingValue || isReadingPropertyValue) {
						isReadingValue = false;
						isReadingPropertyValue = false;
						isReadingPropertyKey = true;
						SetPropertyValue(node, nodeKey, propertyKey, propertyValue, caseInsensitive);
						propertyKey = "";
						propertyValue = "";
					}
				} else if(isReadingValue || isReadingPropertyValue) {
					propertyValue = propertyValue + c;
				}
				continue;
			} else if(c == '"' && cPrev != '\\') {
				if(isReadingValue || isReadingPropertyValue) propertyValue = "";
				inQuotes = true;
				continue;
			}

			// Skip comments by moving idx to the end of the comment
			if(c=='<' && cNext == '!' && idx<(end-2) && xml[idx+1] == '-' && xml[idx+2] == '-') {
				int endComment = xml.IndexOf("-->",idx);
				if(endComment>idx) {
					idx = endComment+3;
				} else {
					Debug.Log("XmlImporter: Comment without end at " + idx);
					idx = end;
				}
				continue;
			}				
			if((!isReadingValue) && ignoreChars.IndexOf(c) >= 0) {
				if(isReadingNodeKey) {
					isReadingNodeKey = false;
					isReadingPropertyKey = true;
				}
				if(isReadingPropertyValue && propertyValue.Length>0) {
					SetPropertyValue(node, nodeKey, propertyKey, propertyValue, caseInsensitive);
					propertyKey = "";
					propertyValue = "";
					isReadingPropertyValue = false;
					isReadingPropertyKey = true;
				}
				continue;
			}

			if(c == '<' && cNext == '/') {
				inQuotes = false;
				SetPropertyValue(node, nodeKey, "", propertyValue, caseInsensitive);
				if(idx < end - nodeKey.Length - 1 && xml.Substring(idx + 1, nodeKey.Length + 1).ToLower() == nodeKey.ToLower() + ">") {
					begin = idx + 3 + nodeKey.Length;
					isNodeClosed = true;
					break;
				} else {
					int i = xml.IndexOf(">", idx, Mathf.Min(80, xml.Length - idx));
					if(i>=idx) begin = i+1; // probably a typo in the closing tag, pr0ceed after the tag
					break;
				}
			} 
			if(isReadingValue || isReadingPropertyValue) {
				if(c == '<') {
					if(cNext != '/') {
						inQuotes = false;
						idx--;
						int idxPrev = idx;
						ArrayList detailNode = ReadNode(xml, out propertyKey, ref idx, end, ref line, caseInsensitive);
						SetPropertyValue(node, nodeKey, propertyKey, detailNode, caseInsensitive);
						if(idxPrev < idx && idx < end && xml[idx] != '>') idx--;
						else {
							Debug.LogWarning("XmlImporter: Empty node at line " + line);
							break; // little safety precaution against infinite loops
						}
					}
				} else if(c=='>') {
					if(propertyValue.Length>0) {
						SetPropertyValue(node, nodeKey, propertyKey, propertyValue, caseInsensitive);
						propertyKey = "";
						propertyValue = "";
					}
					isReadingPropertyValue = false;
					isReadingPropertyKey = true;
				} else {
					propertyValue = propertyValue + c;
				}
			}

			if(c=='/' && cNext == '>') {
				inQuotes = false;
				begin = idx + 2;
				isNodeClosed = true;
				break;
			} else if(c == '>') {
				inQuotes = false;
				isReadingNodeKey = false;
				isReadingValue = true;
				bool valueFastRead = false;
				for(int i = idx;i<end;i++) {
					if(ignoreChars.IndexOf(xml[i]) < 0) {
						if(xml[i] != '<') {
							i = xml.IndexOf("<", i);
							propertyValue = xml.Substring(idx, i-idx);
							idx = i;
							valueFastRead = true;
						}
						break;
					}
				}
				if(valueFastRead) continue;
			} else if(isReadingNodeKey) {
				startLine = line;
				nodeKey = nodeKey + c;
			} else if(c == '<' && cNext != '/' && (nodeKey == null || nodeKey.Length <= 0)) {
				isReadingNodeKey = true;
			} else if(isReadingPropertyKey) {
				if(c == '=') {
					isReadingPropertyKey = false;
					isReadingPropertyValue = true;
				} else {
					propertyKey = propertyKey + c;
				}
			}
		}
		if(!isNodeClosed) {
			Debug.Log("XmlImporter: Node "+nodeKey+" at line "+ startLine + " not closed");
		}
		return node;
	}

	private static void SetPropertyValue(ArrayList parentNode, string parentKey, string key, string value, bool caseInsensitive) {
		if(value == null) return;
		key = TrimPropertyValue(key);
		if(caseInsensitive) key = key.ToLower();
		value = TrimPropertyValue(value).XmlDecode();
		if(key.Length <= 0 && value.Length <= 0) return; // no key and no value, skip this
		Hashtable parentFirstNode = parentNode.GetHashtable(0);
		if(parentFirstNode == null) {
			parentFirstNode = new Hashtable();
			parentNode.Add(parentFirstNode);
		}
		if(key.Length > 0 && parentFirstNode.ContainsKey(key)) {
			if(parentFirstNode[key].GetType() == typeof(ArrayList)) {
				Hashtable newNode = new Hashtable();
				newNode[key] = value;
				((ArrayList)parentFirstNode[key]).Add(newNode);
			} else if(parentFirstNode.Count == 1) {  // the only key in the parent
				Hashtable newNode = new Hashtable();
				newNode[key] = value;
				parentNode.Add(newNode);
			} else {
				ArrayList newArray = new ArrayList();
				Hashtable newNode = new Hashtable();
				newNode[key] = parentFirstNode[key];
				newArray.Add(newNode);
				newNode = new Hashtable();
				newNode[key] = value;
				newArray.Add(newNode);
				parentFirstNode[key] = newArray;
			}
		} else if(key.Length > 0) {
			parentFirstNode[key] = value;  // add the string value
		} else if(value.Length > 0) { // add a key
			if(parentFirstNode.Count == 0) key = ".value.";
			else key = parentKey;
			parentFirstNode[key] = value;
		}
	}

	private static void SetPropertyValue(ArrayList parentNode, string parentKey, string key, ArrayList value, bool caseInsensitive) {
		if(value == null || value.Count == 0) return;
		if(value.Count == 1) {
			Hashtable node = value.GetHashtable(0);
			if(node.Count == 1 && node.ContainsKey(".value.")) {
				if(key.Length <= 0) key = parentKey;
				SetPropertyValue(parentNode, parentKey, key, node.GetString(".value."), caseInsensitive);
			} else {
				Hashtable parentFirstNode = parentNode.GetHashtable(0);
				if(parentFirstNode == null) {
					parentFirstNode = new Hashtable();
					parentNode.Add(parentFirstNode);
				}
				if(parentFirstNode.ContainsKey(key)) {
					if(parentFirstNode[key].GetType() == typeof(ArrayList)) {
						ArrayList firstNodeList = (ArrayList)parentFirstNode[key];
						for(int i=0;i<firstNodeList.Count;i++) {
							if(firstNodeList[i].GetType() == typeof(Hashtable)) {
								Hashtable hash = (Hashtable)firstNodeList[i];
								if(!hash.ContainsKey(".tag.")) hash[".tag."] = key;
							}
						}
						node[".tag."] = key;
						((ArrayList)parentFirstNode[key]).Add(node);
					} else if(parentFirstNode[key].GetType() == typeof(Hashtable) && parentFirstNode.Count == 1) {
						parentNode[0] = parentFirstNode[key];
						((Hashtable)parentNode[0])[".tag."] = key;
						parentFirstNode = parentNode.GetHashtable(0);
						node[".tag."] = key;
						parentNode.Add(node);
					} else if(parentFirstNode[key].GetType() == typeof(Hashtable)) {
						ArrayList newArray = new ArrayList();
						((Hashtable)parentFirstNode[key])[".tag."] = key;
						newArray.Add(parentFirstNode[key]);
						node[".tag."] = key;
						newArray.Add(node);
						parentFirstNode[key] = newArray;
					}
				} else if(parentFirstNode.GetString(".tag.") == key) {
					node[".tag."] = key;
					parentNode.Add(node);
				} else {
					if(parentNode.Count > 1 && parentFirstNode.ContainsKey(".tag.")) {
						ArrayList newList = new ArrayList();
						string tag = parentFirstNode.GetString(".tag.");
						for(int i = parentNode.Count-1;i>=0;i--) {
							Hashtable oldNode = parentNode.GetHashtable(i);
							if(oldNode.GetString(".tag.") == tag) {
								newList.Insert(0, parentNode[i]);
								parentNode.RemoveAt(i);
							}
						}
						Hashtable newHash = new Hashtable();
						newHash[tag] = newList;
						if(value.Count > 1)	newHash[key] = value;
						else newHash[key] = value[0];
						parentNode.Insert(0, newHash);
					} else {
						parentFirstNode[key] = node;
					}
				}
			}
		} else {
			Hashtable node = parentNode.GetHashtable(0);
			if(node == null) {
				node = new Hashtable();
				parentNode.Add(node);
			}
			if(node.ContainsKey(key)) {
				node = new Hashtable();
				parentNode.Add(node);
			}
			node[key] = value;
		}
	}

	private static int FindEndOfXmlIdentifier(string xml) {
		int i = xml.IndexOf("<?xml ");  // get rid of the xml identifier
		if(i>=0) i = xml.IndexOf("?>", i);
		if(i<0) i=0;
		else i+=2;
		return i;
	}

	// remove all leading and trailing <space>, <tab>, <newline>, <return> and <doublequote>
	private static string TrimPropertyValue(string str) {
		str = str.Trim();
		while(str.Length>2 && (str[0]=='\r' || str[0]=='\n' || str[0]=='\t' || str[0]==' ')) str = str.Substring(1,str.Length-1);
		while(str.Length>1 && (str[str.Length-1]=='\r' || str[str.Length-1]=='\n' || str[str.Length-1]=='\t' || str[str.Length-1]==' ')) str = str.Substring(0,str.Length-1);
		if(str != null && str.Length>=2 && str[0] == '"' && str[str.Length-1] == '"') {
			return str.Substring(1,str.Length-2);
		}
		return str;
	}

	private static string ToString(char c) {
		if(c == '\r') return "\\r";
		if(c == '\n') return "\\n";
		if(c == '\t') return "\\t";
		if(c == '\0') return "\\0";
		return "" + c;
	}
}
