/* SimpleCollada 1.4                    */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* Dec 3, 2015                          */
/* games, components and freelance work */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using OrbCreationExtensions;

public class ColladaException : ApplicationException {
    public ColladaException(string Message,
                 Exception innerException) : base(Message, innerException) { }
    public ColladaException(string Message) : base(Message) { }
    public ColladaException() { }
}

public class ColladaImporter {

    public static GameObject Import(string colladaString, bool includeEmptyNodes = false, string colliderName = "") {
        return Import(colladaString, Quaternion.identity, new Vector3(1, 1, 1), Vector3.zero, includeEmptyNodes, colliderName);
    }

    public static GameObject Import(string colladaString, Quaternion rotate, Vector3 scale, Vector3 translate, bool includeEmptyNodes = false, string colliderName = "") {
        GameObject importedGameObject = null;
        Quaternion upCompensation = Quaternion.identity;

        Hashtable assetInfo = SimpleXmlImporter.Import(colladaString, "asset");
        if(assetInfo != null) {
            Hashtable asset = assetInfo.GetHashtable("asset");
            if(asset != null && asset.GetString("up_axis") == "Z_UP") {
                upCompensation = Quaternion.Euler(new Vector3(-90, 0, 0));
                //rotate = rotate * Quaternion.Euler(new Vector3(-90, 0, 0));
            }
            if(asset != null && asset.ContainsKey("unit")) {
                Hashtable unit = asset.GetHashtable("unit");
                if(unit != null && unit.ContainsKey("meter")) {
                    float meter = unit.GetFloat("meter");
                    if(meter > 0f) scale *= meter;
                }
            }
        }

        // Get the material properties...
        ArrayList library_effects = null;
        Hashtable collada = SimpleXmlImporter.Import(colladaString, "library_effects");
        if(collada != null) library_effects = collada.GetArrayList("library_effects", true);

        ArrayList library_materials = null;
        collada = SimpleXmlImporter.Import(colladaString, "library_materials");
        if(collada != null) library_materials = collada.GetArrayList("library_materials", true);

        ArrayList library_images = null;
        collada = SimpleXmlImporter.Import(colladaString, "library_images");
        if(collada != null) library_images = collada.GetArrayList("library_images", true);

        // Hashtable collada = SimpleXmlImporter.Import(colladaString, "library_geometries");
        collada = SimpleXmlImporter.Import(colladaString, "library_geometries");
        if(collada == null) {
            Debug.LogWarning("path COLLADA / library_geometries not found in source");
            throw new ColladaException("path COLLADA / library_geometries not found in source");
        }

        ArrayList library_geometries = collada.GetArrayList("library_geometries", true);
        if(library_geometries == null || library_geometries.Count == 0) {
            Debug.LogWarning("path COLLADA / library_geometries not found in source");
            throw new ColladaException("path COLLADA / library_geometries not found in source");
        }

        collada = SimpleXmlImporter.Import(colladaString, "library_nodes");
        ArrayList library_nodes = null;
        if(collada != null) library_nodes = collada.GetArrayList("library_nodes", true);

        ArrayList library_scenes = SimpleXmlImporter.Import(colladaString, "library_visual_scenes").GetArrayList("library_visual_scenes", true);

        importedGameObject = new GameObject("ImportedColladaScene");
        importedGameObject.transform.position = Vector3.zero;
        importedGameObject.transform.localScale = Vector3.one;
        importedGameObject.transform.rotation = Quaternion.identity;

        for(int k = 0; library_scenes != null && k < library_scenes.Count; k++) {
            Hashtable scene = library_scenes.GetHashtable(k);
            if(scene != null) {
                ArrayList visual_scenes = scene.GetArrayList("visual_scene", true);
                for(int l = 0; visual_scenes != null && l < visual_scenes.Count; l++) {
                    Hashtable visual_scene = visual_scenes.GetHashtable(l);
                    if(visual_scene != null) {
                        AppendGameObjectsFromNodes(importedGameObject, visual_scene.GetArrayList("node", true), library_effects, library_materials, library_images, library_geometries, library_nodes, includeEmptyNodes, colliderName, upCompensation);
                    }
                }
            }
        }

        foreach(var meshFilter in importedGameObject.GetComponentsInChildren<MeshFilter>()) {
            Quaternion rot = upCompensation;
            //int depth = GetChildDepth(meshFilter.transform);

            //if(depth > 1) {
            //    Debug.Log(string.Format("Name:{0} Depth:{1}", meshFilter.name, depth, meshFilter));
            //    rot.eulerAngles *= (depth + 2);
            //}

            if(meshFilter.transform.parent == meshFilter.transform.root)
                meshFilter.mesh = meshFilter.mesh.ScaledRotatedTranslatedMesh(Vector3.one, rot, Vector3.zero);

            //if(!meshFilter.transform.parent == meshFilter.transform.root) {
            //    meshFilter.transform.rotation *= Quaternion.Inverse(rot);
            //}
        }

        //MeshFilter[] meshFilters = importedGameObject.GetComponentsInChildren<MeshFilter>();
        //if(meshFilters.Length == 1) {
        //    GameObject emptyObject = importedGameObject;
        //    importedGameObject = meshFilters[0].gameObject;
        //    importedGameObject.transform.parent = null;
        //    GameObject.Destroy(emptyObject);
        //    meshFilters[0].mesh = meshFilters[0].mesh.ScaledRotatedTranslatedMesh(scale, rotate, translate);
        //}
        //else {
        if(importedGameObject.transform.childCount == 1) {
            GameObject previousParent = importedGameObject;

            importedGameObject = previousParent.transform.GetChild(0).gameObject;
            importedGameObject.transform.parent = null;

            GameObject.Destroy(previousParent);
        }

        importedGameObject.transform.position = translate;
        importedGameObject.transform.localScale = scale;
        importedGameObject.transform.rotation = rotate;
        //}

        return importedGameObject;
    }

