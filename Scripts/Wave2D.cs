using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.Extensions.ReflectionExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

namespace WaterSimulationForGamesSystem {

	public class Wave2D : System.IDisposable {

		public enum OutputMode { Height = 0, Normal, Refract, Caustics_Scan, Caustics, Water }

		protected FilterMode texfilter = FilterMode.Bilinear;

		protected Vector2Int size = Vector2Int.zero;
        protected ParamPack pd;

		protected Clear clear;
		protected Caustics caustics;
		protected Normal2D normal;
		protected Uploader uploader;
		protected Boundary boundary;
		protected WaveEquation2D wave;

		protected RenderTexture v;
		protected RenderTexture u0, u1;
		protected RenderTexture b;
		protected RenderTexture n;
		protected RenderTexture c, tmp0, tmp1;

		protected Validator validator = new Validator();
		protected Renderer rend;
		protected Collider col;
		protected float time;

		#region interface

		#region IDisposable
		public void Dispose() {
			wave.Dispose();
			clear.Dispose();
			normal.Dispose();
			caustics.Dispose();
			uploader.Dispose();
			boundary.Dispose();
			ReleaseTextures();
		}
        #endregion

        #region properties
        public Boundary.ColorIndex BoundaryColorIndex { get; set; } = default;
        public Vector2Int Size { get { return size; } }
		public RenderTexture U { get { return u0; } }
		public RenderTexture V { get { return v; } }
		public RenderTexture B { get { return b; } }
		public RenderTexture N { get { return n; } }
		public RenderTexture C { get { return c; } }

		public RenderTexture Tmp0 { get { return tmp0; } }
		public RenderTexture Tmp1 { get { return tmp1; } }

        public ParamPack Params {
			get { return pd; }
			set {
				if (pd != value) {
					pd = value;
					validator.Invalidate();
				}
			}
		}

		public Vector2 DepthFieldAspect {
			get { return pd.depthFieldAspect * new Vector2((float)size.y / size.x, 1f); }
		}
        #endregion

        #region overrides
        public override string ToString() {
            return $"{GetType().Name} :\n\n{size}\n\n{pd}";
        }
        #endregion

        public Wave2D() {
			clear = new Clear();
			normal = new Normal2D();
			caustics = new Caustics();
			uploader = new Uploader();
			boundary = new Boundary();
			wave = new WaveEquation2D();

			validator.Reset();
			validator.Validation += () => {
				time = 0f;
				wave.Dt = pd.dt;
				wave.Damp = Mathf.Clamp01(pd.damping * wave.Dt);

				normal.Dxy = pd.normalScale;


				caustics.Refractive = pd.refractiveIndex;
				caustics.DepthFieldAspect = DepthFieldAspect;
				caustics.LightDir = pd.lightDir;
			};
		}

		public void SetBoundary(Texture2D srcImage) {
			var foundsrc = (srcImage != null && srcImage.isReadable);
			boundary.Convert(b, foundsrc ? srcImage : Texture2D.whiteTexture, BoundaryColorIndex);
		}

		public void SetSize(int width, int height, bool quantize = false) {
			if (width < 4 || height < 4)
				throw new System.ArgumentException(
					string.Format("Invalid size : {0}x{1}", width, height));

			if (quantize) {
				var qs = wave.CeilSize(width, height);
				width = qs.x;
				height = qs.y;
			}
			var sizeNext = new Vector2Int(width, height);
			if (size != sizeNext) {
				validator.Invalidate();
				size = sizeNext;
				Debug.LogFormat("{0}: Set texture size={1}", GetType().Name, size);

				ReleaseTextures();
				CreateTextures();
				ClearTextures();
			}
		}

