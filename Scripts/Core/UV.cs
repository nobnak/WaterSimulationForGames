using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem.Core {

	public class UV : System.IDisposable {

		public static readonly int P_TEXEL_SIZE = Shader.PropertyToID("TexelSize");
		public static readonly int P_RESULT = Shader.PropertyToID("Result");

		public ComputeShader CS { get; protected set; }

		protected readonly int K_GENERATE;

		public UV() {
			CS = Resources.Load<ComputeShader>("WaterSimulationForGames/UV");
			K_GENERATE = CS.FindKernel("Generate");
		}

		public void Generate(RenderTexture uv) {
			var w = uv.width;
			var h = uv.height;
			var texelsize = new Vector4(1f / w, 1f / h, w, h);

			CS.SetVector(P_TEXEL_SIZE, texelsize);
			CS.SetTexture(K_GENERATE, P_RESULT, uv);

			var ds = CS.DispatchSize(K_GENERATE, w, h);
			CS.Dispatch(K_GENERATE, ds.x, ds.y, ds.z);
		}

		#region IDisposable
		public void Dispose() {
		}
		#endregion
	}
}
