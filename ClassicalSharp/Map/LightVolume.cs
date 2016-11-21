using System;
using ClassicalSharp.Map;
using ClassicalSharp.Events;

namespace ClassicalSharp.Map
{
	
	
	/// <summary>
	/// Calculates the light levels for a byte array the size of the map
	/// </summary>
	public class LightVolume {
		
		protected internal Game game;
		protected World map;
		protected BlockInfo info;
		
		internal int width, length, height, sidesLevel, edgeLevel;
		protected static byte lightmapSize = 8;
		protected static byte lightValueMax = (byte)(lightmapSize -1);
		protected byte curBlock;
		protected int maxX, maxY, maxZ;
		public static byte[, ,] volumeArray;
		
		public LightVolume() {

		}
		
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
			long[] volumeCounts = new long[lightmapSize];
			
			//initial loop for making fullbright spots
			for( int z = maxZ; z >= 0; z-- ) {
			    for( int x = maxX; x >= 0; x-- ) {
					for( int y = maxY; y >= 0; y-- ) {
						int safeY = y -1;
						if( safeY < 0 ) { safeY = 0; }
					    curBlock = map.GetBlock(x, safeY, z);
					    
					    //if the current block is in sunlight assign the fullest sky brightness to the higher 4 bits
					    if( map.IsLit(x, safeY, z) ) { volumeArray[x, y, z] = (byte)(lightValueMax << 4); }
					    //if the current block is fullbright assign the fullest block brightness to the higher 4 bits
					    if( info.FullBright[curBlock] ) { volumeArray[x, y, z] |= lightValueMax; }
					}
			    }
			}
			
			for( int i = lightValueMax -1; i > 0; i-- ) {
				Console.WriteLine("Starting pass " + i + "." );
				for( int z = maxZ; z >= 0; z-- ) {
				    for( int x = maxX; x >= 0; x-- ) {
						for( int y = maxY; y >= 0; y-- ) {
							curBlock = map.GetBlock(x, y, z);
							//your pc: kill me
							
							byte tempSkyLevelPrev = 0;
							byte tempSkyLevel = (byte)(volumeArray[x, y, z] >> 4);
							
							//if the current block is not a light blocker AND the current spot is less than i
							if( !info.BlocksLight[curBlock] && tempSkyLevel < i ) {
								//check the six neighbors sky light value,
								if( y+1 < maxY && tempSkyLevelPrev < (volumeArray[x, y+1, z] >> 4) ) {
									tempSkyLevel = (byte)(volumeArray[x, y+1, z] >> 4);
									tempSkyLevelPrev = tempSkyLevel;
								}
								if( y-1 >= 0 && tempSkyLevelPrev < (volumeArray[x, y-1, z] >> 4) ) {
									tempSkyLevel = (byte)(volumeArray[x, y-1, z] >> 4);
									tempSkyLevelPrev = tempSkyLevel;
								}
								if( x+1 < maxX && tempSkyLevelPrev < (volumeArray[x+1, y, z] >> 4) ) {
									tempSkyLevel = (byte)(volumeArray[x+1, y, z] >> 4);
									tempSkyLevelPrev = tempSkyLevel;
								}
								if( x-1 >= 0 && tempSkyLevelPrev < (volumeArray[x-1, y, z] >> 4) ) {
									tempSkyLevel = (byte)(volumeArray[x-1, y, z] >> 4);
									tempSkyLevelPrev = tempSkyLevel;
								}
								if( z+1 < maxZ && tempSkyLevelPrev < (volumeArray[x, y, z+1] >> 4) ) {
									tempSkyLevel = (byte)(volumeArray[x, y, z+1] >> 4);
									tempSkyLevelPrev = tempSkyLevel;
								}
								if( z-1 >= 0 && tempSkyLevelPrev < (volumeArray[x, y, z-1] >> 4) ) {
									tempSkyLevel = (byte)(volumeArray[x, y, z-1] >> 4);
									tempSkyLevelPrev = tempSkyLevel;
								}
								if( tempSkyLevel > 0 ) {
									tempSkyLevel--;
								}
							}
							
							byte tempBlockLevelPrev = 0;
							byte tempBlockLevel = (byte)(volumeArray[x, y, z] & 0x0F);
							//if the current block is not a light blocker AND the current spot is less than i
							if( !info.BlocksLight[curBlock] && tempBlockLevel < i ) {
								//check the six neighbors sky light value,
								if( y+1 < maxY && tempBlockLevelPrev < (volumeArray[x, y+1, z] & 0x0F) ) {
									tempBlockLevel = (byte)(volumeArray[x, y+1, z] & 0x0F);
									tempBlockLevelPrev = tempBlockLevel;
								}
								if( y-1 >= 0 && tempBlockLevelPrev < (volumeArray[x, y-1, z] & 0x0F) ) {
									tempBlockLevel = (byte)(volumeArray[x, y-1, z] & 0x0F);
									tempBlockLevelPrev = tempBlockLevel;
								}
								if( x+1 < maxX && tempBlockLevelPrev < (volumeArray[x+1, y, z] & 0x0F) ) {
									tempBlockLevel = (byte)(volumeArray[x+1, y, z] & 0x0F);
									tempBlockLevelPrev = tempBlockLevel;
								}
								if( x-1 >= 0 && tempBlockLevelPrev < (volumeArray[x-1, y, z] & 0x0F) ) {
									tempBlockLevel = (byte)(volumeArray[x-1, y, z] & 0x0F);
									tempBlockLevelPrev = tempBlockLevel;
								}
								if( z+1 < maxZ && tempBlockLevelPrev < (volumeArray[x, y, z+1] & 0x0F) ) {
									tempBlockLevel = (byte)(volumeArray[x, y, z+1] & 0x0F);
									tempBlockLevelPrev = tempBlockLevel;
								}
								if( z-1 >= 0 && tempBlockLevelPrev < (volumeArray[x, y, z-1] & 0x0F) ) {
									tempBlockLevel = (byte)(volumeArray[x, y, z-1] & 0x0F);
									tempBlockLevelPrev = tempBlockLevel;
								}
								if( tempBlockLevel > 0 ) {
									tempBlockLevel--;
								}
							}
							
							volumeArray[x, y, z] = (byte)((tempSkyLevel << 4) | (tempBlockLevel) );
							//set the volumeArray's lower 4 bits to the value one less than the brightest neighbors block light value
							
						}
				    }
				}
				
			}
			
			
			for( int x = maxX; x >= 0; x-- ) {
			    for( int z = maxZ; z >= 0; z-- ) {
					for( int y = maxY; y >= 0; y-- ) {
						volumeCounts[(volumeArray[x, y, z] >> 4)]++;
					}
				}
			}
			for( int i = 0; i < lightmapSize; i++ ) {
				Console.WriteLine("volumeCounts[ " + i + "] = " + volumeCounts[i] + ".");
			}
		}
		
	}
}
