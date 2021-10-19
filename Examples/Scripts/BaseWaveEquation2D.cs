using nobnak.Gist;
using nobnak.Gist.Cameras;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
		protected CameraData currCameraData;

		#region unity
		protected virtual void OnEnable() {
			stamp = new Stamp();
			wave = new Wave2D();

			currCameraData = default;
			validator.Reset();
			validator.SetCheckers(() => !transform.hasChanged && currCameraData.Equals(GetCamera()));
			validator.Validation += () => {
				currCameraData = GetCamera();
				if (GetCamera() == null) return;

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
        protected virtual Vector2Int GetResolution() {
            var c = GetCamera();
            var height = (data.resolutionStandard > 0)
                ? data.resolutionStandard
                : (c.pixelHeight >> data.lod);
            //height = Mathf.RoundToInt(height * FieldSize);
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
        protected virtual float FieldSize {
            get => Mathf.Clamp((float)GetCamera().orthographicSize / data.cameraSizeStandard, 0.1f, 10f);
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
            [FormerlySerializedAs("resolution")]
            public int resolutionStandard = -1;
            public float cameraSizeStandard = 20f;
			public Wave2D.ParamPack paramset = Wave2D.ParamPack.CreateDefault();
		}
		#endregion
	}
}
