SimpleXML
---------
Import any XML string into Unity at runtime. The results are formatted as a hierarchy of plain Hashtables and ArrayLists.

No fancy stuff, no nonsense, just an Importer and a few extensions to the existing C# Hashtable and ArrayList classes. And it doesn't start throwing exceptions the second your XML isn't 100% according to standards either.

Functions included are:
- import XML
- import only a subsection (by tag)
- export as XML or JSON
- find node at path
- find node by property value


Documentation
-------------
The full documentation is available online at http://orbcreation.com/orbcreation/docu.orb?1008


Package Contents
----------------
SimpleXML / ReadMe.txt   (this file)
SimpleXML / Documentation.txt   (documentation in plain text format better use http://orbcreation.com/orbcreation/docu.orb?1008)
SimpleXML / SimpleXmlImporter.cs   (the XML importer)
SimpleXML / OrbCreationExtensions   (extensions to default classes that are also used in other OrbCreation packages)
SimpleXML / Demo / Demo.unity   (the demo scene)
SimpleXML / Demo / Background.png (background image for demo)
SimpleXML / Demo / DemoCtrl.cs (script that runs the demo)
SimpleXML / Example Procedural Mini Town  (very basic example of creating game objects at runtime based on an XML file)
SimpleXML / SampleXmlFiles / BookCatalog.xml  (sample book catalog xml file that can also be found on our website)
SimpleXML / SampleXmlFiles / CdCatalog.xml  (sample cd catalog xml file that can also be found on our website)
SimpleXML / SampleXmlFiles / ColladaModel.xml  (sample collada xml file that can also be found on our website)


Minimal needed in your project
------------------------------
SimpleXmlImporter.cs
OrbCreationExtensions folder
All the rest can go.

To use the OrbCreationExtensions you need to add the following line to the script that use it:
	using OrbCreationExtensions;


Using multiple Orbcreation packages
-----------------------------------
When you also use other Orbcreation AssetStore packages in your project, chances are that they too will contain a copy of 
the folder OrbCreationExtensions. It is best to merge those folders together into 1, always using the latest version of 
files inside the folder. You can check the versions by opening the files inside the OrbCreationExtensions folder.


C# and Javascript
-----------------
If you want to create a Javascript that uses the SimpleXML package, you will have to place the scripts in the "Standard Assets", "Pro Standard Assets" or "Plugins" folder and your Javascripts outside of these folders. The code inside the "Standard Assets", "Pro Standard Assets" or "Plugins" is compiled first and the code outside is compiled in a later step making the Types defined in the compilation step (the C# scripts) available to later compilation steps (your Javascript scripts).


