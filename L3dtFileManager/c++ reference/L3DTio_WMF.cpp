/*---------------------------------------------------------------------------------
|     Project | L3DTio_WMF 
| Description | A plugin for L3DT to implement the water map file (WMF) format.
|      Author | Aaron Torpy (aaron@bundysoft.com)
|   Copyright | (C) Copyright 2006-2011 Aaron Torpy. All rights reserved.
|         URL | http://www.bundysoft.com/wiki/doku.php?id=plugins:fileio:L3DTio_WMF
\---------------------------------------------------------------------------------*/

#include "stdafx.h"
#include "L3DTio_WMF.h"

#include <Zeolite.h>

#include <zproj.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CL3DTio_WMFApp

BEGIN_MESSAGE_MAP(CL3DTio_WMFApp, CWinApp)
END_MESSAGE_MAP()


// CL3DTio_WMFApp construction

CL3DTio_WMFApp::CL3DTio_WMFApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}


// The one and only CL3DTio_WMFApp object

CL3DTio_WMFApp theApp;


// CL3DTio_WMFApp initialization

BOOL CL3DTio_WMFApp::InitInstance()
{
	CWinApp::InitInstance();

	return TRUE;
}

zeoexport bool __stdcall zeoInitPlugin() {

	//
	// add any further initialisation here
	//

	ZFORMAT hFormat;

	// water map
	hFormat = zformat_Create("Water map", "L3DT Water Map File", "wmf", "L3DT");
	if(!hFormat) {
		// do nothing; this probably means the format already exists.
	} else {

		// set the flags
		zformat_SetFlags(hFormat, ZFORMAT_FLAG_NATIVE|ZFORMAT_FLAG_MOSAIC|ZFORMAT_FLAG_LOAD|ZFORMAT_FLAG_SAVE|ZFORMAT_FLAG_LOADTILE|ZFORMAT_FLAG_SAVETILE); // native I/O and mosaic

		bool TempBool = true;
		zformat_SetOptionValue(hFormat, "UseHeightfieldScale", VarID_bool, &TempBool);
	}
	hFormat = zformat_CreateGeneric(MAP_WaterMap, "L3DT Water Map File", "wmf", "L3DT");
	if(!hFormat) {
		// do nothing; this probably means the format already exists.
	} else {

		// set the flags
		zformat_SetFlags(hFormat, ZFORMAT_FLAG_NATIVE|ZFORMAT_FLAG_MOSAIC|ZFORMAT_FLAG_LOAD|ZFORMAT_FLAG_SAVE|ZFORMAT_FLAG_LOADTILE|ZFORMAT_FLAG_SAVETILE); // native I/O and mosaic
	}

	// auxiliary water map (as generic format)
	hFormat = zformat_CreateGeneric(MAP_AuxWaterMap, "L3DT Water Map File", "wmf", "L3DT");
	if(!hFormat) {
		// do nothing; this probably means the format already exists.
	} else {

		// set the flags
		zformat_SetFlags(hFormat, ZFORMAT_FLAG_NATIVE|ZFORMAT_FLAG_MOSAIC|ZFORMAT_FLAG_LOAD|ZFORMAT_FLAG_SAVE|ZFORMAT_FLAG_LOADTILE|ZFORMAT_FLAG_SAVETILE); // native I/O and mosaic

		bool TempBool = false;
		zformat_SetOptionValue(hFormat, "UseHeightfieldScale", VarID_bool, &TempBool);
	}

	return true;
}

