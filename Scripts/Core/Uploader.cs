using nobnak.Gist.Extensions.Array;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.GPUBuffer;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaterSimulationForGamesSystem.Core {

	public class Uploader : System.IDisposable {

		public const string PATH = "WaterSimulationForGames/Uploader";

		public static readonly int P_COUNT = Shader.PropertyToID("_Count");

		public static readonly int P_FLOAT_TEX = Shader.PropertyToID("_FloatTex");
		public static readonly int P_FLOAT_VALUES = Shader.PropertyToID("_FloatValues");

		public static readonly int P_INT_TEX = Shader.PropertyToID("_IntTex");
		public static readonly int P_INT_VALUES = Shader.PropertyToID("_IntValues");

		public readonly int K_UPLOAD_FLOAT;
		public readonly int K_UPLOAD_INT;

		public ComputeShader cs { get; protected set; }

		#region interface
		public Uploader() {
			cs = Resources.Load<ComputeShader>(PATH);
			K_UPLOAD_FLOAT = cs.FindKernel("UploadFloat");
			K_UPLOAD_INT = cs.FindKernel("UploadInt");
		}

		public void Upload(RenderTexture tex, IList<float> values) {
			using (var buf = new GPUList<float>(values)) {
				cs.SetInts(P_COUNT, tex.width, tex.height);

				var cap = cs.DispatchSize(K_UPLOAD_FLOAT, new Vector3Int(tex.width, tex.height, 1));
				cs.SetBuffer(K_UPLOAD_FLOAT, P_FLOAT_VALUES, buf);
				cs.SetTexture(K_UPLOAD_FLOAT, P_FLOAT_TEX, tex);
				cs.Dispatch(K_UPLOAD_FLOAT, cap.x, cap.y, cap.z);
			}
		}
		public void Upload(RenderTexture tex, IList<int> values) {
			using (var buf = new GPUList<int>(values)) {

				cs.SetInts(P_COUNT, tex.width, tex.height);

				var cap = cs.DispatchSize(K_UPLOAD_INT, new Vector3Int(tex.width, tex.height, 1));
				cs.SetBuffer(K_UPLOAD_INT, P_INT_VALUES, buf);
				cs.SetTexture(K_UPLOAD_INT, P_INT_TEX, tex);
				cs.Dispatch(K_UPLOAD_INT, cap.x, cap.y, cap.z);
			}
		}

		public void Dispose() {
		}
		#endregion
	}
}
