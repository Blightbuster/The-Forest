using System;
using TheForest.Buildings.World;
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
			if (!BoltNetwork.isClient)
			{
				for (int i = base.transform.childCount - 1; i >= 0; i--)
				{
					Transform child = base.transform.GetChild(i);
					BuildingHealth component = child.GetComponent<BuildingHealth>();
					if (component)
					{
						child.parent = null;
						component.Collapse(child.position);
					}
					else
					{
						FoundationHealth component2 = child.GetComponent<FoundationHealth>();
						if (component2)
						{
							child.parent = null;
							component2.Collapse(child.position);
						}
						else if (BoltNetwork.isRunning && child.GetComponent<BoltEntity>())
						{
							child.parent = null;
							destroyAfter destroyAfter = child.gameObject.AddComponent<destroyAfter>();
							destroyAfter.destroyTime = 1f;
						}
					}
				}
			}
			bool flag = true;
			MeshRenderer[] componentsInChildren = (this._destroyTarget ?? base.gameObject).GetComponentsInChildren<MeshRenderer>();
			int mask = LayerMask.GetMask(new string[]
			{
				"Default",
				"ReflectBig",
				"Prop",
				"PropSmall",
				"PickUp"
			});
			foreach (MeshRenderer renderer in componentsInChildren)
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
					destroyAfter destroyAfter2 = gameObject.AddComponent<destroyAfter>();
					destroyAfter2.destroyTime = 2.5f;
				}
			}
			if (BoltNetwork.isClient)
			{
				BoltEntity component3 = base.GetComponent<BoltEntity>();
				if (component3 && component3.isAttached && !component3.isOwner)
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
