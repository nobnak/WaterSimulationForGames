using nobnak.Gist;
using nobnak.Gist.ObjectExt;
using UnityEngine;
using WaterSimulationForGamesSystem.Core;

public class SimpleWaveEquation1D : MonoBehaviour {
	[SerializeField]
	protected float speed = 1f;
	[SerializeField]
	protected float dt = 1f;
	[SerializeField]
	protected float maxSlope = 1f;
	[SerializeField]
	protected float damping = 1e-3f;
	[SerializeField]
	protected int count = 100;
	[SerializeField]
	protected float intakePower = 1f;
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
			SetSize(count);

			var bs = new int[count];
			var mod = Mathf.RoundToInt(count / 2f);
			//for (var i = 0; i < bs.Length; i++)
			//	bs[i] = ((i % mod) == 0) ? 1 : 0;
			uploader.Upload(b, bs);

			wave.Dt = dt;
			wave.Damp = damping * dt;

			graph.Peak = maxSlope * 2;

			time = 0f;
		};
	}

	private void OnDisable() {
		wave.Dispose();
		stamp.Dispose();
		clear.Dispose();
		ReleaseBuffers();
	}
	private void OnValidate() {
		validator.Invalidate();
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
				var power = intakePower;
				stamp.Draw(u0, uv, 0.02f * Vector2.one, power);
			}
		}

		if (update) {
			time += Time.deltaTime * speed;
			while (time >= dt) {
				time -= dt;
				wave.Next(u1, u0, v, b);
				Swap();
				if (wave.Damp > 0) {
					wave.Clamp(u1, u0, v, b);
					Swap();
				}
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

	private void SetSize(int count) {
		if (v == null || v.width != count) {
			ReleaseBuffers();
			CreateBuffers(count);
			ClearBuffers();
		}
	}

	private void ClearBuffers() {
		foreach (var r in new RenderTexture[] { v, u0, u1 })
			clear.Float(r);
	}

	private void CreateBuffers(int count) {
		var formatfloat = RenderTextureFormat.RFloat;
		var formatint = RenderTextureFormat.RInt;
		v = new RenderTexture(count, 1, 0, formatfloat) {
			enableRandomWrite = true,
			useMipMap = false
		};
		u0 = new RenderTexture(v.descriptor);
		u1 = new RenderTexture(v.descriptor);
		b = new RenderTexture(count, 1, 0, formatint, RenderTextureReadWrite.Linear) {
			enableRandomWrite = true,
			useMipMap = false
		};
		v.filterMode = u0.filterMode = u1.filterMode = FilterMode.Point;
		v.wrapMode = u0.wrapMode = u1.wrapMode = TextureWrapMode.Clamp;
		v.Create();
		u0.Create();
		u1.Create();
		b.Create();
	}

	#endregion
}
