using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.GPUBuffer;
using nobnak.Gist.ObjectExt;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;

public class SimpleWaveEquation2D : MonoBehaviour {
	public static readonly int P_NORMAL_TEX = Shader.PropertyToID("_NormalTex");
	public static readonly int P_ASPECT = Shader.PropertyToID("_Aspect");

	public enum OutputMode { Height = 0, Normal, Refract }
	[SerializeField]
	protected FilterMode texfilter = FilterMode.Point;
	[SerializeField]
	protected OutputMode outputMode;

	[SerializeField]
	protected float aspect = 0.1f;
	[SerializeField]
	protected float normalScale = 1f;
	[SerializeField]
	protected float speed = 100f;
	[SerializeField]
	protected float maxSlope = 1f;
	[SerializeField]
	protected int count = 100;
	[Range(1, 10)]
	[SerializeField]
	protected int quality = 1;
	[SerializeField]
	protected bool update;
	[SerializeField]
	protected Texture2D boundary;

	[SerializeField]
	protected Material[] outputs;

	protected Stamp stamp;
	protected Clear clear;
	protected Normal2D normal;
	protected Uploader uploader;
	protected WaveEquation2D wave;
	protected RenderTexture v;
	protected RenderTexture u0, u1;
	protected RenderTexture b;
	protected RenderTexture n;

	protected Validator validator = new Validator();
	protected Renderer rend;
	protected Collider col;
	protected float time;
	protected float dt;

	#region unity
	private void OnEnable() {
		stamp = new Stamp();
		clear = new Clear();
		normal = new Normal2D();
		uploader = new Uploader();
		wave = new WaveEquation2D();

		validator.Reset();
		validator.Validation += () => {
			rend = GetComponent<Renderer>();
			col = GetComponent<Collider>();

			ReleaseTextures();

			var countQuant = wave.CeilSize(new Vector3Int(count, count, 1));
			Debug.LogFormat("Set size={0}", countQuant);
			var formatf = RenderTextureFormat.RFloat;
			var formati = RenderTextureFormat.RInt;
			v = new RenderTexture(countQuant.x, countQuant.y, 0, formatf) {
				enableRandomWrite = true,
			};
			u0 = new RenderTexture(v.descriptor);
			u1 = new RenderTexture(v.descriptor);
			b = new RenderTexture(countQuant.x, countQuant.y, 0, formati) {
				enableRandomWrite = true,
			};
			n = new RenderTexture(countQuant.x, countQuant.y, 0, RenderTextureFormat.ARGBHalf) {
				enableRandomWrite = true,
			};
			v.Create();
			u0.Create();
			u1.Create();
			n.Create();
			b.Create();

			v.filterMode = u0.filterMode = u1.filterMode = n.filterMode = texfilter;
			v.wrapMode = u0.wrapMode = u1.wrapMode = n.wrapMode = TextureWrapMode.Clamp;

			foreach (var r in new RenderTexture[] { v, u0, u1})
				clear.Float(r);
			clear.Int(b);

			if (boundary != null && boundary.isReadable) {
				var duvdxy = new Vector2(1f / (b.width - 1), 1f / (b.height - 1));
				var bupload = new List<int>(b.width * b.height);
				for (var y = 0; y < b.height; y++) {
					for (var x = 0; x < b.width; x++) {
						var c = boundary.GetPixelBilinear(
							(x + 0.5f) * duvdxy.x, 
							(y + 0.5f) * duvdxy.y);
						bupload.Add(c.r > 0.5f ? 1 : 0);
					}
				}
				uploader.Upload(b, bupload);
			}

			time = 0f;
			Debug.LogFormat("Set dt={0}", dt);
		};
	}
	private void OnDisable() {
		wave.Dispose();
		clear.Dispose();
		stamp.Dispose();
		normal.Dispose();
		uploader.Dispose();
		ReleaseTextures();
	}
	private void Update() {
		validator.Validate();

		wave.C = speed;
		wave.Dxy = 1e3f / u0.width;
		wave.MaxSlope = maxSlope;

		normal.Dxy = normalScale * wave.Dxy;
		dt = Mathf.Min(wave.SupDt(), 0.1f) / (2 * quality);

		if (Input.GetMouseButton(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (col.Raycast(ray, out hit, float.MaxValue)) {
				var uv = hit.textureCoord;
				var w = 3f * (2f * Mathf.PI) * Time.time;
				stamp.Draw(u0, uv, 1e-2f * Vector2.one, Mathf.Sin(w));
			}
		}

		if (update) {
			time += Time.deltaTime;
			while (time >= dt) {
				time -= dt;
				wave.Next(u1, u0, v, b, dt);
				Swap();
				wave.Clamp(u1, u0, v, b);
				Swap();
			}

			normal.Generate(n, u0);
		}

		var ioutput = (int)outputMode;
		if (0 <= ioutput && ioutput < outputs.Length) {
			var mat = outputs[ioutput];
			switch (outputMode) {
				default:
					mat.mainTexture = u0;
					break;
				case OutputMode.Normal:
					mat.mainTexture = n;
					break;
				case OutputMode.Refract:
					mat.SetTexture(P_NORMAL_TEX, n);
					mat.SetFloat(P_ASPECT, aspect);
					break;
			}
			rend.sharedMaterial = mat;
		}
	}
#endregion

#region member
	private void ReleaseTextures() {
		v.DestroySelf();
		u0.DestroySelf();
		u1.DestroySelf();
	}
	private void Swap() {
		var tmp = u1;
		u1 = u0;
		u0 = tmp;
	}
#endregion
}
