using nobnak.Gist;
using nobnak.Gist.GPUBuffer;
using nobnak.Gist.ObjectExt;
using UnityEngine;

namespace WaterSimulationForGamesSystem {

	public class BarGraphBuf : System.IDisposable {
		public static readonly string PATH = "WaterSimulationForGames/BarGraphBuf";

		public static readonly int PROP_COLOR = Shader.PropertyToID("_Color");
		public static readonly int PROP_VALUES = Shader.PropertyToID("_Values");
		public static readonly int PROP_PARAMS = Shader.PropertyToID("_Params");

		protected Validator validator = new Validator();
		protected Color color = Color.magenta;
		protected GPUList<float> buf;
		protected float peak;
		protected Material mat;

		#region interface
		public BarGraphBuf() {
			mat = new Material(Resources.Load<Shader>(PATH));
			validator.Validation += () => {
				mat.SetColor(PROP_COLOR, color);
				mat.SetBuffer(PROP_VALUES, buf);
				mat.SetVector(PROP_PARAMS, new Vector4(
					buf.Count, 0f, 
					0.5f / Mathf.Max(peak, 1e-6f), 0.5f));
			};
		}

		public Material Output {
			get {
				validator.Validate();
				return mat;
			}
		}
		public Color BarColor {
			get { return color; }
			set {
				validator.Invalidate();
				color = value;
			}
		}
		public float Peak {
			set {
				validator.Invalidate();
				peak = value;
			}
		}
		public GPUList<float> Input {
			set {
				validator.Invalidate();
				buf = value;
			}
		}

		#region IDisposable
		public void Dispose() {
			if (mat != null) {
				mat.DestroySelf();
				mat = null;
			}
		}
		#endregion

		#endregion
	}
}