zeoexport bool __stdcall zeoSaveMapFile(ZMAP hMap, LPCSTR lpFileName, ZFORMAT hFormat, ZVAR hProgWnd) {
	
	struct _iobuf *fh;
	int i, j;
	int TempInt;
	float HFmin, HFmax;
	WaterMapPixel WMpxl;
	USHORT TempUSHORT;

	// check the map type
	switch(zmap_GetMapType(hMap)) {
	default:
		zeoReportError("L3DTio_WMF::SaveMapFile error:\r\n - unsupported map type");
		return false;
	//case MAP_AuxWaterMap:
	case MAP_WaterMap:
		// OK
		break;
	}

	bool UseHFScale = false;
	zformat_GetOptionValue(hFormat, "UseHeightfieldScale", VarID_bool, &UseHFScale);
	
	if(UseHFScale) {
		// get the altitude range
		if(!zproj_GetHeightfieldRange(HFmin, HFmax)) {
			zeoReportError("L3DTio_WMF::SaveMapFile warning:\r\n - call to project_GetHeightfieldRange failed");

			UseHFScale = false;
			//return false;
		}
	}

	if(!UseHFScale) {
		// get the altitude range
		if(!zmap_GetMinMaxAlt(hMap, HFmin, HFmax)) {
			zeoReportError("L3DTio_WMF::SaveMapFile error:\r\n - call to map_GetMinMaxAlt failed");
			return false;
		}
	}

	long nx = zmap_GetWidth(hMap);
	long ny = zmap_GetHeight(hMap);

	if(nx<=0 || ny<=0) {
		zeoReportError("L3DTio_WMF::SaveMapFile error:\r\n - map not initialised");
		return false;
	}
	
	float maxval = max(fabs(HFmax), fabs(HFmin));
	if(maxval<=0) {
		zeoReportError("L3DTio_TERApp::SaveMapFile error:\r\n - invalid map contents");
		return false;
	}


	// opening file for writing
	fh=fopen(lpFileName, "wb"); 
	if(fh==NULL) {
		zeoReportError("L3DTio_WMF::SaveMapFile error:\r\n - cannot opem file for writing");
		return false;
	}

	// making file header
	FileHeader_WM600 FH_WM600;
	FH_WM600.DataOffset=64;	
	FH_WM600.nx = nx;
	FH_WM600.ny = ny;
	FH_WM600.Depth_DataSize = 2;
	FH_WM600.FloatFlag = false;
	FH_WM600.HorizScale = zmap_GetHorizScale(hMap);
	FH_WM600.TileSize = 0;
	FH_WM600.WrapFlag = zmap_GetWrapFlag(hMap);
	FH_WM600.RLE_Flag=false;
	FH_WM600.Aux_DataType = DEF_WM_AUX_STANDARD1;
	
	if(FH_WM600.FloatFlag) {
		switch(FH_WM600.Depth_DataSize) {
		case 4:
			FH_WM600.VertScale=1.0f;
			FH_WM600.VertOffset=0.0f;
			break;
		default:
			zeoReportError("L3DTio_WMF::SaveMapFile error\r\n - unacceptable data size in WM600 (float)");
			return false;
		}
	} else {
		switch(FH_WM600.Depth_DataSize) {
		case 2: // USHORT
			FH_WM600.VertScale=(float)(HFmax-HFmin)/65535.0f;
			FH_WM600.VertOffset=HFmin;
			break;
		default:
			zeoReportError("L3DTio_WMF::SaveMapFile error\r\n - unacceptable data size in WM600 (int)");
			return false;
		}
	}

	if(FH_WM600.TileSize>1) {
		if(FH_WM600.nx%FH_WM600.TileSize!=0 || FH_WM600.ny%FH_WM600.TileSize!=0) {
			zeoReportError("L3DTio_WMF::SaveMapFile error\r\n - tile size is incompatible with map size in WM600");
			return false;
		}
	}

	switch(FH_WM600.Aux_DataType) {
	case DEF_WM_AUX_STANDARD1:
		FH_WM600.Aux_DataSize=3;
		break;
	default:
		FH_WM600.Aux_DataType=0;
		FH_WM600.Aux_DataSize=0;
		break;
	}

	// opening file for writing
	fh=fopen(lpFileName, "wb"); 
	if(fh==NULL) {
		return false;
	}

	char TempBuf[16];
	USHORT FileVersionNo = DEF_FILE_WM600;

	// writing header
	sprintf(TempBuf, "L3DT");
	fwrite(TempBuf, 4, 1, fh);
	fwrite((void*)&FileVersionNo, 2, 1, fh);
	sprintf(TempBuf, "WMF_v1.0");
	fwrite(TempBuf, 8, 1, fh);
	fwrite(&FH_WM600.DataOffset, 2, 1, fh);
	fwrite(&FH_WM600.nx, 4, 1, fh);
	fwrite(&FH_WM600.ny, 4, 1, fh);
	fwrite(&FH_WM600.Depth_DataSize, 1, 1, fh);
	fwrite(&FH_WM600.FloatFlag, 1, 1, fh);
	fwrite(&FH_WM600.VertScale, 4, 1, fh);
	fwrite(&FH_WM600.VertOffset, 4, 1, fh);
	fwrite(&FH_WM600.HorizScale, 4, 1, fh);
	fwrite(&FH_WM600.TileSize, 2, 1, fh);
	fwrite(&FH_WM600.WrapFlag, 1, 1, fh);
	fwrite(&FH_WM600.RLE_Flag, 1, 1, fh);
	fwrite(&FH_WM600.Aux_DataType, 2, 1, fh);
	fwrite(&FH_WM600.Aux_DataSize, 1, 1, fh);
	TempBuf[0]=0;
	fwrite(TempBuf, 1, 19, fh);	// filling in reserved junk


	// preparing progress bar
	__int64 p, q, pmax, qstep;
	MSG TempMSG;
	if(hProgWnd) {
		zprogbox_SetTitle(hProgWnd, "Saving WMF");
		zprogbox_ShowWnd(hProgWnd, 0);
		pmax = (__int64)nx * (__int64)ny;
		qstep = (__int64)pmax/50; // HACK!
		if(qstep<1)	qstep=1;
		else 		pmax=50;
		q=0, p=0;
	}


	bool ErrorFlag = false;
	int TileSize = FH_WM600.TileSize;
	if(TileSize<1)	TileSize = 1;
	int ntx = nx/TileSize;
	int nty = ny/TileSize;
	int tx, ty;

	for(ty=0; ty<nty && !ErrorFlag; ty++) {
	for(tx=0; tx<ntx && !ErrorFlag; tx++) {
		for(j=0; j<TileSize && !ErrorFlag; j++) {
		for(i=0; i<TileSize && !ErrorFlag; i++) {

	
			if(hProgWnd) {
				if(q==qstep)	{ p++; q=0; }
				if(q++==0) {
					PeekMessage(&TempMSG, NULL, 0, 0, PM_REMOVE);
					zprogbox_SetProgress(hProgWnd, p, pmax);
				}
			}

			if(!zmap_GetPixel(hMap, i+tx*TileSize, j+ty*TileSize, &WMpxl)) {
				zeoReportError("L3DTio_WMF::SaveMapFile error\n - GetPixel returned false");
				ErrorFlag = true;
			}

			// writing water-level data
			switch(FH_WM600.Depth_DataSize) {
			case 4:
				if(FH_WM600.FloatFlag) {
					if (WMpxl.Type == 0)
					{
						WMpxl.WaterLevel = -1E10;
					}
					fwrite((void*)&(WMpxl.WaterLevel), 4, 1, fh);
				} else {
					zeoReportError("L3DTio_WMF::SaveMapFile error\n - unacceptable data size in WM600 (tiled, non-RLE float)");
					ErrorFlag = true;
				}
				break;
			case 2:
				// interpreting water-level as USHORT
				if (WMpxl.Type != 0){
						TempInt=(int)((WMpxl.WaterLevel-FH_WM600.VertOffset)/FH_WM600.VertScale);
				}
				else{
					TempInt = 0;
				}
				if (TempInt<0){
					TempInt = 0;
				}
				if (TempInt>65535){
					TempInt = 65535;
				}
				TempUSHORT=(USHORT)TempInt;
				fwrite((void*)&TempUSHORT, 2, 1, fh);
				break;
			default:
				break;
			}

			// writing auxilary data
			switch(FH_WM600.Aux_DataType) {
			case DEF_WM_AUX_STANDARD1:
				fwrite(&WMpxl.Type, 1, 1, fh);
				fwrite(&WMpxl.WaterID, 2, 1, fh);
				break;
			default:
				break;
			}
		}
		}
	}
	}

	if(hProgWnd) {
		zprogbox_HideWnd(hProgWnd);
	}

	fclose(fh);

	if(ErrorFlag) {
		zeoReportError("L3DTio_WMF::SaveMapFile error:\r\n - Save terminated following errors");
		return false;
	}

	return true;

}

