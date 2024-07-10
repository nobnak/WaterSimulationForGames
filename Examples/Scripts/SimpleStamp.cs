using nobnak.Gist.ObjectExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

namespace WaterSimulationForGames.Example {

	public class SimpleStamp : MonoBehaviour {

		[SerializeField]
		protected float scale = 1f;
		[SerializeField]
		protected float density = 1f;

		protected Stamp stamp;
		protected RenderTexture canvas;

		protected Material mat;
		protected Collider col;

		#region unity
		private void OnEnable() {
			stamp = new Stamp();
			canvas = new RenderTexture(512, 512, 0) {
				hideFlags = HideFlags.DontSave,
			};

			mat = GetComponent<Renderer>().sharedMaterial;
			col = GetComponent<Collider>();

			mat.mainTexture = canvas;
		}
		private void OnDisable() {
			stamp.Dispose();
			canvas.DestroySelf();
		}
		private void Update() {
			if (Input.GetMouseButton(0)) {
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (col.Raycast(ray, out hit, float.MaxValue)) {
					var uv = hit.textureCoord;
					stamp.Draw(canvas, uv, scale * Vector2.one, density);
					Debug.LogFormat("Draw on uv={0}", uv);
				}
			}
		}
		#endregion
	}
}
