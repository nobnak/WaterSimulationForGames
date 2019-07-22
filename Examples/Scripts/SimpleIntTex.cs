using nobnak.Gist.ObjectExt;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

namespace WaterSimulationForGames.Example {

	public class SimpleIntTex : MonoBehaviour {

		public static readonly int P_TARGET_TEX = Shader.PropertyToID("_TargetTex");

		[SerializeField]
		protected int count = 100;

		protected Material mat;
		protected RenderTexture tex;

		protected Uploader uploader;
		protected Clear clear;

		private void OnEnable() {
			uploader = new Uploader();
			clear = new Clear();

			var format = RenderTextureFormat.RInt;
			tex = new RenderTexture(count, 1, 0, format, RenderTextureReadWrite.Linear) {
				enableRandomWrite = true
			};
			tex.Create();

			mat = GetComponent<Renderer>().sharedMaterial;

			var buf = new int[count];
			for (var i = 0; i < buf.Length; i++)
				buf[i] = Mathf.RoundToInt(i);
			uploader.Upload(tex, buf);

			mat.SetTexture(P_TARGET_TEX, tex);
		}
		private void OnDisable() {
			clear.Dispose();
			uploader.Dispose();
			tex.DestroySelf();
		}
	}
}
