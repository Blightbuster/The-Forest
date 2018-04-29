using System;
using System.Collections;
using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto
{
	
	public class WaveQueryTask : ThreadedTask
	{
		
		public WaveQueryTask(WaveSpectrumBase spectrum, float level, Vector3 offset, IEnumerable<WaveQuery> querys, Action<IEnumerable<WaveQuery>> callBack) : base(true)
		{
			IDisplacementBuffer displacementBuffer = spectrum.DisplacementBuffer;
			displacementBuffer.CopyAndCreateDisplacements(out this.m_displacements);
			this.m_querys = querys;
			this.m_callBack = callBack;
			this.m_enabled = displacementBuffer.EnabledBuffers();
			this.m_level = level;
			Vector4 invGridSizes = default(Vector4);
			invGridSizes.x = 1f / (spectrum.GridSizes.x * spectrum.GridScale);
			invGridSizes.y = 1f / (spectrum.GridSizes.y * spectrum.GridScale);
			invGridSizes.z = 1f / (spectrum.GridSizes.z * spectrum.GridScale);
			invGridSizes.w = 1f / (spectrum.GridSizes.w * spectrum.GridScale);
			this.m_scaling = new QueryGridScaling();
			this.m_scaling.invGridSizes = invGridSizes;
			this.m_scaling.choppyness = spectrum.Choppyness * spectrum.GridScale;
			this.m_scaling.scaleY = spectrum.GridScale;
			this.m_scaling.offset = offset;
			this.m_scaling.tmp = new float[QueryDisplacements.CHANNELS];
		}

		
		public override void Start()
		{
			base.Start();
		}

		
		public override IEnumerator Run()
		{
			this.RunQueries();
			this.FinishedRunning();
			return null;
		}

		
		public override void End()
		{
			base.End();
			this.m_callBack(this.m_querys);
		}

		
		private void RunQueries()
		{
			IEnumerator<WaveQuery> enumerator = this.m_querys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (base.Cancelled)
				{
					return;
				}
				WaveQuery waveQuery = enumerator.Current;
				waveQuery.result.Clear();
				if (waveQuery.mode == QUERY_MODE.DISPLACEMENT || waveQuery.mode == QUERY_MODE.POSITION)
				{
					QueryDisplacements.QueryWaves(waveQuery, this.m_enabled, this.m_displacements, this.m_scaling);
				}
				WaveQuery waveQuery2 = waveQuery;
				waveQuery2.result.height = waveQuery2.result.height + this.m_level;
			}
		}

		
		private IList<InterpolatedArray2f> m_displacements;

		
		private IEnumerable<WaveQuery> m_querys;

		
		private int m_enabled;

		
		private Action<IEnumerable<WaveQuery>> m_callBack;

		
		private float m_level;

		
		private QueryGridScaling m_scaling;
	}
}
