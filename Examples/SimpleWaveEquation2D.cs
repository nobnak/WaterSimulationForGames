using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;

public class SimpleWaveEquation2D : MonoBehaviour {

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

	protected Stamp stamp;
	protected Clear clear;
	protected WaveEquation2D wave;
	protected RenderTexture v;
	protected RenderTexture u0, u1;

	protected Validator validator = new Validator();
	protected Material mat;
	protected Collider col;
	protected float time;
	protected float dt;

	#region unity
	private void OnEnable() {
		stamp = new Stamp();
		clear = new Clear();
		wave = new WaveEquation2D();

		validator.Reset();
		validator.Validation += () => {
			mat = GetComponent<Renderer>().sharedMaterial;
			col = GetComponent<Collider>();

			ReleaseTextures();

			var size = wave.CeilSize(new Vector3Int(count, count, 1));
			Debug.LogFormat("Set size={0}", size);
			var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
			v = new RenderTexture(size.x, size.y, 0, format) {
				enableRandomWrite = true,
				useMipMap = false,
				autoGenerateMips = false,
				anisoLevel = 0
			};
			u0 = new RenderTexture(v.descriptor);
			u1 = new RenderTexture(v.descriptor);

			v.filterMode = u0.filterMode = u1.filterMode = FilterMode.Point;
			v.wrapMode = u0.wrapMode = u1.wrapMode = TextureWrapMode.Clamp;

			foreach (var r in new RenderTexture[] { v, u0, u1}) {
				using (new RenderTextureActivator(r)) {
					clear.Int(r);
				}
			}

			wave.C = speed;
			wave.H = 1000f / v.width;
			wave.MaxSlope = maxSlope;

			time = 0f;
			dt = Mathf.Min(wave.SupDt(), 0.1f) / (2 * quality);
			Debug.LogFormat("Set dt={0}", dt);
		};
	}
	private void OnDisable() {
		wave.Dispose();
		clear.Dispose();
		stamp.Dispose();
		ReleaseTextures();
	}
	private void Update() {
		validator.Validate();

		if (Input.GetMouseButton(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (col.Raycast(ray, out hit, float.MaxValue)) {
				var uv = hit.textureCoord;
				var w = 3f * (2f * Mathf.PI) * Time.time;
				stamp.Draw(u0, uv, 0.01f * Vector2.one, Mathf.Sin(w));
			}
		}

		if (update) {
			time += Time.deltaTime;
			while (time >= dt) {
				time -= dt;
				wave.Next(u1, u0, v, dt);
				Swap();
				wave.Clamp(u1, u0, v);
				Swap();
			}
		}

		mat.mainTexture = u0;
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
