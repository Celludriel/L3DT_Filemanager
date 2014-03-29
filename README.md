L3DT_Filemanager
================

C# library to read and write L3DT binary files

This library was written to support the reading and writing of the L3DT binary files.  An example where this library is used can be found in the nwn2_terrain_importer plugin.  Following file formats are supported: DMF, AMF, AMF.GZ, HFF, HFZ, HF2, HF2.GZ and WMF.

There are Nunit unit tests that test all the loading and writing functionality.  Testfiles have also been provided.

Each file type has some convenience methods to help users in their development, although many more could probably be written.  It's currently very lean but feel free to enrich them with your own helpers that you deem useful.

This version is tested with :

Large 3D Terrain (L3DT)

Version: 11.11 Std release build 1 
Build date: 24-Nov-2011

Copyright 2003-2011 Aaron Torpy
http://www.bundysoft.com/L3DT/