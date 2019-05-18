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
	protected WaveEquation1D we;
	protected BarGraph graph;
	protected Renderer rend;
	protected Collider col;
	protected Validator validator = new Validator();

	protected float time;
	protected float dt;
	protected RenderTexture v;
	protected RenderTexture u0, u1;

	#region unity
	private void OnEnable() {
		we = new WaveEquation1D();
		graph = new BarGraph();
		stamp = new Stamp();
		clear = new Clear();

		rend = GetComponent<Renderer>();
		col = GetComponent<Collider>();

		validator.Reset();
		validator.Validation += () => {
			we.C = speed;
			we.H = 1000f / count;
			we.MaxSlope = maxSlope;

			graph.Peak = maxSlope * 2;

			time = 0f;
			dt = Mathf.Min(we.SupDt(), 0.1f) / (2 * quality);
			Debug.LogFormat("Set dt={0} for speed={1}", dt, speed);

			ReleaseBuffers();

			var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
			v = new RenderTexture(count, 1, 0, format) {
				enableRandomWrite = true
			};
			u0 = new RenderTexture(v.descriptor);
			u1 = new RenderTexture(v.descriptor);
			v.filterMode = u0.filterMode = u1.filterMode = FilterMode.Point;
			v.Create();
			u0.Create();
			u1.Create();
			
			foreach (var r in new RenderTexture[] { v, u0, u1 }) {
				using (new RenderTextureActivator(r)) {
					clear.Do(r);
				}
			}
		};
	}

	private void OnDisable() {
		we.Dispose();
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
				Debug.LogFormat("Click on uv={0}", uv);
				stamp.Draw(u0, uv, 0.1f * Vector2.one);
			}
		}

		if (update) {
			time += Time.deltaTime;
			while (time >= dt) {
				time -= dt;
				we.Next(u1, u0, v, dt);
				Swap();
				we.Clamp(u1, u0);
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
		v.DestroySelf();
		u0.DestroySelf();
		u1.DestroySelf();
	}

#endregion
}
