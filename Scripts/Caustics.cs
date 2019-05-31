using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class Caustics : System.IDisposable {
		public const string PATH = "WaterSimulationForGames/Caustics";

		public static readonly int P_LIGHT_DIR = Shader.PropertyToID("_LightDir");
		public static readonly int P_PARAMS = Shader.PropertyToID("_Params");

		public static readonly int P_TEXEL_SIZE = Shader.PropertyToID("_TexelSize");
		public static readonly int P_NORMAL = Shader.PropertyToID("_Normal");
		public static readonly int P_CAUSTICS = Shader.PropertyToID("_Caustics");

		public static readonly int P_TMP0 = Shader.PropertyToID("_Tmp0");
		public static readonly int P_TMP1 = Shader.PropertyToID("_Tmp1");

		protected readonly int K_SCAN;
		protected readonly int K_ACCUMULATE;

		public ComputeShader cs { get; protected set; }

		protected Vector3 lightDir;
		public float Aspect { get; set; }
		public float Refractive { get; set; }

		public Caustics() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_SCAN = cs.FindKernel("Scan");
			K_ACCUMULATE = cs.FindKernel("Accumulate");

			LightDir = Vector3.down;
			Aspect = 1f;
			Refractive = 1.33f;
		}

		#region interface
		public Vector3 LightDir {
			get { return lightDir; }
			set { lightDir = value.normalized; }
		}

		public void Scan(RenderTexture tmp0, RenderTexture tmp1, RenderTexture n) {
			cs.SetVector(P_LIGHT_DIR, lightDir);
			cs.SetVector(P_PARAMS, GetParams());
			cs.SetVector(P_TEXEL_SIZE, CalculateTexelSize(n));

			cs.SetTexture(K_SCAN, P_NORMAL, n);
			cs.SetTexture(K_SCAN, P_TMP0, tmp0);
			cs.SetTexture(K_SCAN, P_TMP1, tmp1);

			var ds = cs.DispatchSize(K_SCAN, n.width, n.height);
			cs.Dispatch(K_SCAN, ds.x, ds.y, ds.z);
		}
		public void Accumulate(RenderTexture c, RenderTexture tmp0, RenderTexture tmp1) {
			cs.SetTexture(K_ACCUMULATE, P_TMP0, tmp0);
			cs.SetTexture(K_ACCUMULATE, P_TMP1, tmp1);
			cs.SetTexture(K_ACCUMULATE, P_CAUSTICS, c);

			var ds = cs.DispatchSize(K_ACCUMULATE, c.width, c.height);
			cs.Dispatch(K_ACCUMULATE, ds.x, ds.y, ds.z);
		}

		public void Generate(RenderTexture c, RenderTexture n, RenderTexture tmp0, RenderTexture tmp1) {
			Scan(tmp0, tmp1, n);
			Accumulate(c, tmp0, tmp1);
		}

		#region IDisposable
		public void Dispose() {
		}
		#endregion
		#endregion

		#region member
		private static Vector4 CalculateTexelSize(RenderTexture n) {
			return new Vector4(1f / n.width, 1f / n.height, n.width, n.height);
		}

		private Vector4 GetParams() {
			return new Vector4(Aspect, Refractive, 0f, 0f);
		}
		#endregion
	}
}
