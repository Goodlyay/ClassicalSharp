using System;
using System.Drawing;
using System.IO;
using ClassicalSharp.Map;
using ClassicalSharp.Events;

namespace ClassicalSharp.Map {
	/// <summary> Calculates the light levels for a byte array the size of the map </summary>
	public class LightVolume {
		
		Game game;
		World map;
		BlockInfo info;
		WorldEnv env;
		
		int width, length, height, sidesLevel, edgeLevel;
		public const byte lightExtent = 16;
		public const byte maxLight = (byte)(lightExtent - 1);
		protected int maxX, maxY, maxZ;
		public static byte[, ,] lightLevels;
		public static int[,] lightmap, lightmapZSide, lightmapXSide, lightmapYBottom;
		
		public void Init( Game game ) {
			this.game = game;
			map = game.World;
			env = game.World.Env;
			info = game.BlockInfo;
			game.WorldEvents.OnNewMapLoaded += OnNewMapLoaded;
			game.Events.TextureChanged += TextureChanged;
			game.WorldEvents.EnvVariableChanged += EnvVariableChanged;
			
			lightmap = new int[lightExtent, lightExtent];
			lightmapXSide = new int[lightExtent, lightExtent];
			lightmapZSide = new int[lightExtent, lightExtent];
			lightmapYBottom = new int[lightExtent, lightExtent];
			UpdateLightmap();
		}
		
		void EnvVariableChanged(object sender, EnvVarEventArgs e) {
			if (e.Var == EnvVar.ShadowlightColour || e.Var == EnvVar.SunlightColour) {
				UpdateLightmap();
				game.MapRenderer.Refresh();
			}
		}
		
		
		void UpdateLightmap() {
			FastColour darkShadow = env.Shadowlight;
			FastColour torchLight = new FastColour(249, 218, 185, 255);
			darkShadow.R >>= 3;
			darkShadow.G >>= 3;
			darkShadow.B >>= 3;
			FastColour halfSun = FastColour.Lerp(env.Shadowlight, env.Sunlight, 0.5f);
			FastColour torchSun = new FastColour(Math.Max(torchLight.R, env.Sunlight.R), Math.Max(torchLight.G, env.Sunlight.G), Math.Max(torchLight.B, env.Sunlight.B));
			for (int y = 0; y < lightExtent; y++) {
				
				float lerpY = y / (float)(lightExtent - 1);
				lerpY = 1.0f -(float)Math.Cos(lerpY *Math.PI * 0.5f);
				
				FastColour lerpShadow = FastColour.Lerp(darkShadow, torchLight, lerpY);
				FastColour lerpHalfLight = FastColour.Lerp(halfSun, torchLight, lerpY);
				FastColour lerpLight = FastColour.Lerp(env.Sunlight, torchLight, lerpY);
					
				for (int x = 0; x < lightExtent; x++)
				{
					//1 -cos
					float lerpX = x / (float)(lightExtent - 1);
					lerpX = 1.0f -(float)Math.Cos(lerpX *Math.PI * 0.5f);
					
					FastColour col = FastColour.Lerp(lerpShadow, lerpHalfLight, lerpX);
					
					
					SetLightmap(x, y, col);
				}
				SetLightmap(15, y, lerpLight);
			}
		}

		void TextureChanged(object sender, TextureEventArgs e) {
			//return; //because I'm trying to test using only the env colors
			if (e.Name != "lightmap.png") return;
			
			using (MemoryStream ms = new MemoryStream(e.Data))
				using (Bitmap bmp = Platform.ReadBmp(ms))
			{
				if (bmp.Width != lightExtent || bmp.Height != lightExtent) {
					game.Chat.Add("&clightmap.png must be " + lightExtent + "x" + lightExtent + "."); return;
				}
				
				// Not bothering with FastBitmap here as perf increase is insignificant.
				for (int y = 0; y < lightExtent; y++)
					for (int x = 0; x < lightExtent; x++)
				{
					Color col = bmp.GetPixel(x, y);
					SetLightmap(x, y, new FastColour(col));
				}
			}
			game.MapRenderer.Refresh();
		}
		
		static void SetLightmap(int x, int y, FastColour col) {
			lightmap[x, y] = col.Pack();
			FastColour.GetShaded(col, out lightmapXSide[x, y],
			                     out lightmapZSide[x, y], out lightmapYBottom[x, y]);
		}
		
		void OnNewMapLoaded( object sender, EventArgs e ) {
			map = game.World;
			width = map.Width; height = map.Height; length = map.Length;
			maxX = width - 1; maxY = height - 1; maxZ = length - 1;
			Console.WriteLine("got to OnNewMapLoaded; size: " + width * height * length + "." );
			
			sidesLevel = Math.Max( 0, game.World.Env.SidesHeight );
			edgeLevel = Math.Max( 0, game.World.Env.EdgeHeight );
			lightLevels = new byte[width, height, length];
			CastInitial();
			
			for( int pass = maxLight; pass > 1; pass-- ) {
				Console.WriteLine("Starting pass " + pass + "." );
				DoPass(pass);
			}
		}

		void CastInitial() {
			//initial loop for making fullbright spots
			int offset = 0, oneY = width * length;
			for( int z = 0; z < length; z++ ) {
				for( int x = 0; x < width; x++ ) {
					int index = (maxY * oneY) + offset;
					offset++; // increase horizontal position
					
					for( int y = maxY; y >= 0; y-- ) {
						byte curBlock = map.blocks[index];
						index -= oneY; // reduce y position
						
						//if the current block is in sunlight assign the fullest sky brightness to the higher 4 bits
						if( (y - 1) > map.GetLightHeight(x, z) ) { lightLevels[x, y, z] = (byte)(maxLight << 4); }
						//if the current block is fullbright assign the fullest block brightness to the higher 4 bits
						if( info.FullBright[curBlock] ) { lightLevels[x, y, z] |= maxLight; }
					}
				}
			}
		}
		
