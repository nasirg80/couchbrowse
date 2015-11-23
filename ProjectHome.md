A basic C# GUI front-end for CouchDB. The main purpose of this project is as a simple interactive testbed for SharpCouch, which is a basic library for accessing CouchDB from C#. This library is included within the main project.

For a 'real' GUI front-end, use the Futon browser client which is now built in to CouchDB.

This initial release builds with SharpDevelop 2.2 on Windows under .NET framework 2. It should work with Visual Studio 2005 and MonoDevelop.

The project depends on the excellent LitJson library for parsing and constructing Json data. For convenience, the relevant subset of the LitJson source is included.