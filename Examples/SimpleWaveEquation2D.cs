using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;

public class SimpleWaveEquation2D : MonoBehaviour {

	[SerializeField]
	protected float speed = 100f;
	[SerializeField]
	protected int count = 100;

	protected Stamp stamp;
	protected WaveEquation2D weq;
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
		weq = new WaveEquation2D();

		validator.Reset();
		validator.Validation += () => {
			mat = GetComponent<Renderer>().sharedMaterial;
			col = GetComponent<Collider>();

			ReleaseTextures();

			var size = weq.CeilSize(new Vector3Int(count, count, 1));
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

			weq.C = speed;
			weq.H = 100f / count;
			weq.MaxSlope = 1f;

			time = 0f;
			dt = Mathf.Min(weq.SupDt(), Time.fixedDeltaTime) * 0.5f;
			Debug.LogFormat("Set dt={0}", dt);
		};
	}
	private void OnDisable() {
		weq.Dispose();
		stamp.Dispose();
		ReleaseTextures();
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
				Debug.LogFormat("Click on uv={0}", uv);
				stamp.Draw(u0, uv, 0.5f * Vector2.one);
			}
		}

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
