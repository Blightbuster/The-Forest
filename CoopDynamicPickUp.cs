using System;
using System.Collections;
using UnityEngine;


public class CoopDynamicPickUp : CoopBase<IDynamicPickup>
{
	
	public override void Attached()
	{
		this.MultiplayerPriority = 1f;
		base.state.Transform.SetTransforms(base.transform);
		base.entity.Freeze(false);
		if (base.entity.isOwner)
		{
			if (this.destroyAfter > 0f)
			{
				base.StartCoroutine(this.DestroyIn(this.destroyAfter));
			}
		}
		else
		{
			if (this.disablePhysics)
			{
				foreach (Collider collider in base.GetComponentsInChildren<Collider>())
				{
					if (!collider.isTrigger)
					{
						UnityEngine.Object.Destroy(collider);
					}
				}
				foreach (Rigidbody rigidbody in base.GetComponentsInChildren<Rigidbody>())
				{
					if (!rigidbody.isKinematic)
					{
						UnityEngine.Object.Destroy(rigidbody);
					}
				}
			}
			for (int k = 0; k < this.disableOnProxies.Length; k++)
			{
				if (this.disableOnProxies[k])
				{
					this.disableOnProxies[k].enabled = false;
				}
			}
		}
	}

	
	private IEnumerator DestroyIn(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	
	[SerializeField]
	private MonoBehaviour[] disableOnProxies = new MonoBehaviour[0];

	
	public float destroyAfter = 600f;

	
	public bool disablePhysics = true;
}
