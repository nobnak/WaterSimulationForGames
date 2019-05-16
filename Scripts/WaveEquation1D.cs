using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class WaveEquation1D : System.IDisposable {

		public const string PATH = "WaterSimulationForGames/WaveEquation1D";

		public readonly static int P_COUNT = Shader.PropertyToID("_Count");
		public readonly static int P_V = Shader.PropertyToID("_V");
		public readonly static int P_U0 = Shader.PropertyToID("_U0");
		public readonly static int P_U1 = Shader.PropertyToID("_U1");
		public readonly static int P_Params = Shader.PropertyToID("_Params");

		public readonly int K_NEXT;
		public readonly int K_CLAMP;

		protected ComputeShader cs;

		#region interface
		public WaveEquation1D() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_NEXT = cs.FindKernel("Next");
			K_CLAMP = cs.FindKernel("Clamp");

			C = 1f;
			H = 1f;
			MaxSlope = 1f;
		}

		public float C { get ; set; }
		public float H { get; set; }
		public float MaxSlope { get; set; }
		public void Next(ComputeBuffer u1, ComputeBuffer u0, ComputeBuffer v, int count, float dt) {
			var cap = cs.DispatchSize(K_NEXT, new Vector3Int(count, 1, 1));
			cs.SetInt(P_COUNT, count);
			cs.SetVector(P_Params, Params(dt));
			cs.SetBuffer(K_NEXT, P_V, v);
			cs.SetBuffer(K_NEXT, P_U0, u0);
			cs.SetBuffer(K_NEXT, P_U1, u1);
			cs.Dispatch(K_NEXT, cap.x, cap.y, cap.z);
		}
		public void Clamp(ComputeBuffer u1, ComputeBuffer u0, int count) {
			var cap = cs.DispatchSize(K_CLAMP, new Vector3Int(count, 1, 1));
			cs.SetInt(P_COUNT, count);
			cs.SetVector(P_Params, Params());
			cs.SetBuffer(K_CLAMP, P_U0, u0);
			cs.SetBuffer(K_CLAMP, P_U1, u1);
			cs.Dispatch(K_CLAMP, cap.x, cap.y, cap.z);
		}
		public float SupDt() {
			return H / C;
		}
		#region IDisposable
		public void Dispose() {
		}
		#endregion

		#endregion

		#region member
		private Vector4 Params(float dt = 1f) {
			return new Vector4(C * C, 1f / (H * H), dt, MaxSlope * H);
		}
		#endregion
	}
}
