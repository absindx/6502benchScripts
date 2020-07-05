﻿//--------------------------------------------------
// 6502bench NES Tile Array Visualizer
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
	public class VisTile : MarshalByRefObject, IPlugin, IPlugin_Visualizer{
		// IPlugin
		public string		Identifier{
			get{
				return "NES Tile Array Visualizer";
			}
		}
		private IApplication	mAppRef;
		private byte[]		mFileData;

		// Visualization identifiers; DO NOT change or projects that use them will break.
		private const string	VIS_GEN_BITMAP	= "nes-tile-array";

		private const string	P_TILEOFFSET	= "tileoffset";
		private const string	P_WIDTH		= "width";
		private const string	P_HEIGHT	= "height";
		private const string	P_CHROFFSET	= "chroffset";
		private const string	P_CHIPOFFSET	= "chipoffset";
		private const string	P_CHRBANK1	= "cgrbank1";
		private const string	P_CHRBANK2	= "cgrbank2";
		private const string	P_CHRBANK3	= "cgrbank3";
		private const string	P_CHRBANK4	= "cgrbank4";
		private const string	P_PALETTE1	= "palette1";
		private const string	P_PALETTE2	= "palette2";
		private const string	P_PALETTE3	= "palette3";
		private const string	P_PALETTE4	= "palette4";
		private const string	P_VERTICAL	= "vertical";

		// Palette
		// Generated by NTSC NES palette generator
		// https://bisqwit.iki.fi/utils/nespalette.php
		private static int[]	NesPalette	= new int[]{
			0x525252, 0x011A51, 0x0F0F65, 0x230663, 0x36034B, 0x400426, 0x3F0904, 0x321300, 0x1F2000, 0x0B2A00, 0x002F00, 0x002E0A, 0x00262D, 0x000000, 0x000000, 0x000000,
			0xA0A0A0, 0x1E4A9D, 0x3837BC, 0x5828B8, 0x752194, 0x84235C, 0x822E24, 0x6F3F00, 0x515200, 0x316300, 0x1A6B05, 0x0E692E, 0x105C68, 0x000000, 0x000000, 0x000000,
			0xFEFFFF, 0x699EFC, 0x8987FF, 0xAE76FF, 0xCE6DF1, 0xE070B2, 0xDE7C70, 0xC8913E, 0xA6A725, 0x81BA28, 0x63C446, 0x54C17D, 0x56B3C0, 0x3C3C3C, 0x000000, 0x000000,
			0xFEFFFF, 0xBED6FD, 0xCCCCFF, 0xDDC4FF, 0xEAC0F9, 0xF2C1DF, 0xF1C7C2, 0xE8D0AA, 0xD9DA9D, 0xC9E29E, 0xBCE6AE, 0xB4E5C7, 0xB5DFE4, 0xA9A9A9, 0x000000, 0x000000,
		};

		// NES header
		private static byte[]	INesHeaderSignature	= new byte[]{
			0x4E, 0x45, 0x53, 0x1A
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

			// Auto detect CHR offset
			int	chrOffset	= 0x8000;
			bool	findINesHeader	= true;
			for(int i=0; i<INesHeaderSignature.Length; i++){
				if(this.mFileData[i] != INesHeaderSignature[i]){
					findINesHeader	= false;
					break;
				}
			}
			if(findINesHeader){
				chrOffset	= 0x10 + 0x4000 * this.mFileData[4];
			}

			// Visualization descriptors.
			return new VisDescr[]{
				new VisDescr(VIS_GEN_BITMAP, "NES Tile Array", VisDescr.VisType.Bitmap,
					new VisParamDescr[]{
						new VisParamDescr("Tile array file offset (hex)",	P_TILEOFFSET,	typeof(int),  0, 0x00FFFFFF, VisParamDescr.SpecialMode.Offset, 0),
						new VisParamDescr("Width tile",				P_WIDTH,	typeof(int),  1, 512, 0, 16),
						new VisParamDescr("Height tile",			P_HEIGHT,	typeof(int),  1, 512, 0, 16),
						new VisParamDescr("CHR area file offset (hex)",		P_CHROFFSET,	typeof(int),  0, 0x00FFFFFF, VisParamDescr.SpecialMode.Offset, chrOffset),
						new VisParamDescr("CHR chip offset",			P_CHIPOFFSET,	typeof(int),  -255, 255, 0, 0),
						new VisParamDescr("CHR bank 1 ($0000-$03FF)",		P_CHRBANK1,	typeof(int),  0, 512, 0, 0),
						new VisParamDescr("CHR bank 2 ($0400-$07FF)",		P_CHRBANK2,	typeof(int),  0, 512, 0, 1),
						new VisParamDescr("CHR bank 3 ($0800-$0BFF)",		P_CHRBANK3,	typeof(int),  0, 512, 0, 2),
						new VisParamDescr("CHR bank 4 ($0C00-$0FFF)",		P_CHRBANK4,	typeof(int),  0, 512, 0, 3),
						new VisParamDescr("Palette 1",				P_PALETTE1,	typeof(int), -1, NesPalette.Length-1, 0, 0x0F),
						new VisParamDescr("Palette 2",				P_PALETTE2,	typeof(int),  0, NesPalette.Length-1, 0, 0x30),
						new VisParamDescr("Palette 3",				P_PALETTE3,	typeof(int),  0, NesPalette.Length-1, 0, 0x10),
						new VisParamDescr("Palette 4",				P_PALETTE4,	typeof(int),  0, NesPalette.Length-1, 0, 0x00),
						new VisParamDescr("Vertical",				P_VERTICAL,	typeof(bool),  0, 0, 0, false),
					}
				)
			};
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
			int	tileoffset	= Util.GetFromObjDict(parms, P_TILEOFFSET,	0);
			int	width		= Util.GetFromObjDict(parms, P_WIDTH,		1);
			int	height		= Util.GetFromObjDict(parms, P_HEIGHT,		1);
			int	chroffset	= Util.GetFromObjDict(parms, P_CHROFFSET,	0);
			int	chipoffset	= Util.GetFromObjDict(parms, P_CHIPOFFSET,	0);
			int	chrbank1	= Util.GetFromObjDict(parms, P_CHRBANK1,	0);
			int	chrbank2	= Util.GetFromObjDict(parms, P_CHRBANK2,	1);
			int	chrbank3	= Util.GetFromObjDict(parms, P_CHRBANK3,	2);
			int	chrbank4	= Util.GetFromObjDict(parms, P_CHRBANK4,	3);
			int	palette1	= Util.GetFromObjDict(parms, P_PALETTE1,	0x0F);
			int	palette2	= Util.GetFromObjDict(parms, P_PALETTE2,	0x00);
			int	palette3	= Util.GetFromObjDict(parms, P_PALETTE3,	0x10);
			int	palette4	= Util.GetFromObjDict(parms, P_PALETTE4,	0x30);
			bool	vertical	= Util.GetFromObjDict(parms, P_VERTICAL,	false);
			int[]	bank		= new int[4]{ chrbank1, chrbank2, chrbank3, chrbank4 };
			int[]	palette		= new int[4]{ palette1, palette2, palette3, palette4 };
			ChrMapper	mapper	= null;

			// Check parameters.
			try{
				{	// tile
					int	lastAddress	= tileoffset + width * height - 1;
					if((tileoffset < 0) || (lastAddress >= this.mFileData.Length)){
						this.mAppRef.ReportError("Invalid parameter");
						return null;
					}
				}
				{	// bank
					int	lastAddress	= chroffset + 0x03FF;	// chr = [0, 0, 0, 0] -> 0x1000 / 4 - 1 = 0x03FF
					if((chroffset < 0) || (lastAddress >= this.mFileData.Length)){
						this.mAppRef.ReportError("Invalid parameter");
						return null;
					}
				}
				// palette
				for(int i=0; i<palette.Length; i++){
					int	color	= palette[i];
					if(color >= NesPalette.Length){
						this.mAppRef.ReportError("Invalid parameter");
						return null;
					}
				}
				// Copy CHR data.
				mapper	= new ChrMapper(this.mFileData, chroffset, bank);
			}
			catch{
				this.mAppRef.ReportError("Invalid parameter");
				return null;
			}

			// Set palette.
			PaletteBitmap	bitmap	= new PaletteBitmap((uint)(width * 8), (uint)(height * 8));
			bitmap.AddColor(0x00000000);			// Transparent
			for(int i=0; i<palette.Length; i++){
				int	index	= palette[i];
				if(index >= 0){
					UInt32	color	= 0xFF000000U | (UInt32)NesPalette[index];
					bitmap.AddColor((int)color);	// ARGB
				}
				else{
					bitmap.AddColor(0x00000000);	// Transparent
				}
			}

			// Convert to pixels.
			for(int row=0; row<height; row++){
				for(int col=0; col<width; col++){
					int	address;
					if(!vertical){
						// Horizontal
						address	= tileoffset + (row * width) + col;
					}
					else{
						// Vertical
						address	= tileoffset + (col * height) + row;
					}
					byte	tile	= (byte)(this.mFileData[address] + chipoffset);
					this.GenerateTile(bitmap, mapper, (uint)row, (uint)col, tile);
				}
			}

			return bitmap.Bitmap;
		}

		private void GenerateTile(PaletteBitmap bitmap, ChrMapper mapper, uint row, uint col, byte tile){
			for(uint tileY=0; tileY<8; tileY++){
				uint	drawY	= (uint)(row * 8 + tileY);
				for(uint tileX=0; tileX<8; tileX++){
					uint	drawX	= col * 8 + tileX;
					byte	color	= mapper.GetPixel(tile, tileX, tileY);
					bitmap.SetPixel(drawX, drawY, (byte)(color + 1));
				}
			}
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

	internal class ChrMapper{
		private byte[]	chrData	= new byte[256 * 16];	// 2bpp * 8px * 8px / 8byte

		public ChrMapper(byte[] fileData, int chrOffset, int[] bank){
			float	bankLength	= 256.0F / bank.Length;
			float	bankDiv		= bank.Length / 256.0F;
			for(int i=0; i<256; i++){
				int	bankIndex	= (int)Math.Floor(bankDiv * i);
				int	bankOffset	= (int)Math.Floor(bank[bankIndex] * bankLength);
				int	tileIndex	= (int)Math.Floor(i % bankLength);
				int	romAddress	= chrOffset + ((bankOffset + tileIndex) * 16);
				for(int j=0; j<8; j++){
					this.chrData[(i * 16) + j + 0]	= fileData[romAddress + j + 0];
					this.chrData[(i * 16) + j + 8]	= fileData[romAddress + j + 8];
				}
			}
		}

		public byte GetPixel(byte tile, uint x, uint y){
			int	index	= ((int)tile * 16) + ((int)y & 7);	// max : 255 * 16 + 7 + 8 = 4095
			byte	high	= this.chrData[index + 8];
			byte	low	= this.chrData[index + 0];
			int	shift	= 7 - ((int)x & 7);
			int	highBit	= ((high >> shift) & 0x01) << 1;
			int	lowBit	= ((low  >> shift) & 0x01);
			byte	color	= (byte)(highBit | lowBit);
			return color;
		}
	}
}
