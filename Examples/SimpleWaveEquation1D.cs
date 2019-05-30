using nobnak.Gist;
using nobnak.Gist.GPUBuffer;
using nobnak.Gist.ObjectExt;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;

public class SimpleWaveEquation1D : MonoBehaviour {
	[SerializeField]
	protected float speed = 1f;
	[SerializeField]
	protected float size = 1000f;
	[SerializeField]
	protected float maxSlope = 1f;
	[SerializeField]
	protected int count = 100;
	[SerializeField]
	[Range(1, 10)]
	protected int quality = 1;
	[SerializeField]
	protected bool update;

	protected Stamp stamp;
	protected Clear clear;
	protected Uploader uploader;
	protected WaveEquation1D wave;
	protected BarGraph graph;
	protected Renderer rend;
	protected Collider col;
	protected Validator validator = new Validator();

	protected float time;
	protected float dt;
	protected RenderTexture v;
	protected RenderTexture u0, u1;
	protected RenderTexture b;

	#region unity
	private void OnEnable() {
		wave = new WaveEquation1D();
		graph = new BarGraph();
		stamp = new Stamp();
		clear = new Clear();
		uploader = new Uploader();

		rend = GetComponent<Renderer>();
		col = GetComponent<Collider>();

		validator.Reset();
		validator.Validation += () => {
			ReleaseBuffers();

			var formatfloat = RenderTextureFormat.RFloat;
			var formatint = RenderTextureFormat.RInt;
			v = new RenderTexture(count, 1, 0, formatfloat) {
				enableRandomWrite = true
			};
			u0 = new RenderTexture(v.descriptor);
			u1 = new RenderTexture(v.descriptor);
			b = new RenderTexture(count, 1, 0, formatint, RenderTextureReadWrite.Linear) {
				enableRandomWrite = true
			};
			v.filterMode = u0.filterMode = u1.filterMode = FilterMode.Point;
			v.wrapMode = u0.wrapMode = u1.wrapMode = TextureWrapMode.Clamp;
			v.Create();
			u0.Create();
			u1.Create();
			b.Create();
			
			foreach (var r in new RenderTexture[] { v, u0, u1 })
				clear.Float(r);

			var bs = new int[count];
			var mod = Mathf.RoundToInt(count / 2f);
			for (var i = 0; i < bs.Length; i++)
				bs[i] = ((i % mod) == 0) ? 1 : 0;
			uploader.Upload(b, bs);

			wave.C = speed;
			wave.Dxy = size / count;
			wave.MaxSlope = maxSlope;

			graph.Peak = maxSlope * 2;

			time = 0f;
			dt = Mathf.Min(wave.SupDt(), 0.1f) / (2 * quality);
			Debug.LogFormat("Set dt={0} for speed={1}", dt, speed);
		};
	}

	private void OnDisable() {
		wave.Dispose();
		stamp.Dispose();
		clear.Dispose();
		ReleaseBuffers();
	}
	private void OnValidate() {
		//validator.Invalidate();
	}
	private void Update() {
		validator.Validate();

		if (Input.GetMouseButton(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (col.Raycast(ray, out hit, float.MaxValue)) {
				var uv = hit.textureCoord;
				uv.y = 0.5f;
				var w = 2f * Mathf.PI * Time.time;
				stamp.Draw(u0, uv, 0.02f * Vector2.one, 0.1f * Mathf.Sin(w));
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
		}

		graph.Input = u0;
		rend.sharedMaterial = graph.Output;
	}
#endregion

#region member
	private void Swap() {
		var tmp = u1;
		u1 = u0;
		u0 = tmp;
	}
	private void ReleaseBuffers() {
		b.DestroySelf();
		v.DestroySelf();
		u0.DestroySelf();
		u1.DestroySelf();
	}

#endregion
}
