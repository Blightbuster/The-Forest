using System;
using System.Collections;
using UnityEngine;

namespace Pathfinding.Examples
{
	
	[HelpURL("http:
	public class RVOAgentPlacer : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			yield return null;
			for (int i = 0; i < this.agents; i++)
			{
				float angle = (float)i / (float)this.agents * 3.14159274f * 2f;
				Vector3 pos = new Vector3((float)Math.Cos((double)angle), 0f, (float)Math.Sin((double)angle)) * this.ringSize;
				Vector3 antipodal = -pos + this.goalOffset;
				GameObject go = UnityEngine.Object.Instantiate(this.prefab, Vector3.zero, Quaternion.Euler(0f, angle + 180f, 0f)) as GameObject;
				RVOExampleAgent ag = go.GetComponent<RVOExampleAgent>();
				if (ag == null)
				{
					Debug.LogError("Prefab does not have an RVOExampleAgent component attached");
					yield break;
				}
				go.transform.parent = base.transform;
				go.transform.position = pos;
				ag.repathRate = this.repathRate;
				ag.SetTarget(antipodal);
				ag.SetColor(this.GetColor(angle));
			}
			yield break;
		}

		
		public Color GetColor(float angle)
		{
			return AstarMath.HSVToRGB(angle * 57.2957764f, 0.8f, 0.6f);
		}

		
		private const float rad2Deg = 57.2957764f;

		
		public int agents = 100;

		
		public float ringSize = 100f;

		
		public LayerMask mask;

		
		public GameObject prefab;

		
		public Vector3 goalOffset;

		
		public float repathRate = 1f;
	}
}
