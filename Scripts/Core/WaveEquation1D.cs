using nobnak.Gist.Extensions.GPUExt;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaterSimulationForGamesSystem.Core {

	public class WaveEquation1D : System.IDisposable {

		public const string PATH = "WaterSimulationForGames/WaveEquation1D";

		public readonly static int P_COUNT = Shader.PropertyToID("_Count");
		public readonly static int P_B = Shader.PropertyToID("_B");
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

			Dt = 1f;
			Damp = 0f;
		}

		public float Dt { get; set; }
		public float Damp { get; set; }

		public void Next(RenderTexture u1, Texture u0, RenderTexture v, RenderTexture b) {
			cs.SetInt(P_COUNT, u1.width);
			cs.SetVector(P_Params, Params());
			cs.SetTexture(K_NEXT, P_B, b);
			cs.SetTexture(K_NEXT, P_V, v);
			cs.SetTexture(K_NEXT, P_U0, u0);
			cs.SetTexture(K_NEXT, P_U1, u1);

			var cap = cs.DispatchSize(K_NEXT, new Vector3Int(u1.width, 1, 1));
			cs.Dispatch(K_NEXT, cap.x, cap.y, cap.z);
		}
		public void Clamp(RenderTexture u1, Texture u0, RenderTexture v, RenderTexture b) {
			cs.SetInt(P_COUNT, u1.width);
			cs.SetVector(P_Params, Params());
			cs.SetTexture(K_CLAMP, P_B, b);
			cs.SetTexture(K_CLAMP, P_V, v);
			cs.SetTexture(K_CLAMP, P_U0, u0);
			cs.SetTexture(K_CLAMP, P_U1, u1);

			var cap = cs.DispatchSize(K_CLAMP, new Vector3Int(u1.width, 1, 1));
			cs.Dispatch(K_CLAMP, cap.x, cap.y, cap.z);
		}

		#region IDisposable
		public void Dispose() {
		}
		#endregion

		#endregion

		#region member
		private Vector4 Params() {
			var plist = new Vector4(Dt, Damp, 0, 0);
			return plist;
		}
		#endregion
	}
}
