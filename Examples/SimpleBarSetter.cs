using nobnak.Gist.Extensions.Array;
using nobnak.Gist.GPUBuffer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaterSimulationForGamesSystem.Examples {

	[ExecuteInEditMode]
	public class SimpleBarSetter : MonoBehaviour {

		[SerializeField]
		protected float[] values = new float[0];

		protected GPUList<float> buf;
		protected Renderer rend;
		protected BarGraph graph;

		#region unity
		private void OnEnable() {
			graph = new BarGraph();
			buf = new GPUList<float>();

			rend = GetComponent<Renderer>();

			Build();
		}

		private void Update() {
			rend.sharedMaterial = graph.Output;
		}
		private void OnDisable() {
			graph.Dispose();
			buf.Dispose();
		}
		private void OnValidate() {
			Build();
		}
		#endregion

		#region member
		private void Build() {
			if (buf != null && graph != null) {
				buf.Clear();
				buf.AddRange(values);
				graph.Peak = Mathf.Max(Mathf.Abs(values.Max()), Mathf.Abs(values.Min()));
				graph.Input = buf;
			}
		}
		#endregion
	}
}
