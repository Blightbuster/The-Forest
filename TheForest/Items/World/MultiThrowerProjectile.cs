using System;
using Bolt;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class MultiThrowerProjectile : EntityBehaviour<IMultiThrowerProjectileState>
	{
		
		public void InitProjectile(int itemId, Transform rendererTr)
		{
			this.AttachRenderer(rendererTr);
			base.Invoke("enableDamageGo", 0.75f);
			enableGoReceiver enableGoReceiver = (!rendererTr) ? null : rendererTr.GetComponent<enableGoReceiver>();
			if (!enableGoReceiver)
			{
				this._pickup._itemId = itemId;
			}
			else
			{
				UnityEngine.Object.Destroy(this._pickup.gameObject);
				UnityEngine.Object.Destroy(this._damage.gameObject);
				enableGoReceiver.doEnableGo();
			}
		}

		
		private void AttachRenderer(Transform rendererTr)
		{
			if (rendererTr)
			{
				rendererTr.position = rendererTr.localPosition;
				rendererTr.rotation = rendererTr.localRotation;
				rendererTr.parent = base.transform;
				rendererTr.localPosition = rendererTr.position;
				rendererTr.localRotation = rendererTr.rotation;
			}
		}

		
		public override void Attached()
		{
			if (BoltNetwork.isClient)
			{
				base.state.AddCallback("Thrower", new PropertyCallbackSimple(this.CheckEntity));
				base.state.AddCallback("AmmoId", new PropertyCallbackSimple(this.CheckAmmoId));
			}
			base.state.AddCallback("Broken", new PropertyCallbackSimple(this.doBreakReal));
		}

		
		private void doBreakReal()
		{
			Molotov componentInChildren = base.transform.GetComponentInChildren<Molotov>();
			if (componentInChildren)
			{
				componentInChildren.doBreakReal();
				Collider component = base.transform.GetComponent<Collider>();
				if (component)
				{
					component.enabled = false;
				}
			}
		}

		
		private void setStateBroken(bool set)
		{
			if (this.entity && this.entity.isAttached)
			{
				base.state.Broken = set;
			}
		}

		
		private void CheckEntity()
		{
			if (this._ammoIdReady)
			{
				this.InitProjectileMP();
			}
		}

		
		private void CheckAmmoId()
		{
			this._ammoIdReady = true;
			if (base.state.Thrower)
			{
				this.InitProjectileMP();
			}
		}

		
		private void InitProjectileMP()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				coopRockThrower component = base.state.Thrower.GetComponent<coopRockThrower>();
				Transform rendererTr = (component.Anim.rockAmmo[base.state.AmmoId - 1].transform.childCount <= 0) ? null : component.Anim.rockAmmo[base.state.AmmoId - 1].transform.GetChild(0);
				int itemId = component.Holder.AmmoLoaded[base.state.AmmoId - 1];
				this.InitProjectile(itemId, rendererTr);
			}
		}

		
		private void enableDamageGo()
		{
			if (this._damage && this._damage.gameObject)
			{
				this._damage.gameObject.SetActive(true);
			}
		}

		
		public PickUp _pickup;

		
		public thrownRockDamage _damage;

		
		public Rigidbody _rigidbody;

		
		private bool _initialized;

		
		private bool _ammoIdReady;
	}
}
