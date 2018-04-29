using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Items.World;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;


[DoNotSerializePublic]
public class ResetTraps : EntityBehaviour<IBuildingState>
{
	
	private void Awake()
	{
		this.Ghost = base.transform.parent.gameObject;
		base.enabled = false;
		Scene.ActiveMB.StartCoroutine(this.DelayedAwake());
		if (!PlayerPreferences.ShowHud)
		{
			this.SheenIcon.SetActive(false);
			this.PickupIcon.SetActive(false);
			if (this.ResetIcon)
			{
				this.ResetIcon.SetActive(false);
			}
		}
	}

	
	private IEnumerator DelayedAwake()
	{
		yield return null;
		if (this.ResetIcon && !base.GetComponent<DelayedActionSheenBillboard>())
		{
			DelayedActionSheenBillboard delayedIcon = base.gameObject.AddComponent<DelayedActionSheenBillboard>();
			delayedIcon._icon = this.ResetIcon.GetComponent<SheenBillboard>();
			delayedIcon.enabled = this.ItemCost.gameObject.activeSelf;
		}
		base.enabled = this.ItemCost.gameObject.activeSelf;
		yield break;
	}

	
	private void Update()
	{
		bool flag;
		if (this.ItemCost)
		{
			if (this.ResetIcon)
			{
				if (this.PickupIcon.activeSelf != (!this.ItemCost.IsCompleted && PlayerPreferences.ShowHud))
				{
					this.PickupIcon.SetActive(!this.ItemCost.IsCompleted && PlayerPreferences.ShowHud);
				}
				if (this.ResetIcon.activeSelf != (this.ItemCost.IsCompleted && PlayerPreferences.ShowHud))
				{
					this.ResetIcon.SetActive(this.ItemCost.IsCompleted && PlayerPreferences.ShowHud);
				}
				flag = (this.ItemCost.IsCompleted && TheForest.Utils.Input.GetButtonAfterDelay("Take", 3f, false));
			}
			else
			{
				flag = this.ItemCost.IsCompleted;
			}
		}
		else
		{
			flag = TheForest.Utils.Input.GetButtonAfterDelay("Take", 4f, false);
		}
		if (flag)
		{
			base.enabled = false;
			this.Restore();
		}
	}

	
	private void GrabEnter(GameObject grabber)
	{
		this.SheenIcon.SetActive(false);
		if (this.ItemCost)
		{
			this.PickupIcon.SetActive(!this.ItemCost.IsCompleted && PlayerPreferences.ShowHud);
			this.ResetIcon.SetActive(this.ItemCost.IsCompleted && PlayerPreferences.ShowHud);
		}
		else
		{
			this.PickupIcon.SetActive(true && PlayerPreferences.ShowHud);
		}
		if (this.ItemCost)
		{
			this.ItemCost.gameObject.SetActive(true);
		}
		base.enabled = true;
	}

	
	private void GrabExit()
	{
		base.enabled = false;
		this.SheenIcon.SetActive(true);
		this.PickupIcon.SetActive(false);
		if (this.ResetIcon)
		{
			this.ResetIcon.SetActive(false);
		}
		if (this.ItemCost)
		{
			this.ItemCost.gameObject.SetActive(false);
		}
	}

	
	private void Restore()
	{
		LocalPlayer.Sfx.PlayTwinkle();
		if (BoltNetwork.isRunning)
		{
			ResetTrap resetTrap = ResetTrap.Raise(GlobalTargets.OnlyServer);
			resetTrap.TargetTrap = this.entity;
			resetTrap.Send();
		}
		else
		{
			this.RestoreSafe();
		}
	}

	
	public void RestoreSafe()
	{
		if (this.TrapTrigger)
		{
			this.TrapTrigger.SendMessage("releaseTrapped", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			trapTrigger componentInChildren = base.transform.root.GetComponentInChildren<trapTrigger>();
			if (componentInChildren != null)
			{
				componentInChildren.releaseTrapped();
			}
		}
		if (!BoltNetwork.isRunning)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(this.Built, this.Ghost.transform.position, this.Ghost.transform.rotation);
			BuildingHealth componentInParent = base.GetComponentInParent<BuildingHealth>();
			quickBuild component = gameObject.GetComponent<quickBuild>();
			if (componentInParent && componentInParent.Hp < componentInParent._maxHP && component)
			{
				component.newBuildingDamage = componentInParent._maxHP - componentInParent.Hp;
			}
			TreeStructure component2 = this.Ghost.GetComponent<TreeStructure>();
			if (component2 && component)
			{
				component.TreeId = component2.TreeId;
			}
			if (this.Ghost)
			{
				UnityEngine.Object.Destroy(this.Ghost);
			}
		}
	}

	
	public GameObject Built;

	
	public GameObject TrapTrigger;

	
	public GameObject SheenIcon;

	
	public GameObject PickupIcon;

	
	public GameObject ResetIcon;

	
	public ItemCost ItemCost;

	
	private GameObject Ghost;
}
