/**************************************************************
*                                                             *
*   Example code for loading an L3DT heightfield file (HFF)   *
*                                                             *
*   Author: Aaron Torpy                                       *
*   E-mail: aaron_torpy@yahoo.com                             *
*   Date:   19th of August, 2005                              *
*                                                             *
**************************************************************/

/*

IMFORMATION:

This file includes some C++ code for loading a heightfield file (.hff).
The code is an excerpt from the L3DT source code. 

I have not tried to compile this code after chopping it out of L3DT, so it 
is possible that I've added some typographic errors or bugs.

This file should be considered as a guide, as it does not implement all the
classes to which it refers, nor all the function calls. These are left as an 
exercise for the reader.

COPYRIGHT:

This contents of file are in the public domain, and may be used for whatever
purpose you desire. Acknowledgement of the author (see above) in any works
derived from this code is appreciated but not required.

DISCLAIMER:

This code is provided 'as is', with no guarantee that is bug-free or safe
to use. The author will not be liable for any damages arising from the use of
this code or parts thereof.

MAP CLASS:

The function outlined below assume that you've declared a class called
CMapWrap (for want of a better name) that wraps up the float-map into
something more user-friendly.

CMapWrap is assumed to have the following methods:

void  InitMap(int width, int height, float HorizScale, bool WrapFlag);	// allocates the map array
void  FreeMap();						// de-allocates the memory
int   nx();								// gets the map width  (shorthand for 'number of pixels in x direction')
int   ny();								// gets the map height
float SetAlt(int x, int y, float Alt);	// sets the altitude at point (x, y) to 'alt'
*/



// ID number of HFF v1.0
#define DEF_FILE_HF300		300	// generalised HF file format

// header struct of HFF v1.0
typedef struct {
	USHORT DataOffset; // address of 1st data pixel
	UINT nx; // width
	UINT ny; // height
	BYTE DataSize;	// bytes per pixel
	BYTE FloatFlag;
	float VertScale;
	float VertOffset;
	float HorizScale;
	USHORT TileSize;
	BYTE WrapFlag;
} FileHeader_HF300;

