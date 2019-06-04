using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

public class SimpleWaveEquation2D : MonoBehaviour {
	public static readonly int P_NORMAL_TEX = Shader.PropertyToID("_NormalTex");
	public static readonly int P_ASPECT = Shader.PropertyToID("_Aspect");
	public static readonly int P_CAUSTICS_TEX = Shader.PropertyToID("_CausticsTex");
	public static readonly int P_REFRACTIVE = Shader.PropertyToID("_Refractive");

	public enum OutputMode { Height = 0, Normal, Refract, Caustics_Scan, Caustics, Water }
	[SerializeField]
	protected OutputMode outputMode;
	[SerializeField]
	protected bool update;

	[SerializeField]
	protected Data data = new Data();

	[SerializeField]
	protected Material[] outputs;

	protected Stamp stamp;
	protected Wave2D wave;

	protected Validator validator = new Validator();
	protected Renderer rend;
	protected Collider col;

	#region unity
	private void OnEnable() {
		stamp = new Stamp();
		wave = new Wave2D();

		rend = GetComponent<Renderer>();
		col = GetComponent<Collider>();

		validator.Reset();
		validator.Validation += () => {
			wave.SetSize(data.count, data.count);
			wave.SetBoundary(data.boundary);

			var size = wave.Size;
			wave.SetParams(
				data.lightDir,
				data.height,
				data.normalScale,
				data.refractiveIndex,
				data.speed,
				data.maxSlope);
		};
	}
	private void OnDisable() {
		wave.Dispose();
		stamp.Dispose();
	}
	private void Update() {
		validator.Validate();

		if (Input.GetMouseButton(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (col.Raycast(ray, out hit, float.MaxValue)) {
				var uv = hit.textureCoord;
				var w = 1f * (2f * Mathf.PI) * Time.time;
				var power = data.intakePower * Mathf.Sin(w) * Time.deltaTime;
				stamp.Draw(wave.U, uv, 1e-2f * Vector2.one, power);
			}
		}

		if (update) {
			wave.Update();
		}

		var ioutput = (int)outputMode;
		if (0 <= ioutput && ioutput < outputs.Length) {
			var mat = outputs[ioutput];
			switch (outputMode) {
				default:
					mat.mainTexture = wave.U;
					break;
				case OutputMode.Normal:
					mat.mainTexture = wave.N;
					break;
				case OutputMode.Refract:
					mat.SetTexture(P_NORMAL_TEX, wave.N);
					mat.SetFloat(P_ASPECT, data.height);
					mat.SetFloat(P_REFRACTIVE, data.refractiveIndex);
					break;
				case OutputMode.Caustics:
					mat.mainTexture = wave.C;
					break;
				case OutputMode.Water:
					mat.SetTexture(P_NORMAL_TEX,  wave.N);
					mat.SetTexture(P_CAUSTICS_TEX, wave.C);
					mat.SetFloat(P_ASPECT, data.height);
					mat.SetFloat(P_REFRACTIVE, data.refractiveIndex);
					break;
			}
			rend.sharedMaterial = mat;
		}
	}
#endregion

#region member
	#endregion

	#region classes
	[System.Serializable]
	public class Data {
		public FilterMode texfilter = FilterMode.Bilinear;
		public Vector3 lightDir = new Vector3(0f, 0f, -1f);

		[Header("Height (water height / field width)")]
		public float height = 0.1f;
		public float normalScale = 1f;
		public float refractiveIndex = 1.33f;
		public float speed = 50f;
		public float maxSlope = 10f;
		public int count = 500;
		public float intakePower = 1000f;
		[Range(1, 10)]
		public int quality = 1;
		public Texture2D boundary;
	}
	#endregion
}