zeoexport bool __stdcall zeoLoadMapFile(ZMAP hMap, LPCSTR lpFileName, long MapTypeID, ZFORMAT hFormat, ZVAR hProgWnd) {
	
	switch(MapTypeID) {
	case MAP_WaterMap:
		break;
	default:
		zeoReportError("L3DTio_WMF::LoadMapFile error:\r\n - Unsupported map type");
		return false;
	}

	struct _iobuf *fh;
	int i, j, tx, ty;
	char TempBuf[8];
	USHORT TempUSI;
	float TempFloat;
//	UINT TempUINT;
	CString TempStr;

	WaterMapPixel WMpxl;

	USHORT FileVersionNo;
	char* CharBuf;

	// opening file for reading
	fh=fopen(lpFileName, "rb"); 
	if(fh==NULL) {
		TempStr.Format("File '%s' not found", lpFileName);
		zeoReportError(TempStr);
		return false;
	}

	// interpreting header
	fread(TempBuf, 4, 1, fh);
	TempBuf[4]='\0';
	if(strcmp(TempBuf, "L3DT")!=0) {
		zeoReportError("L3DTio_WMF::LoadMapFile error\r\n - invalid file header");
		fclose(fh);
		return false;
	}

	fread(&FileVersionNo, 2, 1, fh);
	if(FileVersionNo!=DEF_FILE_WM600) {
		TempStr.Format("L3DTio_WMF::LoadMapFile error\r\n - unknown file version number (%d)", FileVersionNo);
		zeoReportError(TempStr);
		fclose(fh);
		return false;
	}

	// reading ASCII external file version number
	char FileVersionCharBuf[256];
	fread(FileVersionCharBuf, 8, 1, fh);
	FileVersionCharBuf[8]='\0';	// null-terminating string

	if(strcmp(FileVersionCharBuf, "WMF_v1.0")!=0) {
		TempStr.Format("L3DTio_WMF::LoadMapFile error\r\n - unknown file version '%s'", FileVersionCharBuf);
		zeoReportError(TempStr);
		fclose(fh);
		return false;
	}

	FileHeader_WM600 FH_WM600;

	fread(&FH_WM600.DataOffset, 2, 1, fh);

	if(FH_WM600.DataOffset<64) {
		zeoReportError("L3DTio_WMF::LoadMapFile error\n - data offset too short in WM600");
		fclose(fh);
		return false;
	}

	// reading remainder of header into char buffer
	CharBuf=(char*)calloc(1, FH_WM600.DataOffset-16);
	fread(CharBuf, FH_WM600.DataOffset-16, 1, fh);

	memcpy(&FH_WM600.nx, CharBuf, 4);
	memcpy(&FH_WM600.ny, CharBuf+4, 4);
	memcpy(&FH_WM600.Depth_DataSize, CharBuf+8, 1);
	memcpy(&FH_WM600.FloatFlag, CharBuf+9, 1);
	memcpy(&FH_WM600.VertScale, CharBuf+10, 4);
	memcpy(&FH_WM600.VertOffset, CharBuf+14, 4);
	memcpy(&FH_WM600.HorizScale, CharBuf+18, 4);
	memcpy(&FH_WM600.TileSize, CharBuf+22, 2);
	memcpy(&FH_WM600.WrapFlag, CharBuf+24, 1);
	memcpy(&FH_WM600.RLE_Flag, CharBuf+25, 1);
	memcpy(&FH_WM600.Aux_DataType, CharBuf+26, 2);
	memcpy(&FH_WM600.Aux_DataSize, CharBuf+28, 1);

	free(CharBuf);

	if(FH_WM600.TileSize>1) {
		if(FH_WM600.nx%FH_WM600.TileSize!=0 || FH_WM600.ny%FH_WM600.TileSize!=0) {
			zeoReportError("L3DTio_WMF::LoadMapFile error\n - tile size is incompatible with map size in WM600");
			fclose(fh);
			return false;
		}
	}

	if(FH_WM600.VertScale<=0.0) {
		zeoReportError("L3DTio_WMF::LoadMapFile error\n - vertical scale is equal to or smaller than zero");
		fclose(fh);
		return false;
	}

	switch(FH_WM600.Depth_DataSize) {
	case 4:
		if(!FH_WM600.FloatFlag) {
			zeoReportError("L3DTio_WMF::LoadMapFile error\n - unacceptable data size in WM600 (float)");
			return false;
		}
		break;
	case 2:
		// that's OK
		break;
	case 0:
		// that's OK
		break;
	default:
		zeoReportError("L3DTio_WMF::LoadMapFile error\n - unacceptable data size in WM600");
		return false;
	}

	// initialising map
	if(!zmap_Init(hMap, FH_WM600.nx, FH_WM600.ny, MapTypeID, FH_WM600.HorizScale, (bool)FH_WM600.WrapFlag)) {
		zeoReportError("L3DTio_WMF::LoadMapFile error\n - cannot initialise map");
		return false;
	}

	// preparing progress bar
	__int64 p, q, pmax, qstep;
	MSG TempMSG;
	if(hProgWnd) {
		zprogbox_SetTitle(hProgWnd, "Loading WMF");
		zprogbox_ShowWnd(hProgWnd, 0);
		pmax = FH_WM600.nx * FH_WM600.ny;
		qstep = (__int64)pmax/50; // HACK!
		if(qstep<1)	qstep=1;
		else 		pmax=50;
		q=0, p=0;
	}

	CharBuf=(char*)calloc(1, FH_WM600.Depth_DataSize+FH_WM600.Aux_DataSize);


	bool ErrorFlag = false;
	int TileSize = FH_WM600.TileSize;
	if(TileSize<1)	TileSize = 1;
	int ntx = FH_WM600.nx/TileSize;
	int nty = FH_WM600.ny/TileSize;

	for(ty=0; ty<nty && !ErrorFlag; ty++) {
	for(tx=0; tx<ntx && !ErrorFlag; tx++) {
		for(j=0; j<TileSize && !ErrorFlag; j++) {
		for(i=0; i<TileSize && !ErrorFlag; i++) {

			if(fread(CharBuf, FH_WM600.Depth_DataSize+FH_WM600.Aux_DataSize, 1, fh) == 0) {
				zeoReportError("L3DTio_WMF::LoadMapFile error\r\n - Error reading from file (file is incomlete)");
				ErrorFlag = true;
			}
			
			switch(FH_WM600.Depth_DataSize) {
			case 4:
				memcpy(&TempFloat, CharBuf, FH_WM600.Depth_DataSize);
				WMpxl.WaterLevel=TempFloat*FH_WM600.VertScale+FH_WM600.VertOffset;
				break;
			case 2:
				memcpy(&TempUSI, CharBuf, FH_WM600.Depth_DataSize);
				WMpxl.WaterLevel=(float)TempUSI*FH_WM600.VertScale+FH_WM600.VertOffset;
				break;
			default:
				break;
			}

			switch(FH_WM600.Aux_DataType) {
			case DEF_WM_AUX_STANDARD1:
				memcpy(&WMpxl.Type, CharBuf+FH_WM600.Depth_DataSize, 1);
				memcpy(&WMpxl.WaterID, CharBuf+FH_WM600.Depth_DataSize+1, 2);
				break;
			default:
				break;
			}

			if(hProgWnd) {
				if(q==qstep)	{ p++; q=0; }
				if(q++==0) {
					PeekMessage(&TempMSG, NULL, 0, 0, PM_REMOVE);
					zprogbox_SetProgress(hProgWnd, p, pmax);
				}
			}

			if(!zmap_SetPixel(hMap, i+tx*TileSize, j+ty*TileSize, &WMpxl)) {
				zeoReportError("L3DTio_WMF::LoadMapFile error\n - GetPixel returned false");
				ErrorFlag = true;
			}
		}
		}
	}
	}

	free(CharBuf);

	if(hProgWnd) {
		zprogbox_HideWnd(hProgWnd);
	}


	fclose(fh);

	if(ErrorFlag) {
		return false;
	}

	return true;
}

