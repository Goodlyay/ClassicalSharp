﻿using System;

namespace ClassicalSharp {
	
	public struct TextureRectangle {
		public float U1, V1, U2, V2;
		
		public TextureRectangle( float u, float v, float uWidth, float vHeight ) {
			U1 = u;
			V1 = v;
			U2 = u + uWidth;
			V2 = v + vHeight;
		}
		
		public static TextureRectangle FromPoints( float u1, float u2, float v1, float v2 ) {
			TextureRectangle rec;
			rec.U1 = u1;
			rec.U2 = u2;
			rec.V1 = v1;
			rec.V2 = v2;
			return rec;
		}

		public override string ToString() {
			return String.Format( "{0}, {1} : {2}, {3}", U1, V1, U2, V2 );
		}
		
		internal void SwapU() {
			float u2 = U2;
			U2 = U1;
			U1 = u2;
		}
	}
}