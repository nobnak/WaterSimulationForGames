using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class Clear : System.IDisposable {

		public const string PATH = "WaterSimulationForGames/Clear";

		public static readonly int P_COUNT = Shader.PropertyToID("_Count");
		public static readonly int P_TEX= Shader.PropertyToID("_Tex");
		public static readonly int P_CLEAR_VALUE = Shader.PropertyToID("_ClearValue");

		public readonly int K_CLEAR2D;

		public ComputeShader cs { get; protected set; }

		#region interface
		public Clear() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_CLEAR2D = cs.FindKernel("Clear2D");
		}

		public void Do(RenderTexture tex, float clearValue = 0f) {
			var cap = cs.DispatchSize(K_CLEAR2D, new Vector3Int(tex.width, tex.height, 1));
			cs.SetInts(P_COUNT, tex.width, tex.height);
			cs.SetFloat(P_CLEAR_VALUE, clearValue);
			cs.SetTexture(K_CLEAR2D, P_TEX, tex);
			cs.Dispatch(K_CLEAR2D, cap.x, cap.y, cap.z);
		}

		public void Dispose() {
		}
		#endregion
	}
}
