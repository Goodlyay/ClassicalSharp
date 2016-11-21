// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
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
			
			int col;
			if( X - offset < 0 ) {
				col = FastColour.c7;
			} else {
				col = GetLight(X - offset, Y, Z);
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
			
			
			int col;
			if( X + offset >= width ) {
				col = FastColour.c7;
			} else {
				col = GetLight(X + offset, Y, Z);
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
			
			int col;
			if( Z - offset < 0 ) {
				col = FastColour.c7;
			} else {
				col = GetLight(X, Y, Z - offset);
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
			
			int col;
			if( Z + offset >= length ) {
				col = FastColour.c7;
			} else {
				col = GetLight(X, Y, Z + offset);
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
			
			int col;
			if( Y - offset < 0 ) {
				col = FastColour.c7;
			} else {
				col = GetLight(X, Y - offset, Z);
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
			
			int col;
			if( Y + offset >= height ) {
				col = FastColour.c7;
			} else {
				col = GetLight(X, Y + offset, Z);
			}
			
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x2 + (count - 1), y2, z1, u2, v1, col );
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x1, y2, z1, u1, v1, col );
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x1, y2, z2, u1, v2, col );
			part.vertices[part.vIndex.top++] = new VertexP3fT2fC4b( x2 + (count - 1), y2, z2, u2, v2, col );
		}
		
		int GetLight(int x, int y, int z) {
			int light = LightVolume.volumeArray[x, y, z];
			light = Math.Max(light & 0xF, light >> 4);
			switch (light) {
				case 1: return FastColour.c1;
				case 2: return FastColour.c2;
				case 3: return FastColour.c3;
				case 4: return FastColour.c4;
				case 5: return FastColour.c5;
				case 6: return FastColour.c6;
				case 7: return FastColour.c7;	
				default: return FastColour.c0;
			}
		}
	}
}