    private static Hashtable GetNodeFromLibrary(string nodeName, ArrayList library_nodes) {
        if(library_nodes == null || nodeName == null) return null;
        for(int i = 0; i < library_nodes.Count; i++) {
            Hashtable node = library_nodes.GetHashtable(i);
            if(node.GetString("id") == nodeName) return node;

            node = node.GetHashtable("node");
            if(node != null && node.GetString("id") == nodeName) return node;
        }
        return null;
    }
    private static void AppendGameObjectsFromNodes(GameObject parentGo, ArrayList nodes, ArrayList library_effects, ArrayList library_materials, ArrayList library_images, ArrayList library_geometries, ArrayList library_nodes, bool includeEmptyNodes, string colliderName, Quaternion upCompensation) {
        if(nodes == null) return;
        for(int n = 0; n < nodes.Count; n++) {
            Hashtable node = nodes.GetHashtable(n);
            if(node != null) {
                ArrayList instance_geometries = node.GetArrayList("instance_geometry", true);
                if(instance_geometries == null) {
                    Hashtable instance_node = node.GetHashtable("instance_node");
                    if(instance_node != null) {
                        string url = instance_node.GetString("url");
                        if(url != null && url.Length > 0) {
                            Hashtable subNode = GetNodeFromLibrary(url.RemoveStartCharactersIfPresent("#"), library_nodes);
                            if(subNode != null) {
                                node = subNode;
                                instance_geometries = subNode.GetArrayList("instance_geometry", true);
                            }
                        }
                    }
                }

                if(instance_geometries != null) {
                    //    GameObject firstGo = null;
                    for(int g = 0; g < instance_geometries.Count; g++) {
                        Hashtable instance_geometry = instance_geometries.GetHashtable(g);
                        string url = instance_geometry.GetString("url");
                        if(url != null && url.Length > 0) {
                            url = url.RemoveStartCharactersIfPresent("#");
                            GameObject go = ReadGeometry(url, instance_geometry, library_geometries, library_effects, library_materials, library_images);
                            if(go != null) {

                                //if(firstGo == null) firstGo = go;
                                AppendGameObjectToNode(parentGo, go, node, colliderName, upCompensation);
                                AppendGameObjectsFromNodes(go, node.GetArrayList("node", true), library_effects, library_materials, library_images, library_geometries, library_nodes, includeEmptyNodes, colliderName, upCompensation);
                            }
                        }
                    }
                    //    if(firstGo != null) {
                    //        AppendGameObjectsFromNodes(firstGo, node.GetArrayList("node", true), library_effects, library_materials, library_images, library_geometries, library_nodes, includeEmptyNodes, colliderName);
                    //    }
                }
                //else if(instance_geometries == null && includeEmptyNodes) {
                else if(includeEmptyNodes) {
                    // We have no geometry data associated, but we still want empty nodes to be imported
                    GameObject go = new GameObject();
                    AppendGameObjectToNode(parentGo, go, node, colliderName, upCompensation);
                    AppendGameObjectsFromNodes(go, node.GetArrayList("node", true), library_effects, library_materials, library_images, library_geometries, library_nodes, includeEmptyNodes, colliderName, upCompensation);
                }

            }
        }
    }

    private static void AppendGameObjectToNode(GameObject parentGo, GameObject go, Hashtable node, string colliderName, Quaternion upCompensation) {
        string nodeName = node.GetString("name");
        if(nodeName == null || nodeName.Length <= 0) nodeName = node.GetString("id");
        if(nodeName != null && nodeName.Length > 0) {
            go.name = nodeName;
            if(colliderName != null && colliderName.Length > 0 && nodeName.IndexOf(colliderName) >= 0) {
                MeshFilter mf = go.GetComponent<MeshFilter>();
                if(mf != null) {
                    MeshCollider mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.mesh;
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if(mr != null) GameObject.Destroy(mr);
                    GameObject.Destroy(mf);
                }
            }
        }

        go.transform.parent = parentGo.transform;

        Vector3 nodeTranslation = Vector3.zero;
        Vector3 nodeScale = Vector3.one;
        Quaternion nodeRotation = Quaternion.identity;

        Hashtable translate = node.GetHashtable("translate");
        if(translate != null) {
            string vecString = translate.GetString("translate");
            if(vecString != null) nodeTranslation = vecString.ToVector3(' ', Vector3.zero);
        }
        Hashtable scaleHash = node.GetHashtable("scale");
        if(scaleHash != null) {
            string vecString = scaleHash.GetString("scale");
            if(vecString != null) nodeScale = vecString.ToVector3(' ', new Vector3(1, 1, 1));
        }

        float rotX = 0f;
        float rotY = 0f;
        float rotZ = 0f;
        ArrayList rotates = node.GetArrayList("rotate");
        for(int r = 0; rotates != null && r < rotates.Count; r++) {
            Hashtable rotate = rotates.GetHashtable(r);
            if(rotate != null) {
                if(rotate.GetString("sid") == "rotationX" || rotate.GetString("sid") == "jointOrientX") {
                    string vecString = rotate.GetString("rotate");
                    if(vecString != null) {
                        float[] rotVec = vecString.ToFloatArray(' ');
                        if(rotVec.Length >= 4) rotX = rotVec[3];
                    }
                }
                if(rotate.GetString("sid") == "rotationY" || rotate.GetString("sid") == "jointOrientY") {
                    string vecString = rotate.GetString("rotate");
                    if(vecString != null) {
                        float[] rotVec = vecString.ToFloatArray(' ');
                        if(rotVec.Length >= 4) rotY = rotVec[3];
                    }
                }
                if(rotate.GetString("sid") == "rotationZ" || rotate.GetString("sid") == "jointOrientZ") {
                    string vecString = rotate.GetString("rotate");
                    if(vecString != null) {
                        float[] rotVec = vecString.ToFloatArray(' ');
                        if(rotVec.Length >= 4) rotZ = rotVec[3];
                    }
                }
            }
        }
        nodeRotation = Quaternion.Euler(new Vector3(rotX, rotY, rotZ));
        //Debug.Log(string.Format("Node Rotation:{0}:{1}", go.name, nodeRotation));
        Hashtable hash = node.GetHashtable("matrix");
        if(hash == null) hash = node;
        string matrixString = hash.GetString("matrix");
        if(matrixString != null && matrixString.Length > 0) {
            float[] values = matrixString.ToFloatArray(' ');
            if(values != null && values.Length == 16) {
                Matrix4x4 matrix = new Matrix4x4();
                for(int m = 0; m < 16; m++) matrix[m / 4, m % 4] = values[m];
                nodeTranslation = matrix.MultiplyPoint3x4(nodeTranslation);
                nodeRotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
                nodeScale = new Vector3(Vector3.Magnitude((Vector3)matrix.GetRow(0)), Vector3.Magnitude((Vector3)matrix.GetRow(1)), Vector3.Magnitude((Vector3)matrix.GetRow(2)));

                // This is playing dirty. You can not determin a negative scale from a matrix
                // see http://hub.jmonkeyengine.org/t/problem-with-fetching-the-scale-from-transformation-matrix/28048
                // Yet, unity somehow imports some objects with rotation 0,180,0 and scale -1,-1,-1
                // I don't know how to replicate this, so instead I can only cheat
                if((Vector3)matrix.GetRow(0) == new Vector3(1, 0, 0) && (Vector3)matrix.GetRow(1) == new Vector3(0, -1, 0) && (Vector3)matrix.GetRow(2) == new Vector3(0, 0, 1)) nodeScale = new Vector3(-1, -1, -1);
            }
        }

        go.transform.localPosition = ConvertPosition(nodeTranslation);
        go.transform.localScale = nodeScale;

        if(node.GetString("type") == "JOINT") {
            go.transform.Rotate(Vector3.forward, rotZ * -1f, Space.Self);
            go.transform.Rotate(Vector3.up, rotY * -1f, Space.Self);
            go.transform.Rotate(Vector3.right, rotX, Space.Self);
        }
        else {
            go.transform.localRotation = ConvertRotation(nodeRotation);
        }

        if(go.transform == go.transform.root || go.transform.parent == go.transform.root || parentGo.transform.parent == go.transform.root)
            go.transform.RotateAround(parentGo.transform.position, go.transform.root.right, -90);

        //go.transform.RotateAround(parentGo.transform.position, go.transform.root.right, -90);
    }


