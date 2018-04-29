using System;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class ProjectileThrownListener : MonoBehaviour
	{
		
		private void OnProjectileThrown(GameObject projectileGO)
		{
			if (this._targetView.ActiveBonus == this._bonus)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._prefab);
				gameObject.transform.parent = projectileGO.transform;
				gameObject.transform.localPosition = this._position.localPosition;
				gameObject.transform.localRotation = this._position.localRotation;
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component)
				{
					component.useGravity = false;
					component.isKinematic = true;
				}
				if (gameObject.GetComponent<Molotov>())
				{
					CapsuleCollider component2 = gameObject.GetComponent<CapsuleCollider>();
					BoxCollider component3 = gameObject.GetComponent<BoxCollider>();
					if (component3)
					{
						component3.enabled = false;
					}
					Collider component4 = gameObject.transform.parent.GetComponent<Collider>();
					if (component4)
					{
						Physics.IgnoreCollision(component2, component4, true);
					}
					if (component2)
					{
						component2.radius = 3f;
						component2.center = new Vector3(component2.center.x, 0.4f, component2.center.z);
						component2.height = 1f;
					}
					if (!gameObject.GetComponent<molotovSpearFollow>())
					{
						gameObject.AddComponent<molotovSpearFollow>();
					}
				}
				if (!gameObject.GetComponent<destroyAfter>())
				{
					gameObject.AddComponent<destroyAfter>().destroyTime = 20f;
				}
				if (BoltNetwork.isRunning)
				{
					BoltEntity component5 = gameObject.GetComponent<BoltEntity>();
					if (component5 && !component5.isAttached)
					{
						BoltNetwork.Attach(component5);
					}
				}
				this._targetView.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
			}
		}

		
		public InventoryItemView _targetView;

		
		public WeaponStatUpgrade.Types _bonus;

		
		public GameObject _prefab;

		
		public Transform _position;

		
		public bool _fixMolotovCollider;
	}
}
