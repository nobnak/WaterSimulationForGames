using nobnak.Gist;
using nobnak.Gist.Extensions.GPUExt;
using nobnak.Gist.ObjectExt;
using System.Collections.Generic;
using UnityEngine;
using WaterSimulationForGamesSystem;
using WaterSimulationForGamesSystem.Core;

namespace WaterSimulationForGamesSystem {

	public class Wave2D : System.IDisposable {

		public enum OutputMode { Height = 0, Normal, Refract, Caustics_Scan, Caustics, Water }

		protected FilterMode texfilter = FilterMode.Bilinear;

		protected Vector2Int size = Vector2Int.zero;
		protected ParamPack pd;

		protected Stamp stamp;
		protected Clear clear;
		protected Caustics caustics;
		protected Normal2D normal;
		protected Uploader uploader;
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
		protected float dt;

		#region interface

		#region IDisposable
		public void Dispose() {
			wave.Dispose();
			clear.Dispose();
			stamp.Dispose();
			normal.Dispose();
			caustics.Dispose();
			uploader.Dispose();
			ReleaseTextures();
		}
		#endregion

		#region properties
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

		public Wave2D() {
			stamp = new Stamp();
			clear = new Clear();
			normal = new Normal2D();
			caustics = new Caustics();
			uploader = new Uploader();
			wave = new WaveEquation2D();

			validator.Reset();
			validator.Validation += () => {
				time = 0f;

				wave.C = pd.speed;
				wave.Dxy = 1e3f / size.y;
				dt = Mathf.Min(wave.SupDt(), 0.1f) / (2f * pd.quality);
				wave.Dt = dt;
				Debug.LogFormat("Set dt={0}", dt);
				if (dt < 1e-3f)
					throw new System.InvalidOperationException(string.Format("dt={0} is too small", dt));

				normal.Dxy = pd.normalScale * wave.Dxy;


				caustics.Refractive = pd.refractiveIndex;
				caustics.DepthFieldAspect = DepthFieldAspect;
				caustics.LightDir = pd.lightDir;
			};
		}

		public void SetBoundary(Texture2D boundary) {
			if (boundary != null && boundary.isReadable) {
				var duvdxy = new Vector2(1f / (b.width - 1), 1f / (b.height - 1));
				var bupload = new List<int>(b.width * b.height);
				for (var y = 0; y < b.height; y++) {
					for (var x = 0; x < b.width; x++) {
						var c = boundary.GetPixelBilinear(
							(x + 0.5f) * duvdxy.x,
							(y + 0.5f) * duvdxy.y);
						bupload.Add(c.r > 0.5f ? 1 : 0);
					}
				}
				uploader.Upload(b, bupload);
			}
		}

		public void SetSize(int width, int height, bool quantize = true) {
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

			time += Time.deltaTime;
			while (time >= dt) {
				time -= dt;
				wave.Next(u1, u0, v, b);
				Swap();
				wave.Clamp(u1, u0, v, b);
				Swap();
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
			};
			u0 = new RenderTexture(v.descriptor);
			u1 = new RenderTexture(v.descriptor);
			b = new RenderTexture(size.x, size.y, 0, formati) { enableRandomWrite = true };
			n = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGBFloat) {
				enableRandomWrite = true,
			};
			tmp0 = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGBFloat) { enableRandomWrite = true };
			tmp1 = new RenderTexture(tmp0.descriptor) { enableRandomWrite = true };
			c = new RenderTexture(v.descriptor) { enableRandomWrite = true };
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
			public int quality;

			#region static
			public static ParamPack CreateDefault() {
				return new ParamPack() {
					lightDir = new Vector3(0f, 0f, -1f),
					depthFieldAspect = 0.1f,
					normalScale = 1f,
					refractiveIndex = 0.752f,
					speed = 50f,
					quality = 1,
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
					&& quality == o.quality;
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
				v = (v + quality.GetHashCode()) * 2801;
				return v;
			}
			#endregion
		}
		#endregion
	}
}
