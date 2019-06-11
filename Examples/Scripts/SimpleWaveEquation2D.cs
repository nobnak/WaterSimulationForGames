using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

public class SimpleWaveEquation2D : MonoBehaviour {
	public static readonly int P_NORMAL_TEX = Shader.PropertyToID("_NormalTex");
	public static readonly int P_CAUSTICS_TEX = Shader.PropertyToID("_CausticsTex");
	public static readonly int P_PARAMS = Shader.PropertyToID("_Params");
	public static readonly int P_TMP0 = Shader.PropertyToID("_TmpTex0");
	public static readonly int P_TMP1 = Shader.PropertyToID("_TmpTex1");

	public enum OutputMode { Height = 0, Normal, Refract, Caustics, Water, Water_Screen }
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
		validator.SetCheckers(() => !transform.hasChanged);
		validator.Validation += () => {
			var c = Camera.main;
			var height = c.pixelHeight >> data.lod;
			var localScale = transform.localScale;
			var reqSize = new Vector2Int(Mathf.RoundToInt(height * localScale.x / localScale.y), height);
			wave.SetSize(reqSize.x, reqSize.y);
			wave.SetBoundary(data.boundary);
			wave.Params = data.paramset;

			transform.hasChanged = false;
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

			var depthFieldAspect = wave.DepthFieldAspect;
			var p = new Vector4(depthFieldAspect.x, depthFieldAspect.y, data.paramset.refractiveIndex, 0f);
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
					mat.SetTexture(P_NORMAL_TEX,  wave.N);
					mat.SetTexture(P_CAUSTICS_TEX, wave.C);
					mat.SetVector(P_PARAMS, p);
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
