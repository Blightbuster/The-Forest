using System;
using System.Collections;
using UnityEngine;

namespace Pathfinding.Examples
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_v_o_agent_placer.php")]
	public class RVOAgentPlacer : MonoBehaviour
	{
		private IEnumerator Start()
		{
			yield return null;
			for (int i = 0; i < this.agents; i++)
			{
				float num = (float)i / (float)this.agents * 3.14159274f * 2f;
				Vector3 vector = new Vector3((float)Math.Cos((double)num), 0f, (float)Math.Sin((double)num)) * this.ringSize;
				Vector3 target = -vector + this.goalOffset;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.Euler(0f, num + 180f, 0f));
				RVOExampleAgent component = gameObject.GetComponent<RVOExampleAgent>();
				if (component == null)
				{
					Debug.LogError("Prefab does not have an RVOExampleAgent component attached");
					yield break;
				}
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = vector;
				component.repathRate = this.repathRate;
				component.SetTarget(target);
				component.SetColor(this.GetColor(num));
			}
			yield break;
		}

		public Color GetColor(float angle)
		{
			return AstarMath.HSVToRGB(angle * 57.2957764f, 0.8f, 0.6f);
		}

		public int agents = 100;

		public float ringSize = 100f;

		public LayerMask mask;

		public GameObject prefab;

		public Vector3 goalOffset;

		public float repathRate = 1f;

		private const float rad2Deg = 57.2957764f;
	}
}