    private static Vector3 ConvertPosition(Vector3 pos) {
        return new Vector3(pos.x * -1f, pos.y, pos.z);
    }

    private static Quaternion ConvertRotation(Quaternion rot) {
        return Quaternion.Euler(ConvertRotationVector(rot.eulerAngles));
    }
    private static Vector3 ConvertRotationVector(Vector3 rotEuler) {
        if(rotEuler.x == 0f && rotEuler.y == 0f && rotEuler.z == 180f) rotEuler = new Vector3(0, 180, 0);
        else if(rotEuler.x == 0f && rotEuler.y == 0f && rotEuler.z == 90f) rotEuler = new Vector3(0, 180, 270);
        else if(rotEuler.x == 0f && rotEuler.y == 0f && rotEuler.z == 270f) rotEuler = new Vector3(0, 0, 90);
        else if(rotEuler.x == 0f && rotEuler.y == 180f && rotEuler.z == 90f) rotEuler = new Vector3(0, 0, 270);
        else if(rotEuler.x == 0f && rotEuler.y == 180f && rotEuler.z == 270f) rotEuler = new Vector3(0, 180, 90);
        else rotEuler = new Vector3(rotEuler.x, rotEuler.y * -1f, rotEuler.z * -1f);
        return rotEuler;
    }


    private static GameObject ReadGeometry(string url, Hashtable instance_geometry, ArrayList library_geometries, ArrayList library_effects, ArrayList library_materials, ArrayList library_images) {

        GameObject go = null;
        for(int i = 0; i < library_geometries.Count; i++) {
            Hashtable geometry = library_geometries.GetHashtable(i);
            if(geometry != null && geometry.ContainsKey("geometry")) geometry = geometry.GetHashtable("geometry");
            if(geometry != null) {
                string geometryId = geometry.GetString("id");
                if(geometryId == url) {
                    string geometryName = geometry.GetString("name");
                    if(geometryName == null) geometryName = "collada";
                    ArrayList meshDefs = geometry.GetArrayList("mesh", true);
                    for(int j = 0; j < meshDefs.Count; j++) {
                        Hashtable meshDef = meshDefs.GetHashtable(j);
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector3> normals = new List<Vector3>();
                        List<Vector2> uvs = new List<Vector2>();
                        List<Color> colors = new List<Color>();
                        List<int> old2NewVertexIndex = new List<int>();
                        List<int> triangles = new List<int>();
                        List<int> submeshOffsets = new List<int>();
                        List<string> materialNames = new List<string>();
                        submeshOffsets.Add(0);

                        ArrayList polylists = meshDef.GetArrayList("polylist", true);
                        for(int k = 0; polylists != null && k < polylists.Count; k++) {
                            Hashtable def = polylists.GetHashtable(k);
                            if(ReadPolygonsInto(def, ref meshDef, ref vertices, ref normals, ref uvs, ref colors, ref triangles)) {
                                materialNames.Add(def.GetString("material"));
                                submeshOffsets.Add(triangles.Count);
                            }
                        }

                        ArrayList polygons = meshDef.GetArrayList("polygons", true);
                        for(int k = 0; polygons != null && k < polygons.Count; k++) {
                            Hashtable def = polygons.GetHashtable(k);
                            if(ReadPolygonsInto(def, ref meshDef, ref vertices, ref normals, ref uvs, ref colors, ref triangles)) {
                                materialNames.Add(def.GetString("material"));
                                submeshOffsets.Add(triangles.Count);
                            }
                        }

                        ArrayList triangleDefs = meshDef.GetArrayList("triangles", true);
                        for(int k = 0; triangleDefs != null && k < triangleDefs.Count; k++) {
                            Hashtable def = triangleDefs.GetHashtable(k);
                            if(ReadTrianglesInto(def, ref meshDef, ref vertices, ref normals, ref uvs, ref colors, ref old2NewVertexIndex, ref triangles)) {
                                materialNames.Add(def.GetString("material"));
                                submeshOffsets.Add(triangles.Count);
                            }
                        }
                        //					Log("vertices: "+vertices.Count);
                        //					Log("normals: "+normals.Count);
                        //					Log("uvs: "+uvs.Count);
                        //					Log("colors:"+colors.Count);		
                        //					Log("triangles: "+triangles.Count);
                        //					Log("submeshOffsets: "+submeshOffsets.Count);

                        if(submeshOffsets.Count <= 1) {
                            materialNames.Add(null);
                            submeshOffsets.Add(triangles.Count);
                        }

                        Mesh mesh = new Mesh();
                        Vector3[] vs = vertices.ToArray();
                        FlipVertexXAxis(ref vs);
                        mesh.vertices = vs;
                        if(normals.Count > 0) {
                            Vector3[] ns = normals.ToArray();
                            FlipNormals(ref ns);
                            mesh.normals = ns;
                        }
                        mesh.uv = uvs.ToArray();
                        if(colors.Count > 0) mesh.colors = colors.ToArray();
                        mesh.subMeshCount = submeshOffsets.Count - 1;
                        for(int k = 0; k < submeshOffsets.Count - 1; k++) {
                            int[] tris;
                            int from = submeshOffsets[k];
                            int to = submeshOffsets[k + 1];
                            if(to > from) {
                                tris = new int[to - from];
                                triangles.CopyTo(from, tris, 0, to - from);
                                FlipTriangles(ref tris);
                                mesh.SetTriangles(tris, k);
                            }
                        }
                        if(normals.Count <= 0) mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        mesh.RecalculateTangents();

                        go = new GameObject(geometryName);
                        MeshRenderer mr = go.AddComponent<MeshRenderer>();
                        MeshFilter mf = go.AddComponent<MeshFilter>();
                        mf.mesh = mesh;

                        Material[] mats = new Material[mesh.subMeshCount];
                        for(int k = 0; k < mesh.subMeshCount; k++) {
                            string matName = materialNames[k];
                            Shader sh = Shader.Find("Diffuse");
                            mats[k] = new Material(sh);
                            mats[k].name = matName != null ? matName : ("material_" + k);
                            if(matName != null) {
                                string materialUrl = GetMaterialUrl(instance_geometry.GetArrayList("bind_material", true));
                                string effectUrl = GetEffectUrl(materialUrl, library_materials);
                                Hashtable effectProfile = GetFirstProfileFromLibraryEffects(effectUrl, library_effects);
                                ArrayList effectTechniques = GetFirstTechniqueFromProfile(effectProfile);
                                Color diffuseColor = GetFirstColorFromTechniques(effectTechniques);
                                float opacity = GetFirstTransparencyFromTechniques(effectTechniques);
                                if(opacity < 1f) diffuseColor.a = opacity;
                                if(diffuseColor.a < 1f) {
                                    sh = Shader.Find("Transparent/Diffuse");
                                    if(sh != null) mats[k].shader = sh;
                                }
                                string textureUrl = GetFirstTextureUrlFromTechniques(effectTechniques);
                                textureUrl = GetNewParamForTextureUrl(textureUrl, effectProfile);
                                // Debug.Log("GetNewParamForTextureUrl:"+textureUrl);
                                textureUrl = GetFileUrlForTexture(textureUrl, library_images);
                                //Debug.Log("GetFileUrlForTexture:"+textureUrl);

                                mats[k].SetColor("_Color", diffuseColor);
                                if(textureUrl != null && textureUrl.Length > 0) {
                                    Texture2D tempTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                                    tempTex.name = textureUrl;  // this name can be used to download the actual texture afterwards
                                    mats[k].SetTexture("_MainTex", tempTex);
                                }
                            }
                        }
                        mr.materials = mats;
                    }
                }
            }
        }
        return go;
    }

