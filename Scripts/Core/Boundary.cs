using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class Boundary : System.IDisposable {
        public enum ColorIndex { Red = 0, Green, Blue, Alpha }

		public static readonly int P_SRC_IMAGE = Shader.PropertyToID("_SrcImage");
		public static readonly int P_BOUNDARY_TEXEL_SIZE = Shader.PropertyToID("_Boundary_TexelSize");
		public static readonly int P_BOUNDARY = Shader.PropertyToID("_Boundary");
        public static readonly int P_COLOR_INDEX = Shader.PropertyToID("_ColorIndex");

		public ComputeShader CS { get; protected set; }
		public readonly int K_CONVERT;

		public Boundary() {
			CS = Resources.Load<ComputeShader>("WaterSimulationForGames/Boundary");
			K_CONVERT = CS.FindKernel("Convert");
		}

		public void Convert(RenderTexture boundary, Texture srcImage, ColorIndex colorIndex = default) {
			var w = boundary.width;
			var h = boundary.height;
			var texelsize = new Vector4(1f / w, 1f / h, w, h);

            CS.SetInt(P_COLOR_INDEX, (int)colorIndex);
            CS.SetVector(P_BOUNDARY_TEXEL_SIZE, texelsize);
			CS.SetTexture(K_CONVERT, P_SRC_IMAGE, srcImage);
			CS.SetTexture(K_CONVERT, P_BOUNDARY, boundary);

			var ds = CS.DispatchSize(K_CONVERT, w, h);
			CS.Dispatch(K_CONVERT, ds.x, ds.y, ds.z);
		}

		#region IDisposable
		public void Dispose() {
		}
		#endregion
	}
}