using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

namespace WaterSimulationForGames.Example {

	public class SimpleWaveEquation2D : BaseWaveEquation2D {
		public static readonly int P_NORMAL_TEX = Shader.PropertyToID("_NormalTex");
		public static readonly int P_CAUSTICS_TEX = Shader.PropertyToID("_CausticsTex");
		public static readonly int P_PARAMS = Shader.PropertyToID("_Params");
		public static readonly int P_TMP0 = Shader.PropertyToID("_TmpTex0");
		public static readonly int P_TMP1 = Shader.PropertyToID("_TmpTex1");

		public enum OutputMode { Height = 0, Normal, Refract, Caustics, Water, Water_Screen }
		[SerializeField]
		protected OutputMode outputMode;
		[SerializeField]
		protected Material[] outputs;

		protected Renderer rend;
		protected Collider col;

		#region unity
		protected override void OnEnable() {
			base.OnEnable();

			rend = GetComponent<Renderer>();
			col = GetComponent<Collider>();
		}
		protected override void Update() {
			base.Update();

			if (Input.GetMouseButton(0)) {
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (col.Raycast(ray, out hit, float.MaxValue)) {
					var uv = hit.textureCoord;
					var w = 1f * (2f * Mathf.PI) * Time.time;
					var power = data.intakePower; // * Mathf.Sin(w); // * Time.deltaTime;
					stamp.Draw(wave.U, uv, data.intakeSize * Vector2.one, power);
				}
			}

			var ioutput = (int)outputMode;
			if (0 <= ioutput && ioutput < outputs.Length) {
				var mat = outputs[ioutput];
				var p = GetParameters();
				switch (outputMode) {
					default:
						mat.mainTexture = wave.U;
						break;
					case OutputMode.Normal:
						mat.mainTexture = wave.N;
						break;
					case OutputMode.Refract:
						mat.SetTexture(P_NORMAL_TEX, wave.N);
						mat.SetVector(P_PARAMS, p);
						break;
					case OutputMode.Caustics:
						mat.mainTexture = wave.C;
						mat.SetTexture(P_TMP0, wave.Tmp0);
						mat.SetTexture(P_TMP1, wave.Tmp1);
						break;
					case OutputMode.Water:
					case OutputMode.Water_Screen:
						mat.SetTexture(P_NORMAL_TEX, wave.N);
						mat.SetTexture(P_CAUSTICS_TEX, wave.C);
						mat.SetVector(P_PARAMS, p);
						break;
				}
				rend.sharedMaterial = mat;
			}
		}
		#endregion
	}
}