zeoexport bool __stdcall zeoSaveTileFile(ZMAP hMap, LPVOID hTile, LPCSTR lpFileName, ZFORMAT hFormat) {
	
	int MapType = zmap_GetMapType(hMap);

	// check the map type
	switch(MapType) {
	default:
		zeoReportError("L3DTio_WMF::SaveTileFile error:\r\n - unsupported map type");
		return false;
	case MAP_WaterMap:
	case MAP_AuxWaterMap:
		// OK
		break;
	}
	
	// get the altitude range
	float HFmax, HFmin;
	if(!zmap_tile_GetMinMaxAlt(hTile, HFmin, HFmax)) {
		zeoReportError("L3DTio_WMF::SaveTileFile error:\r\n - call to tile_GetMinMaxAlt failed");
		return false;
	}

	long nx, ny;
	nx = ny = zmap_GetTileSize(hMap);

	if(nx<=0 || ny<=0) {
		zeoReportError("L3DTio_WMF::SaveTileFile error:\r\n - invalid tile size");
		return false;
	}

	float maxval = (float)max(fabs(HFmin), fabs(HFmax));

	struct _iobuf *fh;
	char TempBuf[64];
	USHORT TempUSHORT;
	int TempInt;
	float Level;
	WaterMapPixel WMpxl;
	AuxWaterMapPixel TWMpxl;

	
	// making file header
	FileHeader_WM600 FH_WM600;
	FH_WM600.DataOffset=64;	
	FH_WM600.nx = nx;
	FH_WM600.ny = ny;
	FH_WM600.Depth_DataSize=2;
	FH_WM600.FloatFlag=false;
	FH_WM600.HorizScale = zmap_GetHorizScale(hMap);
	FH_WM600.TileSize=0;
	FH_WM600.WrapFlag=0;
	FH_WM600.RLE_Flag=0;

	switch(MapType) {
	case MAP_WaterMap:		FH_WM600.Aux_DataType=DEF_WM_AUX_STANDARD1; break;
	case MAP_AuxWaterMap:	FH_WM600.Aux_DataType=DEF_WM_AUX_TESTDATA1; break;
	}
	
	if(FH_WM600.FloatFlag) {
		switch(FH_WM600.Depth_DataSize) {
		case 4:
			FH_WM600.VertScale=1.0f;
			FH_WM600.VertOffset=0.0f;
			break;
		default:
			zeoReportError("L3DTio_WMF::SaveTileFile error\r\n - unacceptable data size (float)");
			return false;
		}
	} else {
		switch(FH_WM600.Depth_DataSize) {
		case 2: // USHORT
			FH_WM600.VertScale=(float)(HFmax-HFmin)/65535.0f;
			FH_WM600.VertOffset=HFmin;
			break;
		default:
			zeoReportError("L3DTio_WMF::SaveTileFile error\r\n - unacceptable data size  (int)");
			return false;
		}
	}

	if(FH_WM600.VertScale<=0) 
		FH_WM600.VertScale = 1;

	if(FH_WM600.TileSize>1) {
		if(FH_WM600.nx%FH_WM600.TileSize!=0 || FH_WM600.ny%FH_WM600.TileSize!=0) {
			zeoReportError("L3DTio_WMF::SaveTileFile error\r\n - tile size is incompatible with map size");
			return false;
		}
	}

	switch(FH_WM600.Aux_DataType) {
	case DEF_WM_AUX_STANDARD1:
		FH_WM600.Aux_DataSize=3;
		break;
	case DEF_WM_AUX_TESTDATA1:
		FH_WM600.Aux_DataSize=2;
		break;
	default:
		FH_WM600.Aux_DataType=0;
		FH_WM600.Aux_DataSize=0;
		break;
	}

	// opening file for writing
	fh = fopen(lpFileName, "wb");
	if(!fh) {
		zeoReportError("L3DTio_WMFApp::SaveTileFile error\r\n - cannot write to file");
		return false;
	}
	
	// writing header
	sprintf(TempBuf, "L3DT");
	fwrite(TempBuf, 4, 1, fh);
	USHORT FileVersionNo = DEF_FILE_WM600;
	fwrite((void*)&FileVersionNo, 2, 1, fh);
	sprintf(TempBuf, "WMF_v1.0");
	fwrite(TempBuf, 8, 1, fh);
	fwrite(&FH_WM600.DataOffset, 2, 1, fh);
	fwrite(&FH_WM600.nx, 4, 1, fh);
	fwrite(&FH_WM600.ny, 4, 1, fh);
	fwrite(&FH_WM600.Depth_DataSize, 1, 1, fh);
	fwrite(&FH_WM600.FloatFlag, 1, 1, fh);
	fwrite(&FH_WM600.VertScale, 4, 1, fh);
	fwrite(&FH_WM600.VertOffset, 4, 1, fh);
	fwrite(&FH_WM600.HorizScale, 4, 1, fh);
	fwrite(&FH_WM600.TileSize, 2, 1, fh);
	fwrite(&FH_WM600.WrapFlag, 1, 1, fh);
	fwrite(&FH_WM600.RLE_Flag, 1, 1, fh); // no longer used
	fwrite(&FH_WM600.Aux_DataType, 2, 1, fh);
	fwrite(&FH_WM600.Aux_DataSize, 1, 1, fh);
	TempBuf[0]=0;
	fwrite(TempBuf, 1, 19, fh);	// filling in reserved junk

	__int64 x, y;

	int i, j, ty, tx;

	int TileSize = FH_WM600.TileSize;
	if(TileSize<1) TileSize = 1;
	
	BYTE Type;

	int ntx = nx/TileSize;
	int nty = ny/TileSize;
	bool ErrorFlag = false;

	for(ty=0; ty<nty && !ErrorFlag; ty++) {
	for(tx=0; tx<ntx && !ErrorFlag; tx++) {
		for(j=0; j<TileSize && !ErrorFlag; j++) {
		for(i=0; i<TileSize && !ErrorFlag; i++) {

			x=i+tx*TileSize;
			y=j+ty*TileSize;
			
			switch(MapType) {
			case MAP_WaterMap:
				if(!zmap_tile_GetPixel(hTile, x, y, &WMpxl)) {
					zeoReportError("L3DTio_WMF::SaveTileFile error\r\n - GetPixel returned false (break 3)");
					ErrorFlag = true;
				}
				Type = WMpxl.Type;
				Level = WMpxl.WaterLevel;
				break;
			case MAP_AuxWaterMap:
				if(!zmap_tile_GetPixel(hTile, x, y, &TWMpxl)) {
					zeoReportError("L3DTio_WMF::SaveTileFile error\r\n - GetPixel returned false (break 4)");
					ErrorFlag = true;
				}
				Type = TWMpxl.Type;
				Level = TWMpxl.WaterLevel;
				break;
			}

			// writing water-level data
			switch(FH_WM600.Depth_DataSize) {
			case 4:
				if(FH_WM600.FloatFlag) {
					fwrite((void*)&Level, 4, 1, fh);
				} else {
					zeoReportError("L3DTio_WMF::SaveTileFile error\r\n - unacceptable data size (tiled, non-RLE float)");
					ErrorFlag = true;
				}
				break;
			case 2:
				// interpreting water-level as USHORT
				if(Type!=0)
						TempInt=(int)((Level-FH_WM600.VertOffset)/FH_WM600.VertScale);
				else	TempInt=0;
				if(TempInt<0)		TempInt=0;
				if(TempInt>65535)	TempInt=65535;
				TempUSHORT=(USHORT)TempInt;
				fwrite((void*)&TempUSHORT, 2, 1, fh);
				break;
			default:
				break;
			}

			// writing auxilary data
			switch(FH_WM600.Aux_DataType) {
			case DEF_WM_AUX_STANDARD1:
				fwrite(&WMpxl.Type, 1, 1, fh);
				fwrite(&WMpxl.WaterID, 2, 1, fh);
				break;
			case DEF_WM_AUX_TESTDATA1:
				fwrite(&TWMpxl.Type, 1, 1, fh);
				fwrite(&TWMpxl.DoneFlag, 1, 1, fh);
				break;
			default:
				break;
			}
		}
		}
	}
	}
	
	fclose(fh);

	if(ErrorFlag) {
		zeoReportError("L3DTio_WMF::SaveTileFile error:\r\n - Save terminated following errors");
		return false;
	}
	
	return true;
}