// example code for loading HFF v1.0
bool LoadHFF(LPCTSTR FileName, CMapWrap* pMap) {
	
	struct _iobuf *fh;
	int i, j, tx, ty, i2, j2;
	char TempBuf[8];
	USHORT TempUSI;
	float TempFloat, val;
	CString TempStr; // note MFC-specific string class
	BYTE TempByte;
	char FileVersionCharBuf[9];
	char* pCharBuf;
	USHORT FileVersionNo;

	// opening file for reading (binary)
	fh=fopen(FileName, "rb"); 
	if(fh==NULL) {
		TempStr.Format("LoadHFF error:\r\n - File '%s' not found", FileName);
		ReportError(TempStr);
		return false;
	}

	// interpreting header
	fread(TempBuf, 4, 1, fh);
	TempBuf[4]='\0'; // forcing null termination
	if(strcmp(TempBuf, "L3DT")!=0) {
		ReportError("LoadHFF error\n - file is not a valid HFF");
		fclose(fh);
		return false;
	}

	// read the file type identifier
	fread((char*)&FileVersionNo, 2, 1, fh);

	// check the binary file version
	if(FileVersionNo!=DEF_FILE_HF300) {
		ReportError("LoadHFF error\n - invalid file version number");
		fclose(fh);
		return false;
	}

	// reading ASCII external file version number
	fread(FileVersionCharBuf, 8, 1, fh);
	FileVersionCharBuf[8]='\0';	// null-terminating string

	// checking...
	if(strcmp(FileVersionCharBuf, "HFF_v1.0")!=0) {
		TempStr.Format("LoadHFF error\n - unknown file version '%s' in HFF", FileVersionCharBuf);
		ReportError(TempStr);
		fclose(fh);
		return false;
	}

	// declare a file header struct
	FileHeader_HF300 FH_HF300;

	// read the data offset
	fread(&FH_HF300.DataOffset, 2, 1, fh);

	// check the size of the header
	if(FH_HF300.DataOffset<64) {
		ReportError("LoadHFF error\n - header is incomplete");
		fclose(fh);
		return false;
	}

	// allocate buffer for remaining header
	if(!(pCharBuf=(char*)calloc(1, FH_HF300.DataOffset-16)) {
		ReportError("LoadHFF error\n - cannot allocate header buffer");
		fclose(fh);
		return false;
	}
	
	// reading header into char buffer
	fread(pCharBuf, FH_HF300.DataOffset-16, 1, fh);

	// copying from char buffer to struct members
	memcpy(&FH_HF300.nx, pCharBuf, 4);
	memcpy(&FH_HF300.ny, pCharBuf+4, 4);
	memcpy(&FH_HF300.DataSize, pCharBuf+8, 1);
	memcpy(&FH_HF300.FloatFlag, pCharBuf+9, 1);
	memcpy(&FH_HF300.VertScale, pCharBuf+10, 4);
	memcpy(&FH_HF300.VertOffset, pCharBuf+14, 4);
	memcpy(&FH_HF300.HorizScale, pCharBuf+18, 4);
	memcpy(&FH_HF300.TileSize, pCharBuf+22, 2);
	memcpy(&FH_HF300.WrapFlag, pCharBuf+24, 1);

	// release temp char buffer
	free(pCharBuf);

	// store the horizontal scale
//	HF_PixelSize = FH_HF300.HorizScale; // you don't need this (HF_PixelSize is a global in L3DT)

	// is the map tiled?
	if(FH_HF300.TileSize>1) {

		// if so, is the tile size compatible with the map size?
		if(FH_HF300.nx%FH_HF300.TileSize!=0 || FH_HF300.ny%FH_HF300.TileSize!=0) {
			ReportError("LoadHFF error\n - tile size is incompatible with map size in HF300");
			fclose(fh);
			return false;
		}
	}

	// check the vertical scale
	if(FH_HF300.VertScale<=0.0) {
		ReportError("LoadHFF error\n - vertical scale is equal to or smaller than zero");
		fclose(fh);
		return false;
	}

	// check the horizontal scale
	if(FH_HF300.HorizScale<=0.0) {
		ReportError("LoadHFF error\n - horizontal scale is equal to or smaller than zero");
		fclose(fh);
		return false;
	}

	// check the data type and data size
	if(FH_HF300.FloatFlag) {
		switch(FH_HF300.DataSize) {
		case 4:	// single-precision float is OK
			break;
		default:
			ReportError("LoadHFF error\n - unacceptable data size in HF300 (float)");
			fclose(fh);
			return false;
		}
	} else {
		switch(FH_HF300.DataSize) {
		case 1: // BYTE is OK
			break;
		case 2: // USHORT is OK
			break;
		default:
			ReportError("LoadHFF error\n - unacceptable data size in HF300 (int)");
			fclose(fh);
			return false;
		}
	}

	// if the map memory is already allocated, free it
	if(pMap->IsInit())	pMap->FreeMap();

	// initialising map (width, height, horizontal scale, wrapping flag)
	if(!pMap->InitMap(FH_HF300.nx, FH_HF300.ny, FH_HF300.HorizScale, (bool)FH_HF300.WrapFlag)) {
		ReportError("LoadHFF error:\r\n - InitMap failed");
		fclose(fh);
		return false;
	}
	
	
	int TileSize = FH_HF300.TileSize;
	if(TileSize<1)	TileSize = 1; // not tiled is same as one pixel per tile (latter gives more efficient loader)

	// stepping through tiles
	for(ty=0; ty<pMap->ny()/TileSize; ty++) {
	for(tx=0; tx<pMap->nx()/TileSize; tx++) {

		// stepping through pixels in each tile
		for(j=0; j<TileSize; j++) {
		for(i=0; i<TileSize; i++) {

			// converting from local tile coordinates to global map coordinates
			i2 = i+tx*TileSize;
			j2 = j+ty*TileSize;
			
			val = 0;

			// reading value from file
			if(FH_HF300.FloatFlag) {
				fread((void*)&TempFloat, 4, 1, fh);
				val = TempFloat * FH_HF300.VertScale + FH_HF300.VertOffset;
			} else {
				switch(FH_HF300.DataSize) {
				case 1:
					fread((void*)&TempByte, 1, 1, fh);
					val = (float)TempByte * FH_HF300.VertScale + FH_HF300.VertOffset;
					break;	
				case 2:
					fread((void*)&TempUSI, 2, 1, fh);
					val = (float)TempUSI * FH_HF300.VertScale + FH_HF300.VertOffset;
					break;
				default:
					ReportError("LoadHFF error\n - pixel size is invalid (tiled)");
					fclose(fh);
					return false;
				}
			}
			
			// set the value in the map
			pMap->SetAlt(i2, j2, val);
		}
		}
	}
	}

	// close the file
	fclose(fh);

	// set some map flags
	pMap->Flags.IsReady=true;
	pMap->Flags.IsModified=false;
	pMap->m_FileName = FileName;

	return true;
}