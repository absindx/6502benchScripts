﻿//--------------------------------------------------
// 6502bench NES Palette Visualizer
//   Created based on RuntimeData/... visualizer.
//   Follow the original license. (Apache 2.0)
//   Copyright 2020 absindx
//--------------------------------------------------

// Original license
/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.ObjectModel;

using PluginCommon;

namespace RuntimeData.Nintendo{
	public class VisPalette : MarshalByRefObject, IPlugin, IPlugin_Visualizer{
		// IPlugin
		public string		Identifier{
			get{
				return "NES Palette Visualizer";
			}
		}
		private IApplication	mAppRef;
		private byte[]		mFileData;

		// Visualization identifiers; DO NOT change or projects that use them will break.
		private const string	VIS_GEN_BITMAP	= "pal-bitmap";

		private const string	P_OFFSET	= "offset";
		private const string	P_WIDTH		= "width";
		private const string	P_HEIGHT	= "height";
		private const string	P_SHOWINVALID	= "showinvalid";

		// Palette
		// Generated by NTSC NES palette generator
		// https://bisqwit.iki.fi/utils/nespalette.php
		private static int[]		NesPalette	= new int[]{
			0x525252, 0x011A51, 0x0F0F65, 0x230663, 0x36034B, 0x400426, 0x3F0904, 0x321300, 0x1F2000, 0x0B2A00, 0x002F00, 0x002E0A, 0x00262D, 0x000000, 0x000000, 0x000000,
			0xA0A0A0, 0x1E4A9D, 0x3837BC, 0x5828B8, 0x752194, 0x84235C, 0x822E24, 0x6F3F00, 0x515200, 0x316300, 0x1A6B05, 0x0E692E, 0x105C68, 0x000000, 0x000000, 0x000000,
			0xFEFFFF, 0x699EFC, 0x8987FF, 0xAE76FF, 0xCE6DF1, 0xE070B2, 0xDE7C70, 0xC8913E, 0xA6A725, 0x81BA28, 0x63C446, 0x54C17D, 0x56B3C0, 0x3C3C3C, 0x000000, 0x000000,
			0xFEFFFF, 0xBED6FD, 0xCCCCFF, 0xDDC4FF, 0xEAC0F9, 0xF2C1DF, 0xF1C7C2, 0xE8D0AA, 0xD9DA9D, 0xC9E29E, 0xBCE6AE, 0xB4E5C7, 0xB5DFE4, 0xA9A9A9, 0x000000, 0x000000,
		};

		// Visualization descriptors.
		private VisDescr[]	mDescriptors	= new VisDescr[]{
			new VisDescr(VIS_GEN_BITMAP, "Palette Bitmap", VisDescr.VisType.Bitmap,
				new VisParamDescr[]{
					new VisParamDescr("File offset (hex)",	P_OFFSET,	typeof(int),  0, 0x00FFFFFF, VisParamDescr.SpecialMode.Offset, 0),
					new VisParamDescr("Width",		P_WIDTH,	typeof(int),  1, 512, 0, 16),
					new VisParamDescr("Height",		P_HEIGHT,	typeof(int),  1, 512, 0,  2),
					new VisParamDescr("Show invalid color",	P_SHOWINVALID,	typeof(bool), 0, 0, 0, false),
				}
			)
		};

		// IPlugin
		public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans){
			this.mAppRef	= appRef;
			this.mFileData	= fileData;
		}

		// IPlugin
		public void Unprepare(){
			this.mAppRef	= null;
			this.mFileData	= null;
		}

		// IPlugin_Visualizer
		public VisDescr[] GetVisGenDescrs(){
			if(this.mFileData == null){
				return null;
			}
			return this.mDescriptors;
		}

		// IPlugin_Visualizer
		public IVisualization2d Generate2d(VisDescr descr, ReadOnlyDictionary<string, object> parms){
			switch(descr.Ident){
				case VIS_GEN_BITMAP:
					return this.GenerateBitmap(parms);
				default:
					this.mAppRef.ReportError("Unknown ident " + descr.Ident);
					return null;
			}
		}

		private IVisualization2d GenerateBitmap(ReadOnlyDictionary<string, object> parms){
			int	offset		= Util.GetFromObjDict(parms, P_OFFSET,		0);
			int	width		= Util.GetFromObjDict(parms, P_WIDTH,		1);
			int	height		= Util.GetFromObjDict(parms, P_HEIGHT,		1);
			bool	showInvalid	= Util.GetFromObjDict(parms, P_SHOWINVALID,	false);

			// Check parameters.
			int	lastAddress	= offset + width * height - 1;
			if((offset < 0) || (lastAddress >= this.mFileData.Length)){
				this.mAppRef.ReportError("Invalid parameter");
				return null;
			}

			// Set palette.
			PaletteBitmap	bitmap	= new PaletteBitmap((uint)width, (uint)height);
			bitmap.AddColor(0x00000000);				// Transparent
			for(int i=0; i<NesPalette.Length; i++){
				UInt32	color	= 0xFF000000U | (UInt32)NesPalette[i];
				bitmap.AddColor((int)color);	// Palette color (1 shift)
			}

			// Convert to pixels.
			for(uint y=0; y<(uint)height; y++){
				for(uint x=0; x<(uint)width; x++){
					uint	index	= y * (uint)width + x;

					byte	palette	= this.mFileData[offset + index];
					if(!showInvalid & ((int)palette > NesPalette.Length)){
						// Transparent invalid color.
						bitmap.SetPixel(x, y, 0);
						continue;
					}

					palette	= (byte)((palette % NesPalette.Length) + 1);
					bitmap.SetPixel(x, y, palette);
				}
			}

			return bitmap.Bitmap;
		}
	}

	// Class to manage palette bitmap.
	// Because duplicate colors cannot be registered.
	internal class PaletteBitmap{
		public VisBitmap8	Bitmap		{ get; private set; }
		public byte		PaletteCount	{ get; private set; }
		public byte		ColorCount	{ get; private set; }
		private int[]		paletteColor;
		private byte[]		paletteIndex;

		public PaletteBitmap(uint width, uint height){
			this.Bitmap		= new VisBitmap8((int)width, (int)height);
			this.PaletteCount	= 0;
			this.ColorCount		= 0;
			this.paletteColor	= new int[256];
			this.paletteIndex	= new byte[256];
		}
		public void AddColor(int color){
			this.paletteColor[this.PaletteCount]	= color;
			this.paletteIndex[this.ColorCount]	= this.PaletteCount;

			// Check for duplicate colors.
			bool	newColor	= true;
			for(int i=0; i<this.PaletteCount; i++){
				if(this.paletteColor[i] == color){
					// duplicate
					newColor	= false;
					this.paletteIndex[this.ColorCount]	= (byte)i;
					break;
				}
			}

			if(newColor){
				// add
				this.PaletteCount++;
				this.Bitmap.AddColor(color);
			}

			this.ColorCount++;
		}
		public void SetPixel(uint x, uint y, byte color){
			this.Bitmap.SetPixelIndex((int)x, (int)y, this.paletteIndex[color]);
		}
	}
}
