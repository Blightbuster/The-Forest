using System;
using System.Collections.Generic;
using TheForest.UI.Interfaces;
using UnityEngine;

namespace TheForest.UI
{
	public class VirtualCursorSnapNode : MonoBehaviour
	{
		private void Awake()
		{
			this._groupTester = base.GetComponent<IVirtualCursorSnapNodeGroupTester>();
			if (this._groupTester != null)
			{
				this.GroupedNodes = new List<VirtualCursorSnapNode>();
			}
		}

		private void OnEnable()
		{
			if (ForestVR.Prototype)
			{
				return;
			}
			this.CanBeSnapedTo = true;
			if (this.CheckLayer())
			{
				this._layer.RegisterNode(this);
			}
		}

		private void OnDisable()
		{
			this.CanBeSnapedTo = false;
			if (this.CheckLayer())
			{
				this._layer.UnregisterNode(this);
			}
		}

		private bool CheckLayer()
		{
			if (!this._layer)
			{
				this._layer = base.GetComponentInParent<VirtualCursorSnapLayer>();
			}
			return this._layer;
		}

		public virtual void Refresh()
		{
			Collider component = base.GetComponent<Collider>();
			if (component)
			{
				if (component is BoxCollider)
				{
					this._center = ((BoxCollider)component).center;
				}
				else if (component is SphereCollider)
				{
					this._center = ((SphereCollider)component).center;
				}
				else if (component is CapsuleCollider)
				{
					this._center = ((CapsuleCollider)component).center;
				}
				else
				{
					this._center = component.bounds.center;
				}
			}
		}

		protected bool SearchValidCenter(Vector3 baseCenter, float radius)
		{
			Collider component = base.GetComponent<Collider>();
			for (int i = 0; i < 72; i++)
			{
				Vector3 vector;
				if (i <= 36)
				{
					vector = baseCenter + Vector3.RotateTowards(this._layer.transform.forward, -this._layer.transform.forward, 0.08726647f * (float)i, 0f) * radius;
				}
				else
				{
					vector = baseCenter + Vector3.RotateTowards(-this._layer.transform.forward, this._layer.transform.forward, 0.08726647f * (float)i, 0f) * radius;
				}
				RaycastHit raycastHit;
				if (Physics.Raycast(Camera.main.transform.position, vector - Camera.main.transform.position, out raycastHit, 100f, this._layers))
				{
					if (raycastHit.collider == component)
					{
						Debug.DrawLine(Camera.main.transform.position, vector, Color.green);
						this._center = base.transform.InverseTransformPoint(vector);
						return true;
					}
					Debug.DrawLine(Camera.main.transform.position, vector, Color.cyan);
				}
				else
				{
					Debug.DrawLine(Camera.main.transform.position, vector, Color.yellow);
				}
			}
			return false;
		}

		private void SetHovered(bool on)
		{
			if (this._layer)
			{
				this._layer.NodeHovered(this, on);
			}
		}

		public virtual bool CanBeSnapedTo { get; set; }

		public virtual Vector3 Center
		{
			get
			{
				return base.transform.TransformPoint(this._center);
			}
			set
			{
				this._center = value;
			}
		}

		public VirtualCursorSnapNode MasterGroupNode { get; set; }

		public List<VirtualCursorSnapNode> GroupedNodes { get; private set; }

		public IVirtualCursorSnapNodeGroupTester GroupTester
		{
			get
			{
				return this._groupTester;
			}
		}

		public VirtualCursorSnapLayer _layer;

		public Vector3 _center;

		[Header("Controls")]
		public bool _refresh;

		public bool _checkCenter;

		public LayerMask _layers;

		private IVirtualCursorSnapNodeGroupTester _groupTester;
	}
}
