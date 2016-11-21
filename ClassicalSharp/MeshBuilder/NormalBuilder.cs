﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Map;
using OpenTK;

namespace ClassicalSharp {

	public unsafe sealed class NormalMeshBuilder : ChunkMeshBuilder {
		
		protected override int StretchXLiquid( int xx, int countIndex, int x, int y, int z, int chunkIndex, byte block ) {
			if( OccludedLiquid( chunkIndex ) ) return 0;
			int count = 1;
			return count;
			x++;
			chunkIndex++;
			countIndex += Side.Sides;
			int max = chunkSize - xx;
			
			while( count < max && x < width && CanStretch( block, chunkIndex, x, y, z, Side.Top )
			      && !OccludedLiquid( chunkIndex ) ) {
				counts[countIndex] = 0;
				count++;
				x++;
				chunkIndex++;
				countIndex += Side.Sides;
			}
			return count;
		}
		
		protected override int StretchX( int xx, int countIndex, int x, int y, int z, int chunkIndex, byte block, int face ) {
			int count = 1;
			return count;
			x++;
			chunkIndex++;
			countIndex += Side.Sides;
			int max = chunkSize - xx;
			bool stretchTile = (info.CanStretch[block] & (1 << face)) != 0;
			
			while( count < max && x < width && stretchTile && CanStretch( block, chunkIndex, x, y, z, face ) ) {
				counts[countIndex] = 0;
				count++;
				x++;
				chunkIndex++;
				countIndex += Side.Sides;
			}
			return count;
		}
		
		protected override int StretchZ( int zz, int countIndex, int x, int y, int z, int chunkIndex, byte block, int face ) {
			int count = 1;
			return count;
			z++;
			chunkIndex += extChunkSize;
			countIndex += chunkSize * Side.Sides;
			int max = chunkSize - zz;
			bool stretchTile = (info.CanStretch[block] & (1 << face)) != 0;
			
			while( count < max && z < length && stretchTile && CanStretch( block, chunkIndex, x, y, z, face ) ) {
				counts[countIndex] = 0;
				count++;
				z++;
				chunkIndex += extChunkSize;
				countIndex += chunkSize * Side.Sides;
			}
			return count;
		}
		
