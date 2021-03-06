﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
// Based off the incredible work from https://dl.dropboxusercontent.com/u/12694594/lava.txt
using System;
using System.Security.Cryptography;
using ClassicalSharp.Generator;
using ClassicalSharp;

namespace ClassicalSharp {
	public unsafe class LavaAnimation {
		float[] flameHeat, potHeat, soupHeat;
		JavaRandom rnd = null;
		
		public unsafe void Tick( int* ptr, int size ) {
			if( rnd == null )
				rnd = new JavaRandom( new Random().Next() );
			int mask = size - 1, shift = CheckSize( size );
			
			int i = 0;
			for( int y = 0; y < size; y++ )
				for( int x = 0; x < size; x++ )
			{
				// Calculate the colour at this coordinate in the heatmap
				float lSoupHeat = 0;
				int xOffset = x + (int)(1.2 * Math.Sin( y * 22.5 * Utils.Deg2Rad ));
				int yOffset = y + (int)(1.2 * Math.Sin( x * 22.5 * Utils.Deg2Rad ));
				for( int j = 0; j < 9; j++ ) {
					int xx = xOffset + (j % 3 - 1);
					int yy = yOffset + (j / 3 - 1);
					lSoupHeat += soupHeat[(yy & mask) << shift | (xx & mask)];
				}
				
				float lPotHeat = potHeat[i]                                 // x    , y
					+ potHeat[y << shift | ((x + 1) & mask)] +              // x + 1, y
					+ potHeat[((y + 1) & mask) << shift | x] +              // x    , y + 1
					+ potHeat[((y + 1) & mask) << shift | ((x + 1) & mask)];// x + 1, y + 1
				
				soupHeat[i] = lSoupHeat * 0.1f + lPotHeat * 0.2f;
				potHeat[i] += flameHeat[i];
				if( potHeat[i] < 0 ) potHeat[i] = 0;
				flameHeat[i] -= 0.06f * 0.01f;
				
				if( rnd.NextFloat() <= 0.005f )
					flameHeat[i] = 1.5f * 0.01f;
				
				// Output the pixel
				float col = 2 * soupHeat[i];
				col = col < 0 ? 0 : col;
				col = col > 1 ? 1 : col;
				
				float r = col * 100 + 155;
				float g = col * col * 255;
				float b = col * col * col * col * 128;
				*ptr = 255 << 24 | (byte)r << 16 | (byte)g << 8 | (byte)b;
				
				ptr++; i++;
			}
		}
		
		int CheckSize( int size ) {
			if( potHeat == null || potHeat.Length < size * size ) {
				flameHeat = new float[size * size];
				potHeat = new float[size * size];
				soupHeat = new float[size * size];
			}
			
			int shift = 0;
			while( size > 1 ) { shift++; size >>= 1; }
			return shift;
		}
	}
}