using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

namespace WaterSimulationForGames.Example {

	public class BaseWaveEquation2D : MonoBehaviour {

		public event System.Action Validated;

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
			validator.SetCheckers(() => !transform.hasChanged && CheckValidation());
			validator.Validation += () => {
				Vector2Int reqSize = GetResolution();
				wave.SetSize(reqSize.x, reqSize.y);
				wave.SetBoundary(data.boundary);
				wave.Params = data.paramset;

				transform.hasChanged = false;

                Debug.Log($"{wave}");
			};
			validator.Validated += () => {
				Validated?.Invoke();
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
            var c = GetCamera();
            var es = wave.CurrElasticData;
            es.fieldSize = Mathf.Clamp((float)c.orthographicSize / data.cameraSizeStandard, 0.1f, 10f);
            wave.CurrElasticData = es;

			validator.Validate();
			if (update) {
				wave.Update();
			}
		}
		#endregion

		#region interface
        public virtual void SetBoundary(Texture2D boundary) {
            validator.Invalidate();
            data.boundary = boundary;
        }
		public virtual Wave2D WaveSimulator { get { return wave; } }
        #endregion

        #region member
        protected virtual bool CheckValidation() {
            return true;
        }
        protected virtual Vector2Int GetResolution() {
            var c = GetCamera();
            var height = (data.resolution > 0)
                ? data.resolution
                : (c.pixelHeight >> data.lod);
            var aspect = (float)c.pixelWidth / c.pixelHeight;
            var width = Mathf.RoundToInt(height * aspect);
            var res = new Vector2Int(width, height);
            Debug.Log($"Water resolution={res}");
            return res;
		}
		protected virtual Vector4 GetParameters() {
			var depthFieldAspect = wave.DepthFieldAspect;
			var p = new Vector4(depthFieldAspect.x, depthFieldAspect.y, data.paramset.refractiveIndex, 0f);
			return p;
        }
        protected virtual Camera GetCamera() {
            return Camera.main;
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
            public int resolution = -1;
            public float cameraSizeStandard = 20f;
			public Wave2D.ParamPack paramset = Wave2D.ParamPack.CreateDefault();
		}
		#endregion
	}
}
