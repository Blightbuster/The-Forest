using System;
using UnityEngine;

public class switchToRagdoll : MonoBehaviour
{
	private void Start()
	{
		this.ragDoll = base.GetComponent<clsragdollify>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			this.ragDoll.metgoragdoll(default(Vector3));
			if (this.alsoDestroy.Length > 0)
			{
				foreach (GameObject gameObject in this.alsoDestroy)
				{
					if (gameObject)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
			if (this.destroyParent)
			{
				UnityEngine.Object.Destroy(base.transform.parent.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public GameObject[] alsoDestroy;

	public bool destroyParent;

	private clsragdollify ragDoll;
}
