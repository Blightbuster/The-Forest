using System;
using System.Collections;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using UnityEngine;

public class inventoryItemSpreadTrigger : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.GetComponentInParent<CraftingCog>())
		{
			this.onCraftingMat = true;
			UnityEngine.Object.Destroy(this);
			return;
		}
		this.animator = base.transform.GetComponent<Animator>();
		if (!this.animator)
		{
			this.animator = base.transform.GetComponentInChildren<Animator>(true);
		}
		this.spreadSetup = base.transform.parent.GetComponent<inventoryItemSpreadSetup>();
		if (!this.spreadSetup)
		{
			this.spreadSetup = base.transform.GetComponent<inventoryItemSpreadSetup>();
		}
		this._itemView = base.transform.GetComponent<InventoryItemView>();
	}

	private void OnDisable()
	{
		this.isActive = false;
	}

	public void toggleActive(bool onoff)
	{
		if (!this.isActive && onoff)
		{
			this.OnMouseEnterCollider();
			this.isActive = true;
		}
		else if (this.isActive && !onoff)
		{
			this.isActive = false;
			this.OnMouseExitCollider();
		}
	}

	public void OnMouseExitCollider()
	{
		if (this.isActive)
		{
			return;
		}
		if (this.onCraftingMat)
		{
			return;
		}
		if (this.animator)
		{
			this.animator.SetBool("open", false);
		}
		if (this.spreadSetup.spreadHoveredItemOnlyMode)
		{
			base.StartCoroutine("disableSingleItemSpreadRoutine");
			return;
		}
		this.spreadSetup.spreadActive = false;
		if (this.spreadSetup.singleItemMode)
		{
			base.StartCoroutine(this.resetSpread(0.1f));
		}
		else
		{
			base.StartCoroutine(this.resetSpread(0.25f));
		}
	}

	public void OnMouseEnterCollider()
	{
		if (this.onCraftingMat)
		{
			return;
		}
		int num = 0;
		if (!this.spreadSetup)
		{
			return;
		}
		if (this.animator)
		{
			this.animator.SetBool("open", true);
		}
		if (this.spreadSetup.minSpreadTargetAmount > 0)
		{
			for (int i = 0; i < this.spreadSetup.sourceObjects.Count; i++)
			{
				if (this.spreadSetup.sourceObjects[i].activeSelf)
				{
					num++;
				}
			}
			if (num < this.spreadSetup.minSpreadTargetAmount)
			{
				return;
			}
		}
		if (this.spreadSetup.spreadHoveredItemOnlyMode)
		{
			if (this._itemView)
			{
				this._itemView.PlayCustomSFX();
			}
			base.StartCoroutine("enableSingleItemSpreadRoutine");
			return;
		}
		this.spreadSetup.spreadActive = true;
		if (this._itemView)
		{
			this._itemView.PlayCustomSFX();
		}
		if (!this.spreadSetup.doingItemSpread && this.isNeighbourActive())
		{
			this.spreadSetup.StartCoroutine("enableItemSpreadRoutine");
		}
	}

	private IEnumerator resetSpread(float delay)
	{
		float t = 0f;
		while (t < delay)
		{
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		if (this.spreadSetup.doingItemSpread && !this.spreadSetup.spreadActive && base.gameObject.activeInHierarchy)
		{
			this.spreadSetup.StartCoroutine("disableItemSpreadRoutine");
		}
		yield break;
	}

	private bool isNeighbourActive()
	{
		return this.spreadSetup.singleItemMode || this.spreadSetup.spreadHoveredItemOnlyMode || this.spreadSetup.minSpreadTargetAmount > 0 || (this.neighbour1 && this.neighbour1.activeSelf) || (this.neighbour2 && this.neighbour2.activeSelf);
	}

	private IEnumerator enableSingleItemSpreadRoutine()
	{
		float t = 0f;
		base.StopCoroutine("disableSingleItemSpreadRoutine");
		while (t < 1f)
		{
			if (this.spreadSetup.offsetRenderersMode)
			{
				this.spreadSetup.offsetSourceObjects[this.index].transform.localPosition = Vector3.Slerp(this.spreadSetup.offsetSourceObjects[this.index].transform.localPosition, this.spreadSetup.targetPositions[this.index], t);
				this.spreadSetup.offsetSourceObjects[this.index].transform.localRotation = Quaternion.Slerp(this.spreadSetup.offsetSourceObjects[this.index].transform.localRotation, this.spreadSetup.targetRotations[this.index], t);
			}
			else
			{
				base.transform.localPosition = Vector3.Slerp(base.transform.localPosition, this.spreadSetup.targetPositions[this.index], t);
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, this.spreadSetup.targetRotations[this.index], t);
			}
			t += Time.unscaledDeltaTime * 2f;
			yield return null;
		}
		yield break;
	}

	private IEnumerator disableSingleItemSpreadRoutine()
	{
		float t = 0f;
		while (t < 0.15f)
		{
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		base.StopCoroutine("enableSingleItemSpreadRoutine");
		t = 0f;
		while (t < 1f)
		{
			if (this.spreadSetup.offsetRenderersMode)
			{
				this.spreadSetup.offsetSourceObjects[this.index].transform.localPosition = Vector3.Slerp(this.spreadSetup.offsetSourceObjects[this.index].transform.localPosition, this.spreadSetup.startPositions[this.index], t);
				this.spreadSetup.offsetSourceObjects[this.index].transform.localRotation = Quaternion.Slerp(this.spreadSetup.offsetSourceObjects[this.index].transform.localRotation, this.spreadSetup.startRotations[this.index], t);
			}
			else
			{
				base.transform.localPosition = Vector3.Slerp(base.transform.localPosition, this.spreadSetup.startPositions[this.index], t);
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, this.spreadSetup.startRotations[this.index], t);
			}
			t += Time.unscaledDeltaTime * 2f;
			yield return null;
		}
		yield break;
	}

	private InventoryItemView _itemView;

	public GameObject neighbour1;

	public GameObject neighbour2;

	public int index;

	private bool onCraftingMat;

	private bool animatedItem;

	public bool isActive;

	public Animator animator;

	private inventoryItemSpreadSetup spreadSetup;
}
