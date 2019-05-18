using nobnak.Gist;
using nobnak.Gist.GPUBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;

public class SimpleWaveEquation1DBuf : MonoBehaviour {
	[SerializeField]
	protected float speed = 1f;
	[SerializeField]
	protected float maxSlope = 1f;
	[SerializeField]
	protected int count = 100;
	[SerializeField]
	[Range(1, 10)]
	protected int quality = 1;

	protected WaveEquation1DBuf we;
	protected BarGraphBuf graph;
	protected Renderer rend;
	protected Validator validator = new Validator();

	protected float time;
	protected float dt;
	protected GPUList<float> v;
	protected GPUList<float> u0, u1;

	#region unity
	private void OnEnable() {
		we = new WaveEquation1DBuf();
		graph = new BarGraphBuf();
		v = new GPUList<float>();
		u0 = new GPUList<float>();
		u1 = new GPUList<float>();

		rend = GetComponent<Renderer>();

		validator.Reset();
		validator.Validation += () => {
			we.C = speed;
			we.H = 1000f / count;
			we.MaxSlope = maxSlope;

			graph.Peak = maxSlope * 2;

			time = 0f;
			dt = Mathf.Min(we.SupDt(), Time.fixedDeltaTime) / (2 * quality);
			Debug.LogFormat("Set dt={0} for speed={1}", dt, speed);

			var dw = 4f / count;
			var offset = dw * 0.5f * count;
			var dam = Mathf.RoundToInt(0.1f * count);
			v.Clear();
			u0.Clear();
			u1.Clear();
			for (var i = 0; i < count; i++) {
				var w = 1f - Mathf.Clamp01(Mathf.Abs(dw * i - offset));
				u0.Add(w);
				u1.Add(0);
				v.Add(0);
			}
		};
	}
	private void OnDisable() {
		we.Dispose();
		v.Dispose();
		u0.Dispose();
		u1.Dispose();
	}
	private void OnValidate() {
		validator.Invalidate();
	}
	private void Update() {
		validator.Validate();

		time += Time.deltaTime;
		while (time >= dt) {
			time -= dt;
			we.Next(u1, u0, v, count, dt);
			Swap();
			we.Clamp(u1, u0, count);
			Swap();
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
	#endregion
}
