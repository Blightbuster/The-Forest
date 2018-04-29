using System;
using TheForest.Utils.Enums;
using UnityEngine;

namespace TheForest.Buildings.Utils
{
	
	public class CollapseStructure : MonoBehaviour
	{
		
		private void Start()
		{
			if (this._collapseOnStart)
			{
				this.Collapse();
			}
		}

		
		public void Collapse()
		{
			MeshRenderer[] array = (!this._destroyTarget) ? base.GetComponentsInChildren<MeshRenderer>() : this._destroyTarget.GetComponentsInChildren<MeshRenderer>();
			bool flag = true;
			int mask = LayerMask.GetMask(new string[]
			{
				"Default",
				"ReflectBig",
				"Prop",
				"PropSmall",
				"PickUp"
			});
			foreach (MeshRenderer renderer in array)
			{
				GameObject gameObject = renderer.gameObject;
				if (gameObject.activeInHierarchy && (1 << gameObject.layer & mask) != 0)
				{
					Transform transform = renderer.transform;
					transform.parent = null;
					if (gameObject == base.gameObject)
					{
						flag = false;
					}
					gameObject.layer = this._detachedLayer;
					if (!gameObject.GetComponent<Collider>())
					{
						CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
						capsuleCollider.radius = 0.1f;
						capsuleCollider.height = 1.5f;
						capsuleCollider.direction = (int)this._capsuleDirection;
					}
					Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
					if (rigidbody)
					{
						rigidbody.AddForce((transform.position.normalized + Vector3.up) * (2.5f * this._destructionForceMultiplier), ForceMode.Impulse);
						rigidbody.AddRelativeTorque(Vector3.up * (2f * this._destructionForceMultiplier), ForceMode.Impulse);
					}
					destroyAfter destroyAfter = gameObject.AddComponent<destroyAfter>();
					destroyAfter.destroyTime = 2.5f;
				}
			}
			if (BoltNetwork.isClient)
			{
				BoltEntity component = base.GetComponent<BoltEntity>();
				if (component && component.isAttached && !component.isOwner)
				{
					return;
				}
			}
			if (flag)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		public bool _collapseOnStart = true;

		
		public GameObject _destroyTarget;

		
		public float _destructionForceMultiplier = 1f;

		
		public int _detachedLayer = 31;

		
		public CapsuleDirections _capsuleDirection;
	}
}
