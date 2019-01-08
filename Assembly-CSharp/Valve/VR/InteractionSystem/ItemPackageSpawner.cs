using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(Interactable))]
	public class ItemPackageSpawner : MonoBehaviour
	{
		public ItemPackage itemPackage
		{
			get
			{
				return this._itemPackage;
			}
			set
			{
				this.CreatePreviewObject();
			}
		}

		private void CreatePreviewObject()
		{
			if (!this.useItemPackagePreview)
			{
				return;
			}
			this.ClearPreview();
			if (this.useItemPackagePreview)
			{
				if (this.itemPackage == null)
				{
					return;
				}
				if (!this.useFadedPreview)
				{
					if (this.itemPackage.previewPrefab != null)
					{
						this.previewObject = UnityEngine.Object.Instantiate<GameObject>(this.itemPackage.previewPrefab, base.transform.position, Quaternion.identity);
						this.previewObject.transform.parent = base.transform;
						this.previewObject.transform.localRotation = Quaternion.identity;
					}
				}
				else if (this.itemPackage.fadedPreviewPrefab != null)
				{
					this.previewObject = UnityEngine.Object.Instantiate<GameObject>(this.itemPackage.fadedPreviewPrefab, base.transform.position, Quaternion.identity);
					this.previewObject.transform.parent = base.transform;
					this.previewObject.transform.localRotation = Quaternion.identity;
				}
			}
		}

		private void Start()
		{
			this.VerifyItemPackage();
		}

		private void VerifyItemPackage()
		{
			if (this.itemPackage == null)
			{
				this.ItemPackageNotValid();
			}
			if (this.itemPackage.itemPrefab == null)
			{
				this.ItemPackageNotValid();
			}
		}

		private void ItemPackageNotValid()
		{
			Debug.LogError("ItemPackage assigned to " + base.gameObject.name + " is not valid. Destroying this game object.");
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void ClearPreview()
		{
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (Time.time > 0f)
					{
						UnityEngine.Object.Destroy(transform.gameObject);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(transform.gameObject);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private void Update()
		{
			if (this.itemIsSpawned && this.spawnedItem == null)
			{
				this.itemIsSpawned = false;
				this.useFadedPreview = false;
				this.dropEvent.Invoke();
				this.CreatePreviewObject();
			}
		}

		private void OnHandHoverBegin(Hand hand)
		{
			ItemPackage attachedItemPackage = this.GetAttachedItemPackage(hand);
			if (attachedItemPackage == this.itemPackage && this.takeBackItem && !this.requireTriggerPressToReturn)
			{
				this.TakeBackItem(hand);
			}
			if (!this.requireTriggerPressToTake)
			{
				this.SpawnAndAttachObject(hand);
			}
			if (this.requireTriggerPressToTake && this.showTriggerHint)
			{
				ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_Axis1, "PickUp", true);
			}
		}

		private void TakeBackItem(Hand hand)
		{
			this.RemoveMatchingItemsFromHandStack(this.itemPackage, hand);
			if (this.itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded)
			{
				this.RemoveMatchingItemsFromHandStack(this.itemPackage, hand.otherHand);
			}
		}

		private ItemPackage GetAttachedItemPackage(Hand hand)
		{
			GameObject currentAttachedObject = hand.currentAttachedObject;
			if (currentAttachedObject == null)
			{
				return null;
			}
			ItemPackageReference component = hand.currentAttachedObject.GetComponent<ItemPackageReference>();
			if (component == null)
			{
				return null;
			}
			return component.itemPackage;
		}

		private void HandHoverUpdate(Hand hand)
		{
			if (this.takeBackItem && this.requireTriggerPressToReturn && hand.controller != null && hand.controller.GetHairTriggerDown())
			{
				ItemPackage attachedItemPackage = this.GetAttachedItemPackage(hand);
				if (attachedItemPackage == this.itemPackage)
				{
					this.TakeBackItem(hand);
					return;
				}
			}
			if (this.requireTriggerPressToTake && hand.controller != null && hand.controller.GetHairTriggerDown())
			{
				this.SpawnAndAttachObject(hand);
			}
		}

		private void OnHandHoverEnd(Hand hand)
		{
			if (!this.justPickedUpItem && this.requireTriggerPressToTake && this.showTriggerHint)
			{
				ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_Axis1);
			}
			this.justPickedUpItem = false;
		}

		private void RemoveMatchingItemsFromHandStack(ItemPackage package, Hand hand)
		{
			for (int i = 0; i < hand.AttachedObjects.Count; i++)
			{
				ItemPackageReference component = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
				if (component != null)
				{
					ItemPackage itemPackage = component.itemPackage;
					if (itemPackage != null && itemPackage == package)
					{
						GameObject attachedObject = hand.AttachedObjects[i].attachedObject;
						hand.DetachObject(attachedObject, true);
					}
				}
			}
		}

		private void RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType packageType, Hand hand)
		{
			for (int i = 0; i < hand.AttachedObjects.Count; i++)
			{
				ItemPackageReference component = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
				if (component != null && component.itemPackage.packageType == packageType)
				{
					GameObject attachedObject = hand.AttachedObjects[i].attachedObject;
					hand.DetachObject(attachedObject, true);
				}
			}
		}

		private void SpawnAndAttachObject(Hand hand)
		{
			if (hand.otherHand != null)
			{
				ItemPackage attachedItemPackage = this.GetAttachedItemPackage(hand.otherHand);
				if (attachedItemPackage == this.itemPackage)
				{
					this.TakeBackItem(hand.otherHand);
				}
			}
			if (this.showTriggerHint)
			{
				ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_Axis1);
			}
			if (this.itemPackage.otherHandItemPrefab != null && hand.otherHand.hoverLocked)
			{
				return;
			}
			if (this.itemPackage.packageType == ItemPackage.ItemPackageType.OneHanded)
			{
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.OneHanded, hand);
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand);
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand.otherHand);
			}
			if (this.itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded)
			{
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.OneHanded, hand);
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.OneHanded, hand.otherHand);
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand);
				this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand.otherHand);
			}
			this.spawnedItem = UnityEngine.Object.Instantiate<GameObject>(this.itemPackage.itemPrefab);
			this.spawnedItem.SetActive(true);
			hand.AttachObject(this.spawnedItem, this.attachmentFlags, this.attachmentPoint);
			if (this.itemPackage.otherHandItemPrefab != null && hand.otherHand.controller != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.itemPackage.otherHandItemPrefab);
				gameObject.SetActive(true);
				hand.otherHand.AttachObject(gameObject, this.attachmentFlags, string.Empty);
			}
			this.itemIsSpawned = true;
			this.justPickedUpItem = true;
			if (this.takeBackItem)
			{
				this.useFadedPreview = true;
				this.pickupEvent.Invoke();
				this.CreatePreviewObject();
			}
		}

		public void removeAttachedObjects(Hand hand)
		{
			this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand);
			this.RemoveMatchingItemTypesFromHand(ItemPackage.ItemPackageType.TwoHanded, hand.otherHand);
		}

		public ItemPackage _itemPackage;

		public bool useItemPackagePreview = true;

		public bool useFadedPreview;

		private GameObject previewObject;

		public bool requireTriggerPressToTake;

		public bool requireTriggerPressToReturn;

		public bool showTriggerHint;

		[EnumFlags]
		public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand;

		public string attachmentPoint;

		public bool takeBackItem;

		public bool acceptDifferentItems;

		public GameObject spawnedItem;

		private bool itemIsSpawned;

		public UnityEvent pickupEvent;

		public UnityEvent dropEvent;

		public bool justPickedUpItem;
	}
}
