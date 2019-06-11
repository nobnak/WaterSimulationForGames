using nobnak.Gist.ObjectExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem.Core;

public class TestGenerateUVFromCS : MonoBehaviour {

	[System.Serializable]
	public class Dataset {
		public int size = 100;
	}

	[SerializeField]
	protected Dataset ds;

	protected Material mat;

	protected UV uv;
	protected RenderTexture rt;

	private void OnEnable() {
		uv = new UV();

		rt = new RenderTexture(ds.size, ds.size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		rt.antiAliasing = QualitySettings.antiAliasing;
		rt.enableRandomWrite = true;
		rt.Create();

		mat = GetComponent<Renderer>().sharedMaterial;

		uv.Generate(rt);
		mat.mainTexture = rt;
	}
	private void OnDisable() {
		uv.Dispose();
		rt.DestroySelf();
	}
}
