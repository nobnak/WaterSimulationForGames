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

		public Caustics() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_SCAN = cs.FindKernel("Scan");
			K_ACCUMULATE = cs.FindKernel("Accumulate");
		}

		#region interface

		#region IDisposable
		public void Dispose() {
			throw new System.NotImplementedException();
		}
		#endregion
		#endregion
	}
}