zeoexport bool __stdcall zeoLoadTileFile(ZMAP hMap, LPVOID hTile, LPCSTR lpFileName, ZFORMAT hFormat) {

	int MapType = zmap_GetMapType(hMap);
	int SubMapSize = zmap_GetTileSize(hMap);
	float HorizScale = zmap_GetHorizScale(hMap);

	switch(MapType) {
	case MAP_WaterMap:
	case MAP_AuxWaterMap:
		break;
	default:
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - Unsupported map type");
		return false;
	}

	struct _iobuf *fh;
	int k;
	char TempBuf[64];
	USHORT TempUSHORT;
	int TempInt;
	float Level, minlev, maxlev;
	WaterMapPixel WMpxl;
	AuxWaterMapPixel TWMpxl;
	CString TempStr;
	USHORT FileVersionNo;


	// opening file for reading
	fh=fopen(lpFileName, "rb"); 
	if(fh==NULL) {
		TempStr.Format("L3DTio_WMF::LoadTileFile error\r\n - File '%s' not found fo reading", lpFileName);
		zeoReportError(TempStr);
		return false;
	}

	// interpreting header
	fread(TempBuf, 4, 1, fh);
	TempBuf[4]='\0';
	if(strcmp(TempBuf, "L3DT")!=0) {
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - invalid file header");
		fclose(fh);
		return false;
	}

	fread(&FileVersionNo, 2, 1, fh);
	if(FileVersionNo!=DEF_FILE_WM600) {
		TempStr.Format("L3DTio_WMF::LoadTileFile error\r\n - unknown file version number (%d)", FileVersionNo);
		zeoReportError(TempStr);
		fclose(fh);
		return false;
	}

	// reading ASCII external file version number
	char FileVersionCharBuf[256];
	fread(FileVersionCharBuf, 8, 1, fh);
	FileVersionCharBuf[8]='\0';	// null-terminating string

	if(strcmp(FileVersionCharBuf, "WMF_v1.0")!=0) {
		TempStr.Format("L3DTio_WMF::LoadTileFile error\r\n - unknown file version '%s'", FileVersionCharBuf);
		zeoReportError(TempStr);
		fclose(fh);
		return false;
	}

	FileHeader_WM600 FH_WM600;

	fread(&FH_WM600.DataOffset, 2, 1, fh);

	if(FH_WM600.DataOffset<64) {
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - data offset too short");
		fclose(fh);
		return false;
	}

	// reading remainder of header into char buffer
	char* CharBuf;
	CharBuf=(char*)calloc(1, FH_WM600.DataOffset-16);
	fread(CharBuf, FH_WM600.DataOffset-16, 1, fh);

	memcpy(&FH_WM600.nx, CharBuf, 4);
	memcpy(&FH_WM600.ny, CharBuf+4, 4);
	memcpy(&FH_WM600.Depth_DataSize, CharBuf+8, 1);
	memcpy(&FH_WM600.FloatFlag, CharBuf+9, 1);
	memcpy(&FH_WM600.VertScale, CharBuf+10, 4);
	memcpy(&FH_WM600.VertOffset, CharBuf+14, 4);
	memcpy(&FH_WM600.HorizScale, CharBuf+18, 4);
	memcpy(&FH_WM600.TileSize, CharBuf+22, 2);
	memcpy(&FH_WM600.WrapFlag, CharBuf+24, 1);
	memcpy(&FH_WM600.RLE_Flag, CharBuf+25, 1);
	memcpy(&FH_WM600.Aux_DataType, CharBuf+26, 2);
	memcpy(&FH_WM600.Aux_DataSize, CharBuf+28, 1);
	free(CharBuf);

	if(FH_WM600.RLE_Flag) {
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - RLE flag not supported");
		fclose(fh);
		return false;
	}

	// checking size
	if(FH_WM600.nx!=(UINT)SubMapSize||FH_WM600.ny!=(UINT)SubMapSize) {
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - file size / mosaic sub map size mismatch");
		fclose(fh);
		return false;
	}

	if(FH_WM600.TileSize>1) {
		if(FH_WM600.nx%FH_WM600.TileSize!=0 || FH_WM600.ny%FH_WM600.TileSize!=0) {
			zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - file size is incompatible with map size");
			fclose(fh);
			return false;
		}
	}

	if(FH_WM600.VertScale<=0.0) {
		zeoReportError("L3DTio_WMF::LoadTileFile warning\r\n - vertical scale is equal to or smaller than zero (set to 1)");
		FH_WM600.VertScale = 1;
	}

	if(FH_WM600.HorizScale!=HorizScale) {
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - mismatch in horiz scale (ignoring file value)");
	}

	switch(FH_WM600.Depth_DataSize) {
	case 4:
		if(!FH_WM600.FloatFlag) {
			zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - unacceptable data size  (float)");
			fclose(fh);
			return false;
		}
		break;
	case 2:
		// that's OK
		break;
	case 0:
		// that's OK
		break;
	default:
		zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - unacceptable data size");
		fclose(fh);
		return false;
	}

	CharBuf=(char*)calloc(1, FH_WM600.Depth_DataSize+FH_WM600.Aux_DataSize);

	int ty, tx, i, j;

	int TileSize = FH_WM600.TileSize;
	if(TileSize<=0)	TileSize = 1;

	int ntx = SubMapSize/TileSize;
	int nty = SubMapSize/TileSize;

	bool ErrorFlag = false;

	int x, y;
		
	for(ty=0; ty<nty && !ErrorFlag; ty++) {
	for(tx=0; tx<ntx && !ErrorFlag; tx++) {

		for(j=0; j<TileSize && !ErrorFlag; j++) {
		for(i=0; i<TileSize && !ErrorFlag; i++) {

			x=i+tx*TileSize;
			y=j+ty*TileSize;

            if(fread(CharBuf, FH_WM600.Depth_DataSize+FH_WM600.Aux_DataSize, 1, fh) == 0) {
				zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - file is incomplete");
				ErrorFlag = true;
			}
			
			switch(FH_WM600.Depth_DataSize) {
			case 4:
				memcpy(&Level, CharBuf, FH_WM600.Depth_DataSize);
				Level=Level*FH_WM600.VertScale+FH_WM600.VertOffset;
				break;
			case 2:
				memcpy(&TempUSHORT, CharBuf, FH_WM600.Depth_DataSize);
				Level=(float)TempUSHORT*FH_WM600.VertScale+FH_WM600.VertOffset;
				break;
			default:
				break;
			}

			switch(FH_WM600.Aux_DataType) {
			case DEF_WM_AUX_STANDARD1:
				memcpy(&WMpxl.Type, CharBuf+FH_WM600.Depth_DataSize, 1);
				memcpy(&WMpxl.WaterID, CharBuf+FH_WM600.Depth_DataSize+1, 2);
				break;
			case DEF_WM_AUX_TESTDATA1:
				memcpy(&TWMpxl.Type, CharBuf+FH_WM600.Depth_DataSize, 1);
				memcpy(&TWMpxl.DoneFlag, CharBuf+FH_WM600.Depth_DataSize+1, 1);
				break;
			default:
				break;
			}

			switch(MapType) {
			case MAP_WaterMap:
				WMpxl.WaterLevel = Level;
				if(!zmap_tile_SetPixel(hTile, x, y, &WMpxl)) {
					zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - cannot set pixel value");
					ErrorFlag = true;
				}
				break;
			case MAP_AuxWaterMap:
				TWMpxl.WaterLevel = Level;
				if(!zmap_tile_SetPixel(hTile, x, y, &TWMpxl)) {
					zeoReportError("L3DTio_WMF::LoadTileFile error\r\n - cannot set pixel value");
					ErrorFlag = true;
				}
				break;
			}
		}
		}
	}
	}

	free(CharBuf);

	fclose(fh);

	if(ErrorFlag) {
		return false;
	}

	return true;
}
