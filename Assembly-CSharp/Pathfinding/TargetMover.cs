using System;
using UnityEngine;

namespace Pathfinding
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_target_mover.php")]
	public class TargetMover : MonoBehaviour
	{
		public void Start()
		{
			this.cam = Camera.main;
			this.ais = UnityEngine.Object.FindObjectsOfType<RichAI>();
			this.ais2 = UnityEngine.Object.FindObjectsOfType<AIPath>();
			this.ais3 = UnityEngine.Object.FindObjectsOfType<AILerp>();
			base.useGUILayout = false;
		}

		public void OnGUI()
		{
			if (this.onlyOnDoubleClick && this.cam != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2)
			{
				this.UpdateTargetPosition();
			}
		}

		private void Update()
		{
			if (!this.onlyOnDoubleClick && this.cam != null)
			{
				this.UpdateTargetPosition();
			}
		}

		public void UpdateTargetPosition()
		{
			Vector3 vector = Vector3.zero;
			bool flag = false;
			RaycastHit raycastHit;
			if (this.use2D)
			{
				vector = this.cam.ScreenToWorldPoint(Input.mousePosition);
				vector.z = 0f;
				flag = true;
			}
			else if (Physics.Raycast(this.cam.ScreenPointToRay(Input.mousePosition), out raycastHit, float.PositiveInfinity, this.mask))
			{
				vector = raycastHit.point;
				flag = true;
			}
			if (flag && vector != this.target.position)
			{
				this.target.position = vector;
				if (this.onlyOnDoubleClick)
				{
					if (this.ais != null)
					{
						for (int i = 0; i < this.ais.Length; i++)
						{
							if (this.ais[i] != null)
							{
								this.ais[i].UpdatePath();
							}
						}
					}
					if (this.ais2 != null)
					{
						for (int j = 0; j < this.ais2.Length; j++)
						{
							if (this.ais2[j] != null)
							{
								this.ais2[j].SearchPath();
							}
						}
					}
					if (this.ais3 != null)
					{
						for (int k = 0; k < this.ais3.Length; k++)
						{
							if (this.ais3[k] != null)
							{
								this.ais3[k].SearchPath();
							}
						}
					}
				}
			}
		}

		public LayerMask mask;

		public Transform target;

		private RichAI[] ais;

		private AIPath[] ais2;

		private AILerp[] ais3;

		public bool onlyOnDoubleClick;

		public bool use2D;

		private Camera cam;
	}
}
