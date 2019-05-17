using nobnak.Gist.ObjectExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class Stamp : System.IDisposable {

		public const string DEFAULT_TEXTURE = "WaterSimulationForGames/Brush";
		public const string SHADER = "WaterSimulationForGames/Stamp";

		public static readonly int PROP_AMP = Shader.PropertyToID("_Amp");
		public static readonly int PROP_UV_MAT = Shader.PropertyToID("_UvMat");

		protected Material mat;

		#region interface
		public Stamp() {
			mat = new Material(Resources.Load<Shader>(SHADER));
			Brush = Resources.Load<Texture2D>(DEFAULT_TEXTURE);
		}

		public Texture2D Brush { get; set; }

		public void Draw(RenderTexture target, Vector2 center, Vector2 scale, float amp = 1f) {
			mat.SetFloat(PROP_AMP, amp);

			var translation = new Vector3(-center.x + 0.5f, -center.y + 0.5f, 0f);
			var uvmat =
				Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0f))
				* Matrix4x4.Scale(new Vector3(1f / scale.x, 1f / scale.y, 1f))
				* Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f))
				* Matrix4x4.Translate(translation);
			mat.SetMatrix(PROP_UV_MAT, uvmat);

			Graphics.Blit(Brush, target, mat);
		}

		#region IDisposable
		public void Dispose() {
			mat.DestroySelf();
		}
		#endregion
		#endregion
	}
}
