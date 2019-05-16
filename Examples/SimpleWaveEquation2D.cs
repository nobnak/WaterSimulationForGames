using nobnak.Gist;
using nobnak.Gist.ObjectExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;

public class SimpleWaveEquation2D : MonoBehaviour {

	[SerializeField]
	protected float speed = 100f;
	protected int count = 100;

	protected WaveEquation2D weq;
	protected RenderTexture v;
	protected RenderTexture u0, u1;

	protected Validator validator = new Validator();
	protected Material mat;
	protected float time;
	protected float dt;

	#region unity
	private void OnEnable() {
		weq = new WaveEquation2D();

		validator.Reset();
		validator.Validation += () => {
			mat = GetComponent<Renderer>().sharedMaterial;

			ReleaseTextures();
			var desc = new RenderTextureDescriptor(count, count, RenderTextureFormat.RFloat, 0);
			desc.enableRandomWrite = true;
			desc.sRGB = false;
			desc.useMipMap = false;
			desc.autoGenerateMips = false;
			v = new RenderTexture(desc);
			u0 = new RenderTexture(desc);
			u1 = new RenderTexture(desc);
			v.filterMode = u0.filterMode = u1.filterMode = FilterMode.Point;
			v.wrapMode = u0.wrapMode = u1.wrapMode = TextureWrapMode.Clamp;

			weq.C = speed;
			weq.H = 100f / count;
			weq.MaxSlope = 2f;

			time = 0f;
			dt = Mathf.Min(weq.SupDt(), Time.fixedDeltaTime) * 0.5f;
			Debug.LogFormat("Set dt={0}", dt);
		};
	}
	private void OnDisable() {
		weq.Dispose();
		ReleaseTextures();
	}
	private void Update() {
		validator.Validate();
		time += Time.deltaTime;
		while (time >= dt) {
			time -= dt;
			weq.Next(u1, u0, v, dt);
			Swap();
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