		void DoPass( int pass ) {
			int index = 0;			
			bool[] lightPasses = new bool[Block.Count];
			for (int i = 0; i < lightPasses.Length; i++) {
				// Light passes through a block if a) doesn't block light b) block isn't full block
				lightPasses[i] =
					!game.BlockInfo.BlocksLight[i] ||
					game.BlockInfo.MinBB[i] != OpenTK.Vector3.Zero ||
					game.BlockInfo.MaxBB[i] != OpenTK.Vector3.One;
			}
			
			for( int y = 0; y < height; y++ )
				for( int z = 0; z < length; z++ )
					for( int x = 0; x < width; x++ )
			{
				byte curBlock = map.blocks[index];
				
				int skyLight = lightLevels[x, y, z] >> 4;
				//if the current block is not a light blocker AND the current spot is less than i
				if( !info.BlocksLight[curBlock] && skyLight == pass ) {
					//check the six neighbors sky light value,
					if( y < maxY && skyLight > (lightLevels[x, y+1, z] >> 4) ) {
						if( lightPasses[map.blocks[index + width * length]] ){
							lightLevels[x, y+1, z] &= 0x0F; // reset skylight bits to 0
							lightLevels[x, y+1, z] |= (byte)((skyLight - 1) << 4); // set skylight bits
						}
					}
					if( y > 0 && skyLight > (lightLevels[x, y-1, z] >> 4) ) {
						if( lightPasses[map.blocks[index - width * length]] ) {
							lightLevels[x, y-1, z] &= 0x0F;
							lightLevels[x, y-1, z] |= (byte)((skyLight - 1) << 4);
						}
					}
					if( x < maxX && skyLight > (lightLevels[x+1, y, z] >> 4) ) {
						if( lightPasses[map.blocks[index + 1]] ) {
							lightLevels[x+1, y, z] &= 0x0F;
							lightLevels[x+1, y, z] |= (byte)((skyLight - 1) << 4);
						}
					}
					if( x > 0 && skyLight > (lightLevels[x-1, y, z] >> 4) ) {
						if( lightPasses[map.blocks[index - 1]]) {
							lightLevels[x-1, y, z] &= 0x0F;
							lightLevels[x-1, y, z] |= (byte)((skyLight - 1) << 4);
						}
					}
					if( z < maxZ && skyLight > (lightLevels[x, y, z+1] >> 4) ) {
						if( lightPasses[map.blocks[index + width]]) {
							lightLevels[x, y, z+1] &= 0x0F;
							lightLevels[x, y, z+1] |= (byte)((skyLight - 1) << 4);
						}
					}
					if( z > 0 && skyLight > (lightLevels[x, y, z-1] >> 4) ) {
						if( lightPasses[map.blocks[index - width]]) {
							lightLevels[x, y, z-1] &= 0x0F;
							lightLevels[x, y, z-1] |= (byte)((skyLight - 1) << 4);
						}
					}
				}
				
				int blockLight = lightLevels[x, y, z] & 0x0F;
				//if the current block is not a light blocker AND the current spot is less than i
				if( (info.FullBright[curBlock] || !info.BlocksLight[curBlock]) && blockLight == pass ) {
					//check the six neighbors sky light value,
					if( y < maxY && blockLight > (lightLevels[x, y+1, z] & 0x0F) ) {
						if( lightPasses[map.blocks[index + width * length]] ){
							lightLevels[x, y+1, z] &= 0xF0; // reset blocklight bits to 0
							lightLevels[x, y+1, z] |= (byte)(blockLight - 1); // set blocklight bits
						}
					}
					if( y > 0 && blockLight > (lightLevels[x, y-1, z] & 0x0F) ) {
						if( lightPasses[map.blocks[index - width * length]] ) {
							lightLevels[x, y-1, z] &= 0xF0;
							lightLevels[x, y-1, z] |= (byte)(blockLight - 1);
						}
					}
					if( x < maxX && blockLight > (lightLevels[x+1, y, z] & 0x0F) ) {
						if( lightPasses[map.blocks[index + 1]] ) {
							lightLevels[x+1, y, z] &= 0xF0;
							lightLevels[x+1, y, z] |= (byte)(blockLight - 1);
						}
					}
					if( x > 0 && blockLight > (lightLevels[x-1, y, z] & 0x0F) ) {
						if( lightPasses[map.blocks[index - 1]] ) {
							lightLevels[x-1, y, z] &= 0xF0;
							lightLevels[x-1, y, z] |= (byte)(blockLight - 1);
						}
					}
					if( z < maxZ && blockLight > (lightLevels[x, y, z+1] & 0x0F) ) {
						if( lightPasses[map.blocks[index + width]] ) {
							lightLevels[x, y, z+1] &= 0xF0;
							lightLevels[x, y, z+1] |= (byte)(blockLight - 1);
						}
					}
					if( z > 0 && blockLight > (lightLevels[x, y, z-1] & 0x0F) ) {
						if( lightPasses[map.blocks[index - width]] ) {
							lightLevels[x, y, z-1] &= 0xF0;
							lightLevels[x, y, z-1] |= (byte)(blockLight - 1);
						}
					}
				}
				index++; // increase one coord
			}
		}
	}
}