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

		protected Vector2Int size = new Vector2Int(100, 100);
		// Depth Scale (water height / field width)
		protected Vector3 lightDir = new Vector3(0f, 0f, -1f);
		protected float height = 0.1f;
		protected float normalScale = 1f;
		protected float refractiveIndex = 1.33f;
		protected float speed = 50f;
		protected float maxSlope = 10f;

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

		public Vector3 LightDir {
			get { return lightDir; }
			set {
				if (lightDir != value) {
					lightDir = value.normalized;
					validator.Invalidate();
				}
			}
		}
		public float Height {
			get { return height; }
			set {
				if (value > 0) {
					height = value;
					validator.Invalidate();
				}
			}
		}
		public float NormalScale {
			get { return normalScale; }
			set {
				if (normalScale != value) {
					normalScale = value;
					validator.Invalidate();
				}
			}
		}
		public float RefractiveIndex {
			get { return refractiveIndex; }
			set {
				if (refractiveIndex != value) {
					refractiveIndex = value;
					validator.Invalidate();
				}
			}
		}
		public float Speed {
			get { return speed; }
			set {
				if (speed != value) {
					speed = value;
					validator.Invalidate();
				}
			}
		}
		public float MaxSlope {
			get { return maxSlope; }
			set {
				if (maxSlope != value) {
					maxSlope = value;
					validator.Invalidate();
				}
			}
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

				if (u0 == null || u0.width != size.x || u0.height != size.y) {
					ReleaseTextures();
					CreateTextures();
					ClearTextures();
				}

				time = 0f;

				wave.C = speed;
				wave.Dxy = 1e3f / size.x;
				wave.MaxSlope = maxSlope;

				normal.Dxy = normalScale * wave.Dxy;

				var quality = 1;
				dt = Mathf.Min(wave.SupDt(), 0.1f) / (2f * quality);

				caustics.Refractive = refractiveIndex;
				caustics.Aspect = height;
				caustics.LightDir = lightDir;
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
			}
		}
		public void SetParams(
			Vector3 lightDir,
			float height,
			float normalScale,
			float refractiveIndex,
			float speed,
			float maxSlope) {

			validator.Invalidate();
			this.lightDir = lightDir;
			this.height = height;
			this.normalScale = normalScale;
			this.refractiveIndex = refractiveIndex;
			this.speed = speed;
			this.maxSlope = maxSlope;
		}

		public void Update() {
			validator.Validate();

			time += Time.deltaTime;
			while (time >= dt) {
				time -= dt;
				wave.Next(u1, u0, v, b, dt);
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
	}
}
