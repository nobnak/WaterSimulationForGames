using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class WaveEquation2D : System.IDisposable {

		public const string PATH = "WaterSimulationForGames/WaveEquation2D";

		public readonly static int P_COUNT = Shader.PropertyToID("_Count");
		public readonly static int P_B = Shader.PropertyToID("_B");
		public readonly static int P_V = Shader.PropertyToID("_V");
		public readonly static int P_U0 = Shader.PropertyToID("_U0");
		public readonly static int P_U1 = Shader.PropertyToID("_U1");
		public readonly static int P_Params = Shader.PropertyToID("_Params");

		public readonly int K_NEXT;
		public readonly int K_CLAMP;

		public ComputeShader cs { get; protected set; }

		#region interface
		public WaveEquation2D() {
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
		public void Next(RenderTexture u1, Texture u0, RenderTexture v, RenderTexture b, float dt) {
			var size = new Vector3Int(v.width, v.height, 1);
			var cap = DispatchSize(size);
			cs.SetInts(P_COUNT, size.x, size.y);
			cs.SetVector(P_Params, Params(dt));
			cs.SetTexture(K_NEXT, P_B, b);
			cs.SetTexture(K_NEXT, P_V, v);
			cs.SetTexture(K_NEXT, P_U0, u0);
			cs.SetTexture(K_NEXT, P_U1, u1);
			cs.Dispatch(K_NEXT, cap.x, cap.y, cap.z);
		}

		public void Clamp(RenderTexture u1, Texture u0, RenderTexture v, RenderTexture b) {
			var size = new Vector3Int(u0.width, u0.height, 1);
			var cap = DispatchSize(size);
			cs.SetInts(P_COUNT, size.x, size.y);
			cs.SetVector(P_Params, Params());
			cs.SetTexture(K_CLAMP, P_B, b);
			cs.SetTexture(K_CLAMP, P_V, v);
			cs.SetTexture(K_CLAMP, P_U0, u0);
			cs.SetTexture(K_CLAMP, P_U1, u1);
			cs.Dispatch(K_CLAMP, cap.x, cap.y, cap.z);
		}
		public float SupDt() {
			return H / C;
		}
		public Vector3Int DispatchSize(Vector3Int size) {
			return cs.DispatchSize(K_NEXT, size);
		}
		public Vector3Int CeilSize(Vector3Int size) {
			return cs.CeilSize(K_NEXT, size);
		}
		#region IDisposable
		public void Dispose() {
		}
		#endregion

		#endregion

		#region member
		private Vector4 Params(float dt = 1f) {
			return new Vector4(C * C / (H * H), H, dt, MaxSlope * H);
		}
		#endregion
	}
}