		bool CanStretch( byte initialTile, int chunkIndex, int x, int y, int z, int face ) {
			byte rawBlock = chunk[chunkIndex];
			return rawBlock == initialTile 
				&& !info.IsFaceHidden( rawBlock, chunk[chunkIndex + offsets[face]], face )
				&& (fullBright || IsLit( X, Y, Z, face, initialTile ) == IsLit( x, y, z, face, rawBlock ) );
		}
		
		
		protected override void DrawLeftFace( int count ) {
			int texId = info.textures[curBlock * Side.Sides + Side.Left];
			int i = texId / elementsPerAtlas1D;
			float vOrigin = (texId % elementsPerAtlas1D) * invVerElementSize;
			int offset = (lightFlags >> Side.Left) & 1;
			
			float u1 = minBB.Z, u2 = (count - 1) + maxBB.Z * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			DrawInfo part = isTranslucent ? translucentParts[i] : normalParts[i];
			//int col = fullBright ? FastColour.WhitePacked :
			//	X >= offset ? (Y > map.heightmap[(Z * width) + (X - offset)] ? env.SunXSide : env.ShadowXSide) : env.SunXSide;
			
			int col = FastColour.c0;
			int safeX = X -offset;
			if( safeX < 0 ) {
				safeX = 0;
				col = FastColour.c7;
			}
			else {
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 1 ) {  col = FastColour.c1; }
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 2 ) {  col = FastColour.c2; }
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 3 ) {  col = FastColour.c3; }
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 4 ) {  col = FastColour.c4; }
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 5 ) {  col = FastColour.c5; }
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 6 ) {  col = FastColour.c6; }
				if( (LightVolume.volumeArray[X -offset, Y, Z] >> 4) == 7 ) {  col = FastColour.c7; }				
			}
			
			FastColour colNice = new FastColour( col );
			int colXSide;
			int colZSide;
			int colYBottom;
			FastColour.GetShaded(colNice, out colXSide, out colZSide, out colYBottom);
			col = colXSide;
			
			part.vertices[part.vIndex.left++] = new VertexP3fT2fC4b( x1, y2, z2 + (count - 1), u2, v1, col );
			part.vertices[part.vIndex.left++] = new VertexP3fT2fC4b( x1, y2, z1, u1, v1, col );
			part.vertices[part.vIndex.left++] = new VertexP3fT2fC4b( x1, y1, z1, u1, v2, col );
			part.vertices[part.vIndex.left++] = new VertexP3fT2fC4b( x1, y1, z2 + (count - 1), u2, v2, col );
		}

		protected override void DrawRightFace( int count ) {
			int texId = info.textures[curBlock * Side.Sides + Side.Right];
			int i = texId / elementsPerAtlas1D;
			float vOrigin = (texId % elementsPerAtlas1D) * invVerElementSize;
			int offset = (lightFlags >> Side.Right) & 1;
			
			float u1 = (count - minBB.Z), u2 = (1 - maxBB.Z) * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			DrawInfo part = isTranslucent ? translucentParts[i] : normalParts[i];
			//int col = fullBright ? FastColour.WhitePacked :
			//	X <= (maxX - offset) ? (Y > map.heightmap[(Z * width) + (X + offset)] ? env.SunXSide : env.ShadowXSide) : env.SunXSide;
			
			
			int col = FastColour.c0;
			int safeX = X +offset;
			if( safeX >= width ) {
				safeX = width-1;
				col = FastColour.c7;
			}
			else {
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 1 ) {  col = FastColour.c1; }
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 2 ) {  col = FastColour.c2; }
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 3 ) {  col = FastColour.c3; }
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 4 ) {  col = FastColour.c4; }
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 5 ) {  col = FastColour.c5; }
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 6 ) {  col = FastColour.c6; }
				if( (LightVolume.volumeArray[X +offset, Y, Z] >> 4) == 7 ) {  col = FastColour.c7; }			
			}
			
			FastColour colNice = new FastColour( col );
			int colXSide;
			int colZSide;
			int colYBottom;
			FastColour.GetShaded(colNice, out colXSide, out colZSide, out colYBottom);
			col = colXSide;
			
			part.vertices[part.vIndex.right++] = new VertexP3fT2fC4b( x2, y2, z1, u1, v1, col );
			part.vertices[part.vIndex.right++] = new VertexP3fT2fC4b( x2, y2, z2 + (count - 1), u2, v1, col );
			part.vertices[part.vIndex.right++] = new VertexP3fT2fC4b( x2, y1, z2 + (count - 1), u2, v2, col );
			part.vertices[part.vIndex.right++] = new VertexP3fT2fC4b( x2, y1, z1, u1, v2, col );
		}

		protected override void DrawFrontFace( int count ) {
			int texId = info.textures[curBlock * Side.Sides + Side.Front];
			int i = texId / elementsPerAtlas1D;
			float vOrigin = (texId % elementsPerAtlas1D) * invVerElementSize;
			int offset = (lightFlags >> Side.Front) & 1;
			
			float u1 = (count - minBB.X), u2 = (1 - maxBB.X) * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			DrawInfo part = isTranslucent ? translucentParts[i] : normalParts[i];
			//int col = fullBright ? FastColour.WhitePacked :
			//	Z >= offset ? (Y > map.heightmap[((Z - offset) * width) + X] ? env.SunZSide : env.ShadowZSide) : env.SunZSide;
			
			int col = FastColour.c0;
			int safeZ = Z -offset;
			if( safeZ < 0 ) {
				safeZ = 0;
				col = FastColour.c7;
			}
			else {
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 1 ) {  col = FastColour.c1; }
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 2 ) {  col = FastColour.c2; }
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 3 ) {  col = FastColour.c3; }
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 4 ) {  col = FastColour.c4; }
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 5 ) {  col = FastColour.c5; }
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 6 ) {  col = FastColour.c6; }
				if( (LightVolume.volumeArray[X, Y, Z -offset] >> 4) == 7 ) {  col = FastColour.c7; }				
			}
			
			FastColour colNice = new FastColour( col );
			int colXSide;
			int colZSide;
			int colYBottom;
			FastColour.GetShaded(colNice, out colXSide, out colZSide, out colYBottom);
			col = colZSide;
			
			part.vertices[part.vIndex.front++] = new VertexP3fT2fC4b( x2 + (count - 1), y1, z1, u2, v2, col );
			part.vertices[part.vIndex.front++] = new VertexP3fT2fC4b( x1, y1, z1, u1, v2, col );
			part.vertices[part.vIndex.front++] = new VertexP3fT2fC4b( x1, y2, z1, u1, v1, col );
			part.vertices[part.vIndex.front++] = new VertexP3fT2fC4b( x2 + (count - 1), y2, z1, u2, v1, col );
		}
		
		protected override void DrawBackFace( int count ) {
			int texId = info.textures[curBlock * Side.Sides + Side.Back];
			int i = texId / elementsPerAtlas1D;
			float vOrigin = (texId % elementsPerAtlas1D) * invVerElementSize;
			int offset = (lightFlags >> Side.Back) & 1;
			
			float u1 = minBB.X, u2 = (count - 1) + maxBB.X * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			DrawInfo part = isTranslucent ? translucentParts[i] : normalParts[i];
			//int col = fullBright ? FastColour.WhitePacked :
				//Z <= (maxZ - offset) ? (Y > map.heightmap[((Z + offset) * width) + X] ? env.SunZSide : env.ShadowZSide) : env.SunZSide;
			
				
			
			int col = FastColour.c0;
			int safeZ = Z +offset;
			if( safeZ >= length ) {
				safeZ = length-1;
				col = FastColour.c7;
			}
			else {
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 1 ) {  col = FastColour.c1; }
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 2 ) {  col = FastColour.c2; }
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 3 ) {  col = FastColour.c3; }
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 4 ) {  col = FastColour.c4; }
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 5 ) {  col = FastColour.c5; }
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 6 ) {  col = FastColour.c6; }
				if( (LightVolume.volumeArray[X, Y, Z +offset] >> 4) == 7 ) {  col = FastColour.c7; }				
			}
			
			FastColour colNice = new FastColour( col );
			int colXSide;
			int colZSide;
			int colYBottom;
			FastColour.GetShaded(colNice, out colXSide, out colZSide, out colYBottom);
			col = colZSide;
				
			part.vertices[part.vIndex.back++] = new VertexP3fT2fC4b( x2 + (count - 1), y2, z2, u2, v1, col );
			part.vertices[part.vIndex.back++] = new VertexP3fT2fC4b( x1, y2, z2, u1, v1, col );
			part.vertices[part.vIndex.back++] = new VertexP3fT2fC4b( x1, y1, z2, u1, v2, col );
			part.vertices[part.vIndex.back++] = new VertexP3fT2fC4b( x2 + (count - 1), y1, z2, u2, v2, col );
		}
		
		protected override void DrawBottomFace( int count ) {
			int texId = info.textures[curBlock * Side.Sides + Side.Bottom];
			int i = texId / elementsPerAtlas1D;
			float vOrigin = (texId % elementsPerAtlas1D) * invVerElementSize;
			int offset = (lightFlags >> Side.Bottom) & 1;
			
			float u1 = minBB.X, u2 = (count - 1) + maxBB.X * 15.99f/16f;
			float v1 = vOrigin + minBB.Z * invVerElementSize;
			float v2 = vOrigin + maxBB.Z * invVerElementSize * 15.99f/16f;
			DrawInfo part = isTranslucent ? translucentParts[i] : normalParts[i];
			//int col = fullBright ? FastColour.WhitePacked : ((Y - offset) > map.heightmap[(Z * width) + X] ? env.SunYBottom : env.ShadowYBottom);
			
			int col = FastColour.c0;
			int safeY = Y +offset;
			if( safeY < 0 ) {
				safeY = 0;
				col = FastColour.c7;
			}
			else {
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 1 ) {  col = FastColour.c1; }
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 2 ) {  col = FastColour.c2; }
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 3 ) {  col = FastColour.c3; }
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 4 ) {  col = FastColour.c4; }
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 5 ) {  col = FastColour.c5; }
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 6 ) {  col = FastColour.c6; }
				if( (LightVolume.volumeArray[X, Y -offset, Z] >> 4) == 7 ) {  col = FastColour.c7; }				
			}
			
			FastColour colNice = new FastColour( col );
			int colXSide;
			int colZSide;
			int colYBottom;
			FastColour.GetShaded(colNice, out colXSide, out colZSide, out colYBottom);
			col = colYBottom;
			
			part.vertices[part.vIndex.bottom++] = new VertexP3fT2fC4b( x2 + (count - 1), y1, z2, u2, v2, col );
			part.vertices[part.vIndex.bottom++] = new VertexP3fT2fC4b( x1, y1, z2, u1, v2, col );
			part.vertices[part.vIndex.bottom++] = new VertexP3fT2fC4b( x1, y1, z1, u1, v1, col );
			part.vertices[part.vIndex.bottom++] = new VertexP3fT2fC4b( x2 + (count - 1), y1, z1, u2, v1, col );
		}

		protected override void DrawTopFace( int count ) {
			int texId = info.textures[curBlock * Side.Sides + Side.Top];
			int i = texId / elementsPerAtlas1D;
			float vOrigin = (texId % elementsPerAtlas1D) * invVerElementSize;
			int offset = (lightFlags >> Side.Top) & 1;
			
			float u1 = minBB.X, u2 = (count - 1) + maxBB.X * 15.99f/16f;
			float v1 = vOrigin + minBB.Z * invVerElementSize;
			float v2 = vOrigin + maxBB.Z * invVerElementSize * 15.99f/16f;
			DrawInfo part = isTranslucent ? translucentParts[i] : normalParts[i];
			//int col = fullBright ? FastColour.WhitePacked : ((Y - offset) >= map.heightmap[(Z * width) + X] ? env.Sun : env.Shadow);
			
			int col = FastColour.c0;
			int safeY = Y +offset;
			if( safeY >= height ) {
				safeY = height-1;
				col = FastColour.c7;
			}
			else {
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 1 ) {  col = FastColour.c1; }
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 2 ) {  col = FastColour.c2; }
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 3 ) {  col = FastColour.c3; }
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 4 ) {  col = FastColour.c4; }
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 5 ) {  col = FastColour.c5; }
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 6 ) {  col = FastColour.c6; }
				if( (LightVolume.volumeArray[X, Y +offset, Z] >> 4) == 7 ) {  col = FastColour.c7; }		
			}
				
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x2 + (count - 1), y2, z1, u2, v1, col );
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x1, y2, z1, u1, v1, col );
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x1, y2, z2, u1, v2, col );
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x2 + (count - 1), y2, z2, u2, v2, col );
		}
	}
}