    static int GetChildDepth(Transform child) {
        int depth = 0;

        while(child.parent != null) {
            child = child.parent;
            depth++;
        }

        return depth;
    }

    private static string GetMaterialUrl(ArrayList bind_materials) {
        for(int h = 0; bind_materials != null && h < bind_materials.Count; h++) {
            Hashtable bind_material = bind_materials.GetHashtable(h);
            if(bind_material != null) {
                ArrayList technique_commons = bind_material.GetArrayList("technique_common", true);
                for(int j = 0; technique_commons != null && j < technique_commons.Count; j++) {
                    Hashtable technique_common = technique_commons.GetHashtable(j);
                    if(technique_common != null) {
                        ArrayList instance_materials = technique_common.GetArrayList("instance_material", true);
                        if(instance_materials == null || instance_materials.Count == 0) {
                            if(technique_common.GetString(".tag.") == "instance_material") {
                                Hashtable instance_material = technique_common;
                                if(instance_material != null) {
                                    string target = instance_material.GetString("target");
                                    if(target != null && target.Length > 0) {
                                        return target.RemoveStartCharactersIfPresent("#");
                                    }
                                }
                            }
                        }
                        else {
                            for(int k = 0; instance_materials != null && k < instance_materials.Count; k++) {
                                Hashtable instance_material = instance_materials.GetHashtable(k);
                                if(instance_material != null) {
                                    string target = instance_material.GetString("target");
                                    if(target != null && target.Length > 0) {
                                        return target.RemoveStartCharactersIfPresent("#");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return null;
    }

    private static string GetEffectUrl(string materialUrl, ArrayList library_materials) {
        if(materialUrl == null) return null;
        for(int j = 0; library_materials != null && j < library_materials.Count; j++) {
            Hashtable library_material = library_materials.GetHashtable(j);
            if(library_material.ContainsKey("material")) library_material = library_material.GetHashtable("material");
            if(library_material != null && library_material.GetString("id") == materialUrl) {
                ArrayList instance_effects = library_material.GetArrayList("instance_effect", true);
                for(int x = 0; instance_effects != null && x < instance_effects.Count; x++) {
                    Hashtable instance_effect = instance_effects.GetHashtable(x);
                    if(instance_effect != null) {
                        string url = instance_effect.GetString("url");
                        if(url != null && url.Length > 0) {
                            return url.RemoveStartCharactersIfPresent("#");
                        }
                    }
                }
            }
        }
        return null;
    }

    private static Hashtable GetFirstProfileFromLibraryEffects(string effect_url, ArrayList library_effects) {
        if(effect_url == null) return null;
        for(int j = 0; library_effects != null && j < library_effects.Count; j++) {
            Hashtable effect = library_effects.GetHashtable(j);
            if(effect != null && effect.ContainsKey("effect")) effect = effect.GetHashtable("effect");
            if(effect != null && effect.GetString("id") == effect_url) {
                ArrayList profile_COMMONS = effect.GetArrayList("profile_COMMON", true);
                if(profile_COMMONS != null && profile_COMMONS.Count > 0) return profile_COMMONS.GetHashtable(0);
            }
        }
        return null;
    }

    private static ArrayList GetFirstTechniqueFromProfile(Hashtable profile) {
        if(profile != null) {
            ArrayList techniques = profile.GetArrayList("technique", true);
            for(int k = 0; techniques != null && k < techniques.Count; k++) {
                Hashtable technique = techniques.GetHashtable(k);
                if(technique != null && technique.ContainsKey("technique")) technique = technique.GetHashtable("technique");
                if(technique != null) {
                    ArrayList techniquesList = technique.GetArrayList("phong", true);
                    if(techniquesList != null && techniquesList.Count > 0) return techniquesList;
                    techniquesList = technique.GetArrayList("lambert", true);
                    if(techniquesList != null && techniquesList.Count > 0) return techniquesList;
                    techniquesList = technique.GetArrayList("blinn", true);
                    if(techniquesList != null && techniquesList.Count > 0) return techniquesList;
                }
            }
        }
        return null;
    }

    private static Color GetFirstColorFromTechniques(ArrayList techniques) {
        Color material_diffuse_color = Color.grey;
        for(int p = 0; techniques != null && p < techniques.Count; p++) {
            Hashtable technique = techniques.GetHashtable(p);
            if(technique != null) {
                ArrayList diffuses = technique.GetArrayList("diffuse", true);
                for(int r = 0; diffuses != null && r < diffuses.Count; r++) {
                    Hashtable diffuse = diffuses.GetHashtable(r);
                    if(diffuse != null && diffuse.ContainsKey("diffuse")) diffuse = diffuse.GetHashtable("diffuse");
                    if(diffuse != null) {
                        string diffuseString = diffuse.GetString("color");

                        // Alternative method if the default method fails...
                        if(diffuseString == null) {
                            ArrayList diffuse_colors = diffuse.GetArrayList("color", true);
                            for(int d = 0; diffuse_colors != null && d < diffuse_colors.Count; d++) {
                                Hashtable diffuse_color = diffuse_colors.GetHashtable(d);
                                if(diffuse_color != null) {
                                    diffuseString = diffuse_color.GetString("color");
                                }
                            }
                        }
                        if(diffuseString != null) {
                            string[] diffuseArray = diffuseString.Split(new char[' '], StringSplitOptions.RemoveEmptyEntries);
                            if(diffuseArray.Length == 3) material_diffuse_color = new Color(float.Parse(diffuseArray[0]), float.Parse(diffuseArray[1]), float.Parse(diffuseArray[2]), 1f);
                            else if(diffuseArray.Length > 3) material_diffuse_color = new Color(float.Parse(diffuseArray[0]), float.Parse(diffuseArray[1]), float.Parse(diffuseArray[2]), float.Parse(diffuseArray[3]));
                        }
                    }
                }
            }
        }
        return material_diffuse_color;
    }
    private static float GetFirstTransparencyFromTechniques(ArrayList techniques) {
        for(int p = 0; techniques != null && p < techniques.Count; p++) {
            Hashtable technique = techniques.GetHashtable(p);
            if(technique != null) {
                ArrayList transparencies = technique.GetArrayList("transparency", true);
                for(int r = 0; transparencies != null && r < transparencies.Count; r++) {
                    Hashtable transparency = transparencies.GetHashtable(r);
                    if(transparency != null && transparency.ContainsKey("transparency")) transparency = transparency.GetHashtable("transparency");
                    if(transparency != null) {
                        string transpString = transparency.GetString("float");

                        // Alternative method if the default method fails...
                        if(transpString == null) {
                            ArrayList diffuse_colors = transparency.GetArrayList("float", true);
                            for(int d = 0; diffuse_colors != null && d < diffuse_colors.Count; d++) {
                                Hashtable diffuse_color = diffuse_colors.GetHashtable(d);
                                if(diffuse_color != null) {
                                    transpString = diffuse_color.GetString("float");
                                }
                            }
                        }
                        if(transpString != null) {
                            return float.Parse(transpString);
                        }
                    }
                }
            }
        }
        return 1f;
    }
    private static string GetFirstTextureUrlFromTechniques(ArrayList techniques) {
        for(int p = 0; techniques != null && p < techniques.Count; p++) {
            Hashtable technique = techniques.GetHashtable(p);
            if(technique != null && technique.ContainsKey("technique")) technique = technique.GetHashtable("technique");
            if(technique != null) {
                ArrayList diffuses = technique.GetArrayList("diffuse", true);
                for(int r = 0; diffuses != null && r < diffuses.Count; r++) {
                    Hashtable diffuse = diffuses.GetHashtable(r);
                    if(diffuse != null && diffuse.ContainsKey("diffuse")) diffuse = diffuse.GetHashtable("diffuse");
                    if(diffuse != null) {
                        Hashtable textureHash = diffuse.GetHashtable("texture");
                        if(textureHash != null) {
                            string url = textureHash.GetString("texture");
                            if(url != null) {
                                url = url.RemoveStartCharactersIfPresent("#");
                                return url;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }

    private static string GetNewParamForTextureUrl(string url, Hashtable profile) {
        string prevUrl = null;
        while(prevUrl != url) {
            prevUrl = url;
            if(profile != null && profile.ContainsKey("newparam")) {
                ArrayList newParams = profile.GetArrayList("newparam", true);
                for(int p = 0; newParams != null && p < newParams.Count; p++) {
                    Hashtable newParam = newParams.GetHashtable(p);
                    if(newParam.GetString("sid") == url) {
                        Hashtable surface = newParam.GetHashtable("surface");
                        if(surface != null) newParam = surface;
                        string newUrl = newParam.GetString("init_from");
                        if(newUrl == null) newUrl = newParam.GetString("source");
                        if(newUrl != null && newUrl.Length > 0) {
                            url = newUrl.RemoveStartCharactersIfPresent("#");
                        }
                        Hashtable sampler2D = newParam.GetHashtable("sampler2D");
                        if(sampler2D != null) {
                            newUrl = sampler2D.GetString("init_from");
                            if(newUrl == null) newUrl = sampler2D.GetString("source");
                            if(newUrl != null && newUrl.Length > 0) {
                                url = newUrl.RemoveStartCharactersIfPresent("#");
                            }
                        }
                    }
                }
            }
        }
        return url;
    }


    private static string GetFileUrlForTexture(string url, ArrayList library_images) {
        for(int p = 0; library_images != null && p < library_images.Count; p++) {
            Hashtable imageHash = library_images.GetHashtable(p);
            if(imageHash != null && imageHash.ContainsKey("image")) imageHash = imageHash.GetHashtable("image");
            if(imageHash != null && imageHash.GetString("id") == url) {
                return imageHash.GetString("init_from");
            }
        }
        return null;
    }

    private static Color OldGetFirstColorFromLibraryEffects(string effect_url, ArrayList library_effects) {
        Color material_diffuse_color = Color.grey;
        Hashtable effect = GetNodeFromLibrary(effect_url, library_effects);
        if(effect != null) {
            ArrayList profile_COMMONS = effect.GetArrayList("profile_COMMON", true);
            for(int x = 0; profile_COMMONS != null && x < profile_COMMONS.Count; x++) {
                Hashtable profile_COMMON = profile_COMMONS.GetHashtable(x);
                if(profile_COMMON != null) {
                    ArrayList techniques = profile_COMMON.GetArrayList("technique", true);
                    for(int k = 0; techniques != null && k < techniques.Count; k++) {
                        Hashtable technique = techniques.GetHashtable(k);
                        if(technique != null) {
                            ArrayList phongs = technique.GetArrayList("phong", true);
                            for(int p = 0; phongs != null && p < phongs.Count; p++) {
                                Hashtable phong = phongs.GetHashtable(p);
                                if(phong != null) {
                                    ArrayList diffuses = phong.GetArrayList("diffuse", true);
                                    for(int r = 0; diffuses != null && r < diffuses.Count; r++) {
                                        Hashtable diffuse = diffuses.GetHashtable(r);
                                        if(diffuse != null) {
                                            string diffuseString = diffuse.GetString("color");

                                            // Alternative method if the default method fails...
                                            if(diffuseString == null) {
                                                ArrayList diffuse_colors = diffuse.GetArrayList("color", true);
                                                for(int d = 0; diffuse_colors != null && d < diffuse_colors.Count; d++) {
                                                    Hashtable diffuse_color = diffuse_colors.GetHashtable(d);
                                                    if(diffuse_color != null) {
                                                        diffuseString = diffuse_color.GetString("color");
                                                    }
                                                }
                                            }
                                            string[] diffuseArray = diffuseString.Split(new char[' '], StringSplitOptions.RemoveEmptyEntries);
                                            material_diffuse_color = new Color(float.Parse(diffuseArray[0]), float.Parse(diffuseArray[1]), float.Parse(diffuseArray[2]));
                                        }
                                    }
                                }
                                else {
                                    // CHECK FOR OTHER TYPES!
                                }
                            }
                        }
                    }
                }
            }
        }
        return material_diffuse_color;
    }

    private static bool ReadPolygonsInto(Hashtable polylist, ref Hashtable meshDef, ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> uvs, ref List<Color> colors, ref List<int> triangles) {
        Vector3[] vs = null;
        Vector3[] ns = null;
        Vector2[] us = null;
        Vector3[] cs = null;
        int[] old2NewIndex;
        int offsetV = -1;
        int offsetN = -1;
        int offsetU = -1;
        int offsetC = -1;
        int offsetCount = 1;

        if(polylist == null) return false;

        // Find sourcedata by using the input specification in the polygon/polylist definition
        ArrayList inputs = polylist.GetArrayList("input", true);
        if(inputs == null || inputs.Count <= 0) return false;

        for(int i = 0; i < inputs.Count; i++) {
            offsetCount = Mathf.Max(offsetCount, inputs.GetHashtable(i).GetInt("offset"));
        }
        offsetCount++;
        int foundOffset = -1;
        vs = ReadVector3ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "POSITION", "VERTEX"));
        if(vs == null || vs.Length <= 0) {
            Debug.Log("no vertices found");
            throw new ColladaException("No vertices found");
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for vertices");
            throw new ColladaException("No offset found for vertices");
        }
        offsetV = foundOffset;
        // We now have a raw array of vertices and an offset for reading the triangles/polygons

        foundOffset = -1;
        ns = ReadVector3ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "NORMAL", "VERTEX"));
        if(ns == null || ns.Length <= 0) {
            Debug.Log("no normals found");
            foundOffset = -1;
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for normals");
        }
        offsetN = foundOffset;
        // We now have a raw array of normals and an offset for reading the triangles/polygons

        foundOffset = -1;
        us = ReadVector2ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "TEXCOORD", ""));
        if(us == null || us.Length <= 0) {
            //Debug.Log("no uvs found");
            foundOffset = -1;
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for uvs");
        }
        offsetU = foundOffset;

        foundOffset = -1;
        cs = ReadVector3ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "COLOR", ""));
        if(cs == null || cs.Length <= 0) {
            foundOffset = -1;
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for vertex colors");
        }
        offsetC = foundOffset;

        // Unity requires the same nr of vertices, normals and uv coordinates. 
        // But Collada doesnt work that way
        // So by tracking down the vertex, normals and uv per corner of a triangle/polygon
        // we copy/expand the nr of vertices, normals and uvs to match the nr of triangle/polygon corners
        // This would be highly inefficient, so to prevent copying too much, we test if the combination
        // vertex, normal, uv has already been used. If so, we dont copy but reference to that index
        // We therefor need to keep an array with references to see where our raw vertices went
        old2NewIndex = new int[vs.Length];
        for(int i = 0; i < old2NewIndex.Length; i++) old2NewIndex[i] = -1;

        ArrayList ps = polylist.GetArrayList("p", true);
        if(ps == null || offsetCount <= 0) return false; // nothing to import if this isnt present
        for(int i = 0; i < ps.Count; i++) {
            string pString = ps.GetString(i);
            if(pString == null) {
                Hashtable pHash = ps.GetHashtable(i);
                if(pHash != null) pString = pHash.GetString("p");
            }
            if(pString == null) return false; // this shouldnt be possible
            List<int> p = pString.ToIntList(' ');
            string vcountString = polylist.GetString("vcount");
            List<int> vcounts;
            if(vcountString == null) {
                vcounts = new List<int>();
                vcounts.Add(p.Count / offsetCount);
            }
            else {
                vcounts = vcountString.ToIntList(' ');
            }

            // read p  2 0 0  0 0 1  1 0 2  3 0 3  2 1 4  3 1 5  5 1 6  4 1 7
            int pIdx = 0;
            for(int j = 0; j < vcounts.Count; j++) {  // polygon by polygon
                int vcount = vcounts[j];
                int[] triangleIndexes = new int[vcount];
                for(int k = 0; k < vcount; k++) {  // corner by corner
                    Vector3 v = new Vector3(0, 0, 0);
                    Vector3 n = new Vector3(0, 0, -1);
                    Vector2 u = new Vector2(0, 0);
                    Vector3 c = new Vector3(0, 0, 0);
                    int rawVertexIdx = -1;
                    for(int offset = 0; offset < offsetCount; offset++) {
                        int rawIndex = p[pIdx + (k * offsetCount) + offset];
                        if(offset == offsetV) {
                            rawVertexIdx = rawIndex;
                            v = vs[rawIndex];
                        }
                        if(offset == offsetN) {
                            n = ns[rawIndex];
                        }
                        if(offset == offsetU) {
                            u = us[rawIndex];
                        }
                        if(offset == offsetC) {
                            c = cs[rawIndex];
                        }
                    }
                    int newVertexIndex = old2NewIndex[rawVertexIdx];
                    if(newVertexIndex >= 0) {  // this vertex was already used in a triangle
                        Vector3 newN = n;
                        if(offsetN > 0) newN = normals[newVertexIndex];
                        // test if the vertex, normal, uv compbination is already used
                        if(!IsSameVertex(v, n, u, vertices[newVertexIndex], newN, uvs[newVertexIndex])) {
                            newVertexIndex = vertices.Count;
                        }
                    }
                    else {
                        newVertexIndex = vertices.Count;
                    }

                    if(newVertexIndex >= vertices.Count) {
                        vertices.Add(v);
                        if(offsetN >= 0) normals.Add(n);
                        uvs.Add(u);
                        if(offsetC >= 0) colors.Add(new Color(c.x, c.y, c.z));
                        old2NewIndex[rawVertexIdx] = newVertexIndex;  // remember where the vertex went to
                    }
                    triangleIndexes[k] = newVertexIndex;

                }

                PolygonIntoTriangle(triangleIndexes, ref triangles);
                pIdx += vcount * offsetCount;
            }
        }
        return true;
    }


    private static bool ReadTrianglesInto(Hashtable triDef, ref Hashtable meshDef, ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> uvs, ref List<Color> colors, ref List<int> doubledVertices, ref List<int> triangles) {
        Vector3[] vs = null;
        Vector3[] ns = null;
        Vector2[] us = null;
        Vector3[] cs = null;
        int[] old2NewIndex;
        int offsetV = -1;
        int offsetN = -1;
        int offsetU = -1;
        int offsetC = -1;
        int offsetCount = 1;

        if(triDef == null) return false;

        // Find sourcedata by using the input specification in the triangles definition
        ArrayList inputs = triDef.GetArrayList("input", true);
        if(inputs == null || inputs.Count <= 0) return false;

        int foundOffset = -1;
        vs = ReadVector3ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "POSITION", "VERTEX"));
        if(vs == null || vs.Length <= 0) {
            Debug.Log("no vertices found");
            throw new ColladaException("No vertices found");
        }
        else if(foundOffset < 0) {
            Debug.Log("No offset found for vertices");
            throw new ColladaException("No offset found for vertices");
        }
        offsetV = foundOffset;
        // We now have a raw array of vertices and an offset for reading the triangles/polygons

        foundOffset = -1;
        ns = ReadVector3ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "NORMAL", "VERTEX"));
        if(ns == null || ns.Length <= 0) {
            Debug.Log("no normals found");
            foundOffset = -1;
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for normals");
        }
        offsetN = foundOffset;
        // We now have a raw array of normals and an offset for reading the triangles/polygons

        foundOffset = -1;
        us = ReadVector2ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "TEXCOORD", ""));
        if(us == null || us.Length <= 0) {
            // Debug.Log("no uvs found");
            foundOffset = -1;
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for uvs");
        }
        offsetU = foundOffset;
        // We now have a raw array of uv coordinates and an offset for reading the triangles/polygons

        foundOffset = -1;
        cs = ReadVector3ArrayFromSourceData(
            FindSourceDataAndOffsetInHierarchy(ref foundOffset, ref meshDef, ref inputs, "COLOR", ""));
        if(cs == null || cs.Length <= 0) {
            //			Debug.Log("no vertex colors found");
            foundOffset = -1;
        }
        else if(foundOffset < 0) {
            Debug.Log("no offset found for vertex colors");
        }
        offsetC = foundOffset;
        //		Log("colors:",cs);

        // Determine the total nr of offsets for reading the triangles/polygons
        offsetCount = Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(offsetU, offsetN), offsetV), offsetC) + 1, inputs.Count);

        // Unity requires the same nr of vertices, normals and uv coordinates. 
        // But Collada doesnt work that way
        // So by tracking down the vertex, normals and uv per corner of a triangle/polygon
        // we copy/expand the nr of vertices, normals and uvs to match the nr of triangle/polygon corners
        // This would be highly inefficient, so to prevent copying too much, we test if the combination
        // vertex, normal, uv has already been used. If so, we dont copy but reference to that index
        // We therefor need to keep an array with references to see where our raw vertices went
        old2NewIndex = new int[vs.Length];
        for(int i = 0; i < old2NewIndex.Length; i++) old2NewIndex[i] = -1;

        ArrayList ps = triDef.GetArrayList("p", true);
        if(ps == null) return false; // nothing to import if this isnt present
        for(int i = 0; i < ps.Count; i++) {
            string pString = ps.GetString(i);
            if(pString == null) {
                Hashtable pHash = ps.GetHashtable(i);
                if(pHash != null) pString = pHash.GetString("p");
            }
            if(pString == null) return false; // this shouldnt be possible
            List<int> p = pString.ToIntList(' ');

            // read p  2 0 0  0 0 1  1 0 2  3 0 3  2 1 4  3 1 5  5 1 6  4 1 7
            for(int pIdx = 0; pIdx < p.Count; pIdx += (3 * offsetCount)) {
                for(int k = 0; k < 3; k++) {  // corner by corner
                    Vector3 v = new Vector3(0, 0, 0);
                    Vector3 n = new Vector3(0, 0, -1);
                    Vector2 u = new Vector2(0, 0);
                    Vector3 c = new Vector3(0, 0, 0);
                    int rawVertexIdx = -1;
                    for(int offset = 0; offset < offsetCount; offset++) {
                        int rawIndex = p[pIdx + (k * offsetCount) + offset];
                        if(offset == offsetV) {
                            rawVertexIdx = rawIndex;
                            v = vs[rawIndex];
                        }
                        if(offset == offsetN) {
                            n = ns[rawIndex];
                        }
                        if(offset == offsetU) {
                            u = us[rawIndex];
                        }
                        if(offset == offsetC) {
                            c = cs[rawIndex];
                        }
                    }

                    int newVertexIndex = old2NewIndex[rawVertexIdx];
                    if(newVertexIndex >= 0) {  // this vertex was already used in a triangle
                        Vector3 newN = n;
                        if(offsetN > 0) newN = normals[newVertexIndex];
                        // test if the vertex, normal, uv combination is already used
                        if(!IsSameVertex(v, n, u, vertices[newVertexIndex], newN, uvs[newVertexIndex])) {
                            newVertexIndex = vertices.Count;
                        }
                    }
                    else {
                        newVertexIndex = vertices.Count;
                    }

                    if(newVertexIndex >= vertices.Count) {
                        vertices.Add(v);
                        if(offsetN >= 0) normals.Add(n);
                        uvs.Add(u);
                        if(offsetC >= 0) colors.Add(new Color(c.x, c.y, c.z));
                        old2NewIndex[rawVertexIdx] = newVertexIndex;  // remember where the vertex went to
                    }
                    triangles.Add(newVertexIndex);
                }
            }
        }
        return true;
    }

    private static Hashtable FindSourceDataAndOffsetInHierarchy(ref int offset, ref Hashtable meshDef, ref ArrayList inputs, string identifier, string altIdentifier) {
        // Find the sourcedata for a collada model is done by following the path from triangles/polygons 
        // to vertex->position and to normals and texcoord.
        // this is indicated with the tag "semantic"

        Hashtable retval = null;
        for(int i = 0; i < inputs.Count; i++) {
            Hashtable input = inputs.GetHashtable(i);
            string semantic = input.GetString("semantic");
            if(semantic == identifier || semantic == altIdentifier) {
                offset = input.GetInt("offset", -1);
                Hashtable sourceData = null;
                string sourceName = input.GetString("source");
                if(sourceName != null) {
                    sourceName = sourceName.RemoveStartCharactersIfPresent("#");
                    sourceData = meshDef.GetNodeWithProperty("id", sourceName);
                    if(sourceData != null) {
                        if(sourceData.ContainsKey("input")) {
                            Hashtable subInput = sourceData.GetHashtable("input");
                            if(subInput != null) {
                                semantic = subInput.GetString("semantic");
                                if(semantic == identifier) {
                                    sourceName = subInput.GetString("source");
                                    if(sourceName != null) {
                                        sourceName = sourceName.RemoveStartCharactersIfPresent("#");
                                        sourceData = meshDef.GetNodeWithProperty("id", sourceName);
                                        if(sourceData != null) i = inputs.Count;  // stop searching
                                    }
                                }
                            }
                            else {
                                ArrayList subInputs = sourceData.GetArrayList("input", true);
                                for(int j = 0; j < subInputs.Count; j++) {
                                    subInput = subInputs.GetHashtable(j);
                                    if(subInput != null) {
                                        semantic = subInput.GetString("semantic");
                                        if(semantic == identifier) {
                                            sourceName = subInput.GetString("source");
                                            if(sourceName != null) {
                                                sourceName = sourceName.RemoveStartCharactersIfPresent("#");
                                                sourceData = meshDef.GetNodeWithProperty("id", sourceName);
                                                if(sourceData != null) i = inputs.Count;  // stop searching
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            if(sourceData != null) i = inputs.Count;  // stop searching
                        }
                        input["sourceData"] = sourceData;
                    }

                    if(sourceData != null) retval = sourceData;
                }
            }
        }
        return retval;
    }

    private static Vector2[] ReadVector2ArrayFromSourceData(Hashtable sourceData) {
        int floatsPerValue = 2;
        object value = sourceData.GetNodeAtPath(new string[2] { "technique_common", "accessor" });
        if(value != null && value.GetType() == typeof(Hashtable)) floatsPerValue = ((Hashtable)value).GetInt("stride");
        if(floatsPerValue < 2) floatsPerValue = 2;
        Vector2[] vs = null;
        if(sourceData != null) {
            Hashtable floatHash = sourceData.GetHashtable("float_array");
            if(floatHash != null) {
                string floatString = floatHash.GetString("float_array");
                if(floatString != null) {
                    vs = floatString.ToVector2List(' ', floatsPerValue).ToArray();
                }
            }
        }
        return vs;
    }
    private static Vector3[] ReadVector3ArrayFromSourceData(Hashtable sourceData) {
        Vector3[] vs = null;
        if(sourceData != null) {
            Hashtable floatHash = sourceData.GetHashtable("float_array");
            if(floatHash != null) {
                string floatString = floatHash.GetString("float_array");
                if(floatString != null) {
                    vs = floatString.ToVector3List(' ').ToArray();
                }
            }
        }
        return vs;
    }

    private static bool IsSameVertex(Vector3 v1, Vector3 n1, Vector2 u1, Vector3 v2, Vector3 n2, Vector2 u2) {
        return (v1 == v2 && n1 == n2 && u1 == u2);
    }

    private static void PolygonIntoTriangle(int[] polygon, ref List<int> triangles) {
        if(polygon.Length < 3) return; // no lines supported
        else if(polygon.Length == 3) {  // the polygon is already a triangle
            for(int i = 0; i < 3; i++) triangles.Add(polygon[i]);
        }
        else {
            // we divide the polygon in half until it is composed of only triangles
            int i = 0;
            int p = 0;
            int halfSize = polygon.Length / 2;  // index of the corner on the other side
            int[] p1 = new int[halfSize + 1];  // set new polygons size 
            int[] p2 = new int[(polygon.Length - halfSize) + 1];
            for(i = 0; i < p1.Length; i++) {  // copy into p1
                p1[i] = polygon[p++];
            }
            p2[0] = polygon[p - 1];
            for(i = 1; i < p2.Length - 1; i++) {  // copy the rest into p2
                p2[i] = polygon[p++];
            }
            p2[i] = polygon[0];
            PolygonIntoTriangle(p1, ref triangles);  // if our polygons are not ye triangles, the process is repeated
            PolygonIntoTriangle(p2, ref triangles);
        }
    }

    private static void Log(string str, Vector3[] vectors) {
        string s = str + ": ";
        for(int i = 0; i < vectors.Length; i++) s = s + "\n" + vectors[i];
        Log(s);
    }

    private static void Log(string str) {
        Debug.Log(str + "\n" + DateTime.Now.ToString("yyy/MM/dd hh:mm:ss.fff"));
    }

    private static void FlipVertexXAxis(ref Vector3[] vs) {
        for(int i = 0; i < vs.Length; i++) {
            vs[i].x *= -1;
        }
    }
    private static void FlipNormals(ref Vector3[] ns) {
        for(int i = 0; i < ns.Length; i++) {
            ns[i].x *= -1;
        }
    }
    private static void FlipTriangles(ref int[] tris) {
        for(int t = 0; t < tris.Length; t += 3) {
            int tmp = tris[t];
            tris[t] = tris[t + 1];
            tris[t + 1] = tmp;
        }
    }
}

