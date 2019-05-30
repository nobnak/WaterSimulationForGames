using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class Normal2D : System.IDisposable {
		public const string PATH = "WaterSimulationForGames/Normal";

		public static readonly int P_COUNT = Shader.PropertyToID("_Count");
		public static readonly int P_HEIGHT = Shader.PropertyToID("_Height");
		public static readonly int P_NORMAL = Shader.PropertyToID("_Normal");
		public static readonly int P_PARAMS = Shader.PropertyToID("_Params");

		public readonly int K_GENERATE;

		public ComputeShader cs { get; protected set; }

		public float Dxy { get; set; }

		#region interface
		public Normal2D() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_GENERATE = cs.FindKernel("Generate");
		}

		public void Generate(RenderTexture n, RenderTexture h) {
			cs.SetVector(P_PARAMS, GetParams());
			cs.SetInts(P_COUNT, n.width, n.height);
			cs.SetTexture(K_GENERATE, P_HEIGHT, h);
			cs.SetTexture(K_GENERATE, P_NORMAL, n);

			var size = cs.DispatchSize(K_GENERATE, n.width, n.height);
			cs.Dispatch(K_GENERATE, size.x, size.y, size.z);
		}

		#region IDisposable
		public void Dispose() {
		}
		#endregion

		#endregion

		#region member
		private Vector4 GetParams() {
			return new Vector4(Dxy, 0f, 0f, 0f);
		}
		#endregion
	}
}
