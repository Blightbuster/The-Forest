using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Player
{
	
	public class WeaponBonus : MonoBehaviour
	{
		
		public void OnEnable()
		{
			if (!this._owner)
			{
				base.StartCoroutine(this.DelayedOnEnable());
			}
			else
			{
				this.OnEnableFinal();
			}
		}

		
		private IEnumerator DelayedOnEnable()
		{
			yield return null;
			this.OnEnableFinal();
			yield break;
		}

		
		private void OnEnableFinal()
		{
			if (!this._owner)
			{
				this._owner = base.transform.root;
			}
			if (this._mode == WeaponBonus.EventMode.AttackToAttackEnd)
			{
				base.GetComponent<Collider>().enabled = false;
				if (LocalPlayer.Inventory && this._owner == LocalPlayer.Transform)
				{
					LocalPlayer.Inventory.Attacked.AddListener(new UnityAction(this.OnAttack));
					LocalPlayer.Inventory.AttackEnded.AddListener(new UnityAction(this.EndAttack));
				}
			}
			else if (this._mode == WeaponBonus.EventMode.Released)
			{
				base.GetComponent<Collider>().enabled = false;
				if (LocalPlayer.Inventory && this._owner == LocalPlayer.Transform)
				{
					LocalPlayer.Inventory.ReleasedAttack.AddListener(new UnityAction(this.OnAttack));
				}
			}
			else
			{
				base.GetComponent<Collider>().enabled = true;
			}
		}

		
		private void OnDisable()
		{
			if (LocalPlayer.Inventory)
			{
				if (this._mode == WeaponBonus.EventMode.AttackToAttackEnd)
				{
					LocalPlayer.Inventory.Attacked.RemoveListener(new UnityAction(this.OnAttack));
					LocalPlayer.Inventory.AttackEnded.RemoveListener(new UnityAction(this.EndAttack));
				}
				else if (this._mode == WeaponBonus.EventMode.Released)
				{
					LocalPlayer.Inventory.ReleasedAttack.RemoveListener(new UnityAction(this.OnAttack));
				}
				base.GetComponent<Collider>().enabled = false;
			}
		}

		
		private void OnTriggerEnter(Collider otherObject)
		{
			if (this._owner && this._owner != otherObject.transform.root && base.transform.root != otherObject.transform.root)
			{
				if (BoltNetwork.isRunning)
				{
					GameObject gameObject = otherObject.transform.root.gameObject;
					BoltEntity component = gameObject.GetComponent<BoltEntity>();
					if (component)
					{
						WeaponBonus.BonusTypes bonusType = this._bonusType;
						if (bonusType != WeaponBonus.BonusTypes.Burn)
						{
							if (bonusType != WeaponBonus.BonusTypes.Poison)
							{
								if (bonusType == WeaponBonus.BonusTypes.DouseBurn)
								{
									Burn burn = Burn.Create(GlobalTargets.OnlyServer);
									burn.Entity = component;
									burn.Send();
								}
							}
							else if (Vector3.Dot(otherObject.transform.position - base.transform.position, base.transform.forward) > 0.25f)
							{
								Poison poison = Poison.Create(GlobalTargets.OnlyServer);
								poison.Entity = component;
								poison.Send();
							}
						}
						else
						{
							Burn burn2 = Burn.Create(GlobalTargets.OnlyServer);
							burn2.Entity = component;
							burn2.Send();
						}
					}
				}
				WeaponBonus.BonusTypes bonusType2 = this._bonusType;
				if (bonusType2 != WeaponBonus.BonusTypes.Poison)
				{
					if (bonusType2 != WeaponBonus.BonusTypes.DouseBurn)
					{
						if (bonusType2 == WeaponBonus.BonusTypes.Burn)
						{
							Prefabs.Instance.SpawnFireHitPS(base.transform.position, Quaternion.LookRotation(base.transform.position - otherObject.transform.position));
							otherObject.SendMessage("Burn", SendMessageOptions.DontRequireReceiver);
							this._onHit.Invoke();
						}
					}
					else
					{
						Prefabs.Instance.SpawnFireHitPS(base.transform.position, Quaternion.LookRotation(base.transform.position - otherObject.transform.position));
						otherObject.SendMessage("Douse", SendMessageOptions.DontRequireReceiver);
						otherObject.SendMessage("Burn", SendMessageOptions.DontRequireReceiver);
						this._onHit.Invoke();
					}
				}
				else if (otherObject.CompareTag("enemyRoot") || otherObject.CompareTag("enemyCollide") || otherObject.CompareTag("animalCollide") || otherObject.CompareTag("animalRoot"))
				{
					otherObject.SendMessage("Poison", SendMessageOptions.DontRequireReceiver);
					this._onHit.Invoke();
				}
				else if (Vector3.Dot(otherObject.transform.position - base.transform.position, base.transform.forward) > 0.25f)
				{
					otherObject.SendMessage("Poison", SendMessageOptions.DontRequireReceiver);
					this._onHit.Invoke();
				}
			}
		}

		
		private void OnAttack()
		{
			base.GetComponent<Collider>().enabled = true;
		}

		
		private void EndAttack()
		{
			base.GetComponent<Collider>().enabled = false;
		}

		
		public WeaponBonus.EventMode _mode;

		
		public Transform _owner;

		
		public WeaponBonus.BonusTypes _bonusType;

		
		public UnityEvent _onHit;

		
		public enum EventMode
		{
			
			AttackToAttackEnd,
			
			Released,
			
			Passive,
			
			Auto
		}

		
		public enum BonusTypes
		{
			
			Burn,
			
			Poison,
			
			DouseBurn
		}
	}
}
