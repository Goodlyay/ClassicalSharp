using System;
using ClassicalSharp.Map;
using ClassicalSharp.Events;

namespace ClassicalSharp.Map {
	/// <summary> Calculates the light levels for a byte array the size of the map </summary>
	public class LightVolume {
		
		Game game;
		World map;
		BlockInfo info;
		
		int width, length, height, sidesLevel, edgeLevel;
		const byte lightmapSize = 8;
		const byte lightValueMax = (byte)(lightmapSize -1);
		protected int maxX, maxY, maxZ;
		public static byte[, ,] volumeArray;
		
		public void Init( Game game ) {
			this.game = game;
			info = game.BlockInfo;
			//game.Chat.Add("test.");
			//game.Events.TerrainAtlasChanged += TerrainAtlasChanged;
		}
		
		public void OnNewMapLoaded( object sender, EventArgs e ) {
			map = game.World;
			width = map.Width; height = map.Height; length = map.Length;
			maxX = width - 1; maxY = height - 1; maxZ = length - 1;
			Console.WriteLine("got to OnNewMapLoaded; size: " + width * height * length + "." );
			
			sidesLevel = Math.Max( 0, game.World.Env.SidesHeight );
			edgeLevel = Math.Max( 0, game.World.Env.EdgeHeight );
			
			volumeArray = new byte[width, height, length];
			//initial loop for making fullbright spots
			CastInitial();
			
			for( int pass = lightValueMax - 1; pass > 0; pass-- ) {
				Console.WriteLine("Starting pass " + pass + "." );
				DoPass(pass);
			}
		}

		void CastInitial() {
			int offset = 0, oneY = width * length;
			for( int z = 0; z < length; z++ ) {
				for( int x = 0; x < width; x++ ) {
					int index = (maxY * oneY) + offset;
					offset++; // increase horizontal position
					
					for( int y = maxY; y >= 0; y-- ) {
						byte curBlock = map.blocks[index];
						index -= oneY; // reduce y position
						
						//if the current block is in sunlight assign the fullest sky brightness to the higher 4 bits
						if( (y - 1) > map.GetLightHeight(x, z) ) { volumeArray[x, y, z] = (byte)(lightValueMax << 4); }
						//if the current block is fullbright assign the fullest block brightness to the higher 4 bits
						if( info.FullBright[curBlock] ) { volumeArray[x, y, z] |= lightValueMax; }
					}
				}
			}
		}
		
		void DoPass( int i ) {
			int offset = 0, oneY = width * length;
			for( int z = 0; z < length; z++ ) {
				for( int x = 0; x < width; x++ ) {
					int index = (maxY * oneY) + offset;
					offset++; // increase horizontal position
					
					for( int y = maxY; y >= 0; y-- ) {
						byte curBlock = map.blocks[index];
						index -= oneY; // reduce y position
						
						byte maxSkyLight = 0, skyLight = (byte)(volumeArray[x, y, z] >> 4);
						//if the current block is not a light blocker AND the current spot is less than i
						if( !info.BlocksLight[curBlock] && skyLight < i ) {
							//check the six neighbors sky light value,
							if( y+1 < maxY && maxSkyLight < (volumeArray[x, y+1, z] >> 4) ) {
								skyLight = (byte)(volumeArray[x, y+1, z] >> 4);
								maxSkyLight = skyLight;
							}
							if( y-1 >= 0 && maxSkyLight < (volumeArray[x, y-1, z] >> 4) ) {
								skyLight = (byte)(volumeArray[x, y-1, z] >> 4);
								maxSkyLight = skyLight;
							}
							if( x+1 < maxX && maxSkyLight < (volumeArray[x+1, y, z] >> 4) ) {
								skyLight = (byte)(volumeArray[x+1, y, z] >> 4);
								maxSkyLight = skyLight;
							}
							if( x-1 >= 0 && maxSkyLight < (volumeArray[x-1, y, z] >> 4) ) {
								skyLight = (byte)(volumeArray[x-1, y, z] >> 4);
								maxSkyLight = skyLight;
							}
							if( z+1 < maxZ && maxSkyLight < (volumeArray[x, y, z+1] >> 4) ) {
								skyLight = (byte)(volumeArray[x, y, z+1] >> 4);
								maxSkyLight = skyLight;
							}
							if( z-1 >= 0 && maxSkyLight < (volumeArray[x, y, z-1] >> 4) ) {
								skyLight = (byte)(volumeArray[x, y, z-1] >> 4);
								maxSkyLight = skyLight;
							}
							if( skyLight > 0 ) {
								skyLight--;
							}
						}
						
						byte maxBlockLight = 0, blockLight = (byte)(volumeArray[x, y, z] & 0x0F);
						//if the current block is not a light blocker AND the current spot is less than i
						if( !info.BlocksLight[curBlock] && blockLight < i ) {
							//check the six neighbors sky light value,
							if( y+1 < maxY && maxBlockLight < (volumeArray[x, y+1, z] & 0x0F) ) {
								blockLight = (byte)(volumeArray[x, y+1, z] & 0x0F);
								maxBlockLight = blockLight;
							}
							if( y-1 >= 0 && maxBlockLight < (volumeArray[x, y-1, z] & 0x0F) ) {
								blockLight = (byte)(volumeArray[x, y-1, z] & 0x0F);
								maxBlockLight = blockLight;
							}
							if( x+1 < maxX && maxBlockLight < (volumeArray[x+1, y, z] & 0x0F) ) {
								blockLight = (byte)(volumeArray[x+1, y, z] & 0x0F);
								maxBlockLight = blockLight;
							}
							if( x-1 >= 0 && maxBlockLight < (volumeArray[x-1, y, z] & 0x0F) ) {
								blockLight = (byte)(volumeArray[x-1, y, z] & 0x0F);
								maxBlockLight = blockLight;
							}
							if( z+1 < maxZ && maxBlockLight < (volumeArray[x, y, z+1] & 0x0F) ) {
								blockLight = (byte)(volumeArray[x, y, z+1] & 0x0F);
								maxBlockLight = blockLight;
							}
							if( z-1 >= 0 && maxBlockLight < (volumeArray[x, y, z-1] & 0x0F) ) {
								blockLight = (byte)(volumeArray[x, y, z-1] & 0x0F);
								maxBlockLight = blockLight;
							}
							if( blockLight > 0 ) {
								blockLight--;
							}
						}
						
						volumeArray[x, y, z] = (byte)((skyLight << 4) | (blockLight) );
						//set the volumeArray's lower 4 bits to the value one less than the brightest neighbors block light value
						
					}
				}
			}
		}
	}
}
