using System;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class survivalBookController : MonoBehaviour
{
	
	private void Awake()
	{
		this.bookPosition = this.survivalBookReal.transform.localPosition;
		this.bookRotation = this.survivalBookReal.transform.localRotation;
		this.survivalBookReal.transform.parent = null;
		this.animator = base.transform.GetComponent<Animator>();
		base.Invoke("initMe", 1f);
	}

	
	private void initMe()
	{
		base.gameObject.SetActive(false);
		this.initBool = true;
	}

	
	private void Update()
	{
		if (!this.initBool)
		{
			return;
		}
		this.nextState = LocalPlayer.Animator.GetNextAnimatorStateInfo(1);
		this.thisState = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
		if (LocalPlayer.Animator.GetBool("bookHeld"))
		{
			LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
			LocalPlayer.Animator.SetBoolReflected("clampSpine", true);
		}
		if (this.thisState.IsName("upperBody.bookIdle") && !this.realBookOpen)
		{
			LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			LocalPlayer.FpCharacter.LockView(false);
			this.survivalBookReal.SetActive(true);
			this.survivalBookReal.transform.localPosition = this.bookPosition;
			this.survivalBookReal.transform.localRotation = this.bookRotation;
			this.survivalBookAnimated.SetActive(false);
			this.survivalBookReal.SendMessage("CheckPage");
			this.realBookOpen = true;
			this.bookIsOpen = true;
			this.isOpeningBook = false;
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.BlockTogglingInventory = false;
			}
		}
		if (this.thisState.IsName("upperBody.bookIdle") && this.bookIsOpen)
		{
			this.survivalBookAnimated.SetActive(false);
			this.survivalBookReal.transform.position = this.localBookPosTr.position;
			this.survivalBookReal.transform.rotation = this.localBookPosTr.transform.rotation;
			this.survivalBookReal.SetActive(true);
		}
		if (LocalPlayer.AnimControl.swimming && this.bookIsOpen)
		{
			LocalPlayer.Create.CloseTheBook(false);
		}
		if (!LocalPlayer.Animator.GetBool("bookHeld") && this.bookIsOpen)
		{
			this.FinalCloseBook();
		}
		if (!LocalPlayer.Animator.GetBool("bookHeld") && !this.getAnimatorBookState() && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._bookId))
		{
			this.ForceAnimatedBookReset();
		}
	}

	
	private void FinalCloseBook()
	{
		LocalPlayer.CamRotator.dampingOverride = 0f;
		this.animator.SetBoolReflected("bookHeld", false);
		this.animator.Play(this.toIdleHash, 0, 0f);
		LocalPlayer.Animator.SetBoolReflected("clampSpine", false);
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			LocalPlayer.FpCharacter.UnLockView();
		}
		this.bookIsOpen = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		SimpleMouseRotator camRotator = LocalPlayer.CamRotator;
		camRotator.targetAngles.x = camRotator.targetAngles.x + -20f;
		SimpleMouseRotator camRotator2 = LocalPlayer.CamRotator;
		camRotator2.followAngles.x = camRotator2.followAngles.x + -20f;
		LocalPlayer.CamRotator.xOffset = 0f;
		LocalPlayer.CamRotator.fixCameraRotation = true;
		if (!base.IsInvoking("setCloseBook"))
		{
			base.Invoke("setCloseBook", 1f);
		}
		this.survivalBookReal.SendMessage("CloseBook", SendMessageOptions.DontRequireReceiver);
		this.survivalBookAnimated.SetActive(true);
		this.survivalBookReal.SetActive(false);
	}

	
	private void ForceAnimatedBookReset()
	{
		LocalPlayer.CamRotator.dampingOverride = 0f;
		this.animator.SetBoolReflected("bookHeld", false);
		this.animator.Play(this.toIdleHash, 0, 1f);
		LocalPlayer.Animator.SetBoolReflected("clampSpine", false);
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			LocalPlayer.FpCharacter.UnLockView();
		}
		this.bookIsOpen = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.CamRotator.xOffset = 0f;
		LocalPlayer.CamRotator.fixCameraRotation = true;
		this.survivalBookReal.SetActive(false);
		base.gameObject.SetActive(false);
	}

	
	private void LateUpdate()
	{
		if (this.thisState.IsName("upperBody.bookIdle") && this.bookIsOpen)
		{
			this.survivalBookReal.transform.position = this.localBookPosTr.position;
			this.survivalBookReal.transform.rotation = this.localBookPosTr.transform.rotation;
		}
	}

	
	private void fastCloseBook()
	{
		LocalPlayer.Animator.SetBool("resetBook", true);
		base.Invoke("resetFastClose", 0.2f);
	}

	
	public void setCloseBook()
	{
		if (LocalPlayer.Inventory)
		{
			LocalPlayer.Inventory.BlockTogglingInventory = false;
		}
		this.bookIsOpen = false;
		this.realBookOpen = false;
		base.gameObject.SetActive(false);
		LocalPlayer.CamRotator.dampingOverride = 0f;
	}

	
	public void setOpenBook()
	{
		if (!this.isOpeningBook && !this.realBookOpen && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book && LocalPlayer.Inventory.IsSlotAndNextSlotEmpty(Item.EquipmentSlot.RightHand))
		{
			this.isOpeningBook = true;
			if (!this.animator)
			{
				this.animator = base.transform.GetComponent<Animator>();
			}
			this.animator.SetBool("bookHeld", true);
			LocalPlayer.Animator.SetBoolReflected("clampSpine", true);
			LocalPlayer.CamRotator.dampingOverride = 1f;
			LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
			LocalPlayer.CamRotator.xOffset = -20f;
			float normalizedTime = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
			this.animator.CrossFade("Base Layer.toBookIdle", 0f, 0, normalizedTime);
		}
	}

	
	private void cancelBookClose()
	{
		base.CancelInvoke("setCloseBook");
	}

	
	private void resetFastClose()
	{
		LocalPlayer.Animator.SetBool("resetBook", false);
		base.gameObject.SetActive(false);
	}

	
	private void OnDisable()
	{
		base.CancelInvoke("setCloseBook");
		LocalPlayer.Animator.SetBoolReflected("clampSpine", false);
		LocalPlayer.Animator.SetBool("resetBook", false);
		this.bookIsOpen = false;
		this.realBookOpen = false;
		this.isOpeningBook = false;
		if (LocalPlayer.Inventory)
		{
			LocalPlayer.Inventory.BlockTogglingInventory = false;
		}
	}

	
	private bool getAnimatorBookState()
	{
		return LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._idleToBookHash || LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._bookIdleHash || LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._bookToIdleHash;
	}

	
	public GameObject survivalBookReal;

	
	public GameObject survivalBookAnimated;

	
	public Transform localBookPosTr;

	
	public Animator animator;

	
	public bool bookIsOpen;

	
	public bool realBookOpen;

	
	private Vector3 bookPosition;

	
	private Quaternion bookRotation;

	
	private bool initBool;

	
	private bool isOpeningBook;

	
	[ItemIdPicker]
	public int _bookId;

	
	private AnimatorStateInfo nextState;

	
	private AnimatorStateInfo thisState;

	
	private int _idleToBookHash = Animator.StringToHash("idleToBookIdle");

	
	private int _bookIdleHash = Animator.StringToHash("bookIdle");

	
	private int _bookToIdleHash = Animator.StringToHash("bookIdleToIdle");

	
	private int toIdleHash = Animator.StringToHash("bookIdleToIdle");
}
