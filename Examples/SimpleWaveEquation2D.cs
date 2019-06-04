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
			var c = Camera.main;
			var reqSize = new Vector2Int(c.pixelWidth >> data.lod, c.pixelHeight >> data.lod);
			wave.SetSize(reqSize.y, reqSize.y);
			wave.SetBoundary(data.boundary);
			wave.Params = data.paramset;
		};
	}
	private void OnValidate() {
		validator.Invalidate();
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
					mat.SetFloat(P_ASPECT, data.paramset.height);
					mat.SetFloat(P_REFRACTIVE, data.paramset.refractiveIndex);
					break;
				case OutputMode.Caustics:
					mat.mainTexture = wave.C;
					break;
				case OutputMode.Water:
					mat.SetTexture(P_NORMAL_TEX,  wave.N);
					mat.SetTexture(P_CAUSTICS_TEX, wave.C);
					mat.SetFloat(P_ASPECT, data.paramset.height);
					mat.SetFloat(P_REFRACTIVE, data.paramset.refractiveIndex);
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
		public Texture2D boundary;
		public float intakePower = 1000f;
		[Range(0, 4)]
		public int lod = 1;
		public Wave2D.ParamPack paramset = Wave2D.ParamPack.CreateDefault();
	}
	#endregion
}
