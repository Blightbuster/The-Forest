using System;
using Bolt;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

public class Explode : EntityBehaviour
{
	private void Start()
	{
		base.Invoke("RunExplode", 0.1f);
		if (this.wormDamageSetup)
		{
			base.transform.GetComponent<FMOD_StudioEventEmitter>().enabled = true;
		}
		if (this.disableClientCleanUp && BoltNetwork.isClient)
		{
			return;
		}
		base.Invoke("CleanUp", 7f);
	}

	public void setRadius(float val)
	{
		this.radius = val;
	}

	private void RunExplode()
	{
		Vector3 position = base.transform.position;
		Collider[] array = Physics.OverlapSphere(position, this.radius);
		int num = 0;
		int num2 = 0;
		foreach (Collider collider in array)
		{
			bool flag = collider.CompareTag("Fish");
			bool flag2 = !flag && (collider.CompareTag("Tree") || collider.CompareTag("MidTree"));
			if (flag)
			{
				num++;
			}
			else if (flag2)
			{
				num2++;
			}
			if (!this.wormDamageSetup || !collider.GetComponent<wormHitReceiver>())
			{
				if (BoltNetwork.isClient)
				{
					if (collider.CompareTag("playerHitDetect"))
					{
						float num3 = Vector3.Distance(base.transform.position, collider.transform.position);
						collider.SendMessageUpwards("Explosion", num3, SendMessageOptions.DontRequireReceiver);
						collider.SendMessage("lookAtExplosion", base.transform.position, SendMessageOptions.DontRequireReceiver);
					}
					else if (collider.CompareTag("SmallTree") || flag2 || collider.CompareTag("BreakableWood") || collider.CompareTag("BreakableRock") || flag)
					{
						float num4 = Vector3.Distance(base.transform.position, collider.transform.position);
						if (collider.CompareTag("lb_bird") || flag)
						{
							collider.gameObject.SendMessage("Explosion", num4, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							collider.gameObject.SendMessageUpwards("Explosion", num4, SendMessageOptions.DontRequireReceiver);
						}
						collider.gameObject.SendMessage("lookAtExplosion", base.transform.position, SendMessageOptions.DontRequireReceiver);
					}
					else if (collider.CompareTag("structure"))
					{
						float distance = Vector3.Distance(base.transform.position, collider.transform.position);
						collider.gameObject.SendMessage("OnExplode", new Explode.Data
						{
							distance = distance,
							explode = this
						}, SendMessageOptions.DontRequireReceiver);
					}
					else if (collider.CompareTag("animalCollide"))
					{
						collider.gameObject.SendMessageUpwards("Explosion", SendMessageOptions.DontRequireReceiver);
					}
				}
				else
				{
					if (collider.CompareTag("enemyCollide") || collider.CompareTag("animalCollide") || collider.CompareTag("lb_bird") || collider.CompareTag("playerHitDetect") || collider.CompareTag("structure") || collider.CompareTag("SLTier1") || collider.CompareTag("SLTier2") || collider.CompareTag("SLTier3") || flag2 || collider.CompareTag("SmallTree") || collider.CompareTag("BreakableWood") || collider.CompareTag("BreakableRock") || flag || collider.CompareTag("jumpObject") || collider.CompareTag("UnderfootWood") || collider.CompareTag("UnderfootRock") || collider.CompareTag("Target") || collider.CompareTag("dummyExplode"))
					{
						float num5 = Vector3.Distance(base.transform.position, collider.transform.position);
						if (collider.CompareTag("lb_bird") || flag)
						{
							collider.gameObject.SendMessage("Explosion", num5, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							collider.gameObject.SendMessageUpwards("Explosion", num5, SendMessageOptions.DontRequireReceiver);
						}
						collider.gameObject.SendMessage("lookAtExplosion", base.transform.position, SendMessageOptions.DontRequireReceiver);
						if (num5 < this.radius)
						{
							collider.gameObject.SendMessage("OnExplode", new Explode.Data
							{
								distance = num5,
								explode = this
							}, SendMessageOptions.DontRequireReceiver);
						}
					}
					else if (collider.CompareTag("TripWireTrigger"))
					{
						collider.SendMessage("OnTripped", SendMessageOptions.DontRequireReceiver);
					}
					if (collider && collider.GetComponent<Rigidbody>())
					{
						if (!collider.gameObject.CompareTag("Tree"))
						{
							float num6 = 10000f;
							if (collider.GetComponent<logChecker>())
							{
								num6 *= 5.5f;
							}
							if (collider.CompareTag("Fish"))
							{
								num6 = 1850f;
							}
							if (!collider.CompareTag("Player") && !collider.CompareTag("PlayerNet"))
							{
								collider.GetComponent<Rigidbody>().AddExplosionForce(num6, position, this.radius, 3f, ForceMode.Force);
							}
						}
					}
				}
			}
		}
		if (num > 0)
		{
			EventRegistry.Achievements.Publish(TfEvent.Achievements.FishDynamited, num);
		}
		if (num2 > 0)
		{
			EventRegistry.Achievements.Publish(TfEvent.Achievements.TreeDynamited, num2);
		}
		if (LocalPlayer.GameObject)
		{
			float num7 = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
			LocalPlayer.GameObject.SendMessage("enableExplodeShake", num7, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void CleanUp()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public float radius = 15f;

	public float power = 10000f;

	public float damage = 400f;

	public bool wormDamageSetup;

	public bool disableClientCleanUp;

	public static float overridePowerFloat;

	public struct Data
	{
		public float distance;

		public Explode explode;
	}
}
