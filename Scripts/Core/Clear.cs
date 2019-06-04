using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem.Core {

	public class Clear : System.IDisposable {

		public const string PATH = "WaterSimulationForGames/Clear";

		public static readonly int P_COUNT = Shader.PropertyToID("_Count");
		public static readonly int P_FLOAT_TEX = Shader.PropertyToID("_FloatTex");
		public static readonly int P_FLOAT_VALUE = Shader.PropertyToID("_FloatClearValue");
		public static readonly int P_INT_TEX = Shader.PropertyToID("_IntTex");
		public static readonly int P_INT_VALUE = Shader.PropertyToID("_IntClearValue");

		public readonly int K_CLEAR_FLOAT;
		public readonly int K_CLEAR_INT;

		public ComputeShader cs { get; protected set; }

		#region interface
		public Clear() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_CLEAR_FLOAT = cs.FindKernel("ClearFloat");
			K_CLEAR_INT = cs.FindKernel("ClearInt");
		}

		public void Float(RenderTexture tex, float clearValue = 0f) {
			cs.SetInts(P_COUNT, tex.width, tex.height);
			cs.SetFloat(P_FLOAT_VALUE, clearValue);

			var cap = cs.DispatchSize(K_CLEAR_FLOAT, new Vector3Int(tex.width, tex.height, 1));
			cs.SetTexture(K_CLEAR_FLOAT, P_FLOAT_TEX, tex);
			cs.Dispatch(K_CLEAR_FLOAT, cap.x, cap.y, cap.z);
		}
		public void Int(RenderTexture tex, int clearValue = 0) {
			cs.SetInts(P_COUNT, tex.width, tex.height);
			cs.SetInt(P_INT_VALUE, clearValue);

			var cap = cs.DispatchSize(K_CLEAR_INT, new Vector3Int(tex.width, tex.height, 1));
			cs.SetTexture(K_CLEAR_INT, P_INT_TEX, tex);
			cs.Dispatch(K_CLEAR_INT, cap.x, cap.y, cap.z);
		}

		public void Dispose() {
		}
		#endregion
	}
}