		public void Update() {
			validator.Validate();

			time += Time.deltaTime * pd.speed;
			while (time >= wave.Dt && wave.Dt > 0) {
				time -= wave.Dt;
				wave.Next(u1, u0, v, b);
				Swap();
				if (pd.damping > 0f) {
					wave.Clamp(u1, u0, v, b);
					Swap();
				}
			}

			normal.Generate(n, u0);

			caustics.Generate(c, n, tmp0, tmp1);
		}
#endregion

#region member
		private void CreateTextures() {
			var formatf = RenderTextureFormat.RFloat;
			var formati = RenderTextureFormat.RInt;
			v = new RenderTexture(size.x, size.y, 0, formatf) {
				enableRandomWrite = true,
				useMipMap = false
			};
			u0 = new RenderTexture(v.descriptor);
			u1 = new RenderTexture(v.descriptor);
			b = new RenderTexture(size.x, size.y, 0, formati) {
				enableRandomWrite = true,
				useMipMap = false,
			};
			n = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGBFloat) {
				enableRandomWrite = true,
				useMipMap = false,
			};
			tmp0 = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGBFloat) {
				enableRandomWrite = true,
				useMipMap = false,
			};
			tmp1 = new RenderTexture(tmp0.descriptor) {
				enableRandomWrite = true,
				useMipMap = false,
			};
			c = new RenderTexture(v.descriptor) {
				enableRandomWrite = true,
				useMipMap = false,
			};
			v.Create();
			u0.Create();
			u1.Create();
			n.Create();
			b.Create();
			tmp0.Create();
			tmp1.Create();
			c.Create();

			v.filterMode = u0.filterMode = u1.filterMode = n.filterMode = c.filterMode = texfilter;
			v.wrapMode = u0.wrapMode = u1.wrapMode = n.wrapMode = c.wrapMode = tmp0.wrapMode = tmp1.wrapMode = TextureWrapMode.Clamp;
		}

		private void ClearTextures() {
			foreach (var r in new RenderTexture[] { v, u0, u1 })
				clear.Float(r);
			clear.Int(b);
		}

		private void ReleaseTextures() {
			v.DestroySelf();
			u0.DestroySelf();
			u1.DestroySelf();

			b.DestroySelf();
			n.DestroySelf();

			tmp0.DestroySelf();
			tmp1.DestroySelf();
			c.DestroySelf();
		}
		private void Swap() {
			var tmp = u1;
			u1 = u0;
			u0 = tmp;
		}
#endregion

#region definitions
        [System.Serializable]
		public struct ParamPack : System.IEquatable<ParamPack> {
			public Vector3 lightDir;
			[Header("Depth-Field aspect (water depth / field height)")]
			public float depthFieldAspect;
			public float normalScale;
			public float refractiveIndex;
			public float speed;
			public float dt;
			public float damping;

#region static
			public static ParamPack CreateDefault() {
                return new ParamPack() {
					lightDir = new Vector3(0f, 0f, -1f),
					depthFieldAspect = 0.05f,
					normalScale = 1f,
					refractiveIndex = 0.752f,
					speed = 30f,
					dt = 0.01f,
					damping = 0.01f
				};
			}
			public static bool operator ==(ParamPack a, ParamPack b) {
				return a.Equals(b);
			}
			public static bool operator !=(ParamPack a, ParamPack b) {
				return !a.Equals(b);
			}
#endregion

#region IEquatable
			public bool Equals(ParamPack o) {
				return lightDir == o.lightDir
					&& depthFieldAspect == o.depthFieldAspect
					&& normalScale == o.normalScale
					&& refractiveIndex == o.refractiveIndex
					&& speed == o.speed
					&& dt == o.dt
					&& damping == o.damping;
			}
#endregion

#region Object
			public override bool Equals(object obj) {
				return (obj is ParamPack) && Equals((ParamPack)obj);
			}
			public override int GetHashCode() {
				var v = 4049;
				v = (v + lightDir.GetHashCode()) * 2801;
				v = (v + depthFieldAspect.GetHashCode()) * 2801;
				v = (v + normalScale.GetHashCode()) * 2801;
				v = (v + refractiveIndex.GetHashCode()) * 2801;
				v = (v + speed.GetHashCode()) * 2801;
				v = (v + dt.GetHashCode()) * 2801;
				v = (v + damping.GetHashCode()) * 2801;
				return v;
			}
            public override string ToString() {
                var tmp = new StringBuilder();

                tmp.Append($"{GetType().Name}:\n");
                foreach (var f in GetType().GetInstancePublicFields()) {
                    tmp.Append($"\t{f.Name}={f.GetValue(this)},\n");
                }
                return tmp.ToString();
            }
            #endregion
        }
#endregion
	}
}
