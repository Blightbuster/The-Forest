using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Player.Clothing;
using TheForest.Utils;
using UnityEngine;


public class SkinAnimal : EntityEventListener
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	private void GrabEnter()
	{
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
		base.enabled = true;
	}

	
	private void GrabExit()
	{
		this.Sheen.SetActive(true);
		this.MyPickUp.SetActive(false);
		base.enabled = false;
	}

	
	public override void OnEvent(SkinnedAnimal evnt)
	{
		this.SetSkinnedMP();
	}

	
	private void Update()
	{
		if ((LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).tagHash != this.idleHash || (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).tagHash != this.idleHash && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).tagHash != this.heldHash)) && !this.startedSkinning && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.onRaft)
		{
			this.MyPickUp.SetActive(false);
			return;
		}
		if (!this.startedSkinning)
		{
			this.MyPickUp.SetActive(true);
		}
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false) && !this.startedSkinning)
		{
			LocalPlayer.Sfx.PlayWhoosh();
			base.StartCoroutine(this.setSkinnedRoutine());
			this.startedSkinning = true;
		}
	}

	
	private void SinglePlayer_Skinning()
	{
		this.SetSkinned();
	}

	
	private void tryAddSkin()
	{
		if (this.Croc)
		{
			this.TryAddItem();
			this.TryAddItem();
			this.TryAddItem();
			this.TryAddItem();
		}
		if (this.Lizard || this.Rabbit || this.Deer || this.raccoon || this.boar || this.creepy || this.stewardess)
		{
			this.TryAddItem();
		}
	}

	
	private IEnumerator setSkinnedRoutine()
	{
		int animalType = 0;
		if (this.Lizard || this.Croc)
		{
			animalType = 1;
		}
		else if (this.Rabbit)
		{
			animalType = 2;
		}
		else if (this.Deer)
		{
			animalType = 3;
		}
		else if (this.turtle)
		{
			animalType = 4;
		}
		else if (this.creepy)
		{
			animalType = 5;
		}
		else if (this.boar)
		{
			animalType = 6;
		}
		else if (this.raccoon)
		{
			animalType = 7;
		}
		else if (this.stewardess)
		{
			animalType = 8;
		}
		LocalPlayer.SpecialActions.SendMessage("setAnimalTransform", base.transform.root, SendMessageOptions.DontRequireReceiver);
		LocalPlayer.SpecialActions.SendMessage("setAnimalType", animalType, SendMessageOptions.DontRequireReceiver);
		LocalPlayer.SpecialActions.SendMessage("skinAnimalRoutine", base.transform.position, SendMessageOptions.DontRequireReceiver);
		yield return YieldPresets.WaitOneSecond;
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		if (BoltNetwork.isRunning)
		{
			if (base.entity)
			{
				base.entity.Freeze(false);
			}
			this.SetSkinned();
			yield return YieldPresets.WaitOneSecond;
			yield return YieldPresets.WaitPointTwoFiveSeconds;
			this.tryAddSkin();
			if (base.entity)
			{
				SkinnedAnimal skinnedAnimal = SkinnedAnimal.Create(GlobalTargets.Everyone);
				skinnedAnimal.Target = base.transform.GetComponentInParent<BoltEntity>();
				skinnedAnimal.Send();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			this.SinglePlayer_Skinning();
			yield return YieldPresets.WaitOneSecond;
			yield return YieldPresets.WaitPointFiveSeconds;
			yield return YieldPresets.WaitPointTwoFiveSeconds;
			this.tryAddSkin();
			UnityEngine.Object.Destroy(base.gameObject);
		}
		yield break;
	}

	
	public void SetSkinned()
	{
		if (this.MyBody)
		{
			this.MyBody.material = this.SkinnedMat;
		}
		if (this.FleshTrigger)
		{
			this.FleshTrigger.SetActive(true);
		}
	}

	
	public void SetSkinnedMP()
	{
		if (this.MyBody)
		{
			this.MyBody.material = this.SkinnedMat;
		}
		if (this.FleshTrigger)
		{
			this.FleshTrigger.SetActive(true);
		}
		if (base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	private void TryAddItem()
	{
		if (this.disableAfterPickupRenderer)
		{
			this.disableAfterPickupRenderer.enabled = false;
		}
		if (this.enableAfterPickupRenderer)
		{
			this.enableAfterPickupRenderer.enabled = true;
		}
		if (this.stewardess)
		{
			if (LocalPlayer.Clothing.AddClothingOutfit(new List<int>
			{
				this._clothingItemId
			}, true))
			{
				Scene.HudGui.ToggleGotClothingOutfitHud();
				LocalPlayer.Clothing.RefreshVisibleClothing();
				LocalPlayer.Stats.CheckArmsStart();
			}
			return;
		}
		if (!LocalPlayer.Inventory.HasOwned(this._itemId))
		{
			LocalPlayer.Inventory.SheenItem(this._itemId, ItemProperties.Any, true);
		}
		if (!LocalPlayer.Inventory.AddItem(this._itemId, 1, false, false, null))
		{
			LocalPlayer.Inventory.FakeDrop(this._itemId, null);
		}
	}

	
	public Material SkinnedMat;

	
	public Renderer MyBody;

	
	public GameObject FleshTrigger;

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public GameObject ragdollExplode;

	
	public Renderer disableAfterPickupRenderer;

	
	public Renderer enableAfterPickupRenderer;

	
	public bool Croc;

	
	public bool Lizard;

	
	public bool Rabbit;

	
	public bool Deer;

	
	public bool turtle;

	
	public bool boar;

	
	public bool raccoon;

	
	public bool creepy;

	
	public bool stewardess;

	
	private bool startedSkinning;

	
	private int idleHash = Animator.StringToHash("idling");

	
	private int heldHash = Animator.StringToHash("held");

	
	private AnimatorStateInfo pState;

	
	[ItemIdPicker]
	public int _itemId;

	
	[ClothingItemIdPicker]
	public int _clothingItemId;
}
