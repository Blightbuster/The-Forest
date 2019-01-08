using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

public class hangingRopeTrigger : MonoBehaviour
{
	private void Start()
	{
		this.health = 2;
		base.gameObject.tag = "Rope";
	}

	private void Hit(int damage)
	{
		this.health -= damage;
		if (this.health <= 0)
		{
			LocalPlayer.GameObject.SendMessage("releaseFromHanging", SendMessageOptions.DontRequireReceiver);
			this.rootGo.transform.parent = null;
			base.transform.GetComponent<Collider>().enabled = false;
			this.rope.SetActive(false);
			this.rope_frayed.SetActive(true);
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._planeAxeId) && LocalPlayer.vrPlayerControl.RightHand.GetStandardInteractionButtonDown())
		{
			LocalPlayer.ScriptSetup.pmControl.SendEvent("axeAttack");
		}
	}

	public int health = 2;

	public GameObject rootGo;

	public GameObject rope;

	public GameObject rope_frayed;

	public Transform vrPosTransform;
}
