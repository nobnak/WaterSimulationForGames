using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

public class BaseWaveEquation2D : MonoBehaviour {

	[SerializeField]
	protected bool update;
	[SerializeField]
	protected Data data = new Data();

	protected Stamp stamp;
	protected Wave2D wave;

	protected Validator validator = new Validator();

	#region unity
	protected virtual void OnEnable() {
		stamp = new Stamp();
		wave = new Wave2D();

		validator.Reset();
		validator.SetCheckers(() => !transform.hasChanged);
        validator.Validation += () => {
            Vector2Int reqSize = GetResolution();
            wave.SetSize(reqSize.x, reqSize.y);
            wave.SetBoundary(data.boundary);
            wave.Params = data.paramset;

            transform.hasChanged = false;
        };
	}
    protected virtual void OnValidate() {
		validator.Invalidate();
	}
	protected virtual void OnDisable() {
		wave.Dispose();
		stamp.Dispose();
	}
	protected virtual void Update() {
        validator.Validate();
        if (update) {
            wave.Update();
        }
    }
    #endregion

    #region member
    protected virtual Vector2Int GetResolution() {
        var c = Camera.main;
        var height = c.pixelHeight >> data.lod;
        var localScale = transform.localScale;
        var reqSize = new Vector2Int(Mathf.RoundToInt(height * localScale.x / localScale.y), height);
        return reqSize;
    }
    protected virtual Vector4 GetParameters() {
        var depthFieldAspect = wave.DepthFieldAspect;
        var p = new Vector4(depthFieldAspect.x, depthFieldAspect.y, data.paramset.refractiveIndex, 0f);
        return p;
    }
    #endregion

    #region classes
    [System.Serializable]
	public class Data {
		public FilterMode texfilter = FilterMode.Bilinear;
		public Texture2D boundary;
		public float intakePower = 10;
        public float intakeSize = 0.01f;
        [Range(0, 4)]
		public int lod = 1;
		public Wave2D.ParamPack paramset = Wave2D.ParamPack.CreateDefault();
	}
	#endregion
}
