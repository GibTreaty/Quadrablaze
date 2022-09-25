SimpleCollada
-------------
Import COLLADA models into Unity3D at runtime. SimpleCollada provides the tools to let users upload 3D models 
and use them in your game.

No fancy stuff, no nonsense. It imports vertices, normals, 1 uv map, and triangles. (polygons are 
automatically converted to triangles). Supports multiple meshes and sub meshes.

Just download a Collada file into a string, pass the string to the import function and receive a GameObject in return. Like so:
	// import a Collada string
	myGameObject = ColladaImporter.Import(colladaString);

Also includes the Simple XML Asset Store package.


Documentation
-------------
The full documentation is available online at http://orbcreation.com/orbcreation/docu.orb?1033


Package Contents
----------------
SimpleCollada / ReadMe.txt   (this file)
SimpleCollada / Documentation.txt   (documentation in plain text format better use http://orbcreation.com/orbcreation/docu.orb?1033)
SimpleCollada / ColladaImporter.cs   (the Collada importer)
SimpleCollada / MeshExtensions.cs   (class extensions for Mesh)
SimpleCollada / OrbCreationExtensions   (various extensions to default classes used also in other Orbcreation packages)
SimpleCollada / SimpleXML   (SimpleXML assetstore package)
SimpleCollada / Demo / SimpleColladaDemo.unity   (the demo scene)
SimpleCollada / Demo / SimpleColladaDemoCtrl.cs (script that runs the demo)
SimpleCollada / Demo / SampleColladaFiles (sample files that can also be found at http://orbcreation.com/SimpleCollada)
SimpleCollada / Demo / Materials (materials used for floor and background)
SimpleCollada / Demo / Shaders (simple shaders for text and background)
SimpleCollada / Demo / Textures (grid texture for the floor, grid texture for the models, background image)


Minimal needed in your project
------------------------------
The following files need to be somewhere in your project folders:
- ColladaImporter.cs
- MeshExtensions.cs
- Folder OrbCreationExtensions
All the rest can go.


Using multiple Orbcreation packages
-----------------------------------
When you also use other Orbcreation AssetStore packages in your project, chances are that they too will contain a copy of 
the folder OrbCreationExtensions. It is best to merge those folders together into 1, always using the latest version of 
files inside the folder. You can check the versions by opening the files inside the OrbCreationExtensions folder.


Downloading textures
--------------------
Inside a .DAE collada file there are no textures, only the relative location of the textures is provided. So after importing 
the Collada file and generating the gameObject(s) from it, the  textures that were specified inside the Collada file still need 
to be downloaded and applied. But that would mean going through the entire Collada file again to look up the relative paths of 
the texture locations. To prevent this, a dirty trick has been used:
When the model is loaded, materials are applied and when a material should use a textures, a dummy texture of 1 x 1 pixel is 
created. The name of this texture is then set to match the path. So after loading the model, all you have to do is iterate 
through the materials, see if they have a temporary texture and replace that with a new texture that you can download or 
retrieve from elsewhere.

Have a look inside SimpleColladaDemoCtrl.cs for an example of how this could be implemented. 
The function 
	private IEnumerator DownloadAndImportFile(string url, Quaternion rotate, Vector3 scale, Vector3 translate)
shows an example of how to download and import a collada file as a background process.
It also calls
	private IEnumerator DownloadTextures(GameObject go, string originalUrl)
This function downloads and applies the textures that were specified in the collada file.


C# and Javascript
-----------------
If you want to create a Javascript that uses the SimpleCollada package, you will have to place the scripts in the "Standard Assets", "Pro Standard Assets" or "Plugins" folder and your Javascripts outside of these folders. The code inside the "Standard Assets", "Pro Standard Assets" or "Plugins" is compiled first and the code outside is compiled in a later step making the Types defined in the compilation step (the C# scripts) available to later compilation steps (your Javascript scripts).


