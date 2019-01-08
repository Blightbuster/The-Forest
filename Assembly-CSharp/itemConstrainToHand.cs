using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Networking;
using UniLinq;
using UnityEngine;

public class itemConstrainToHand : EntityBehaviour
{
	private void Start()
	{
		base.Invoke("SetupWeapons", 0.3f);
	}

	public void SetupWeaponsIdentifier()
	{
		if (!this.fixedItems)
		{
			if (!this.isPlayerNet || !Application.isPlaying)
			{
				List<GameObject> list = new List<GameObject>();
				IEnumerator enumerator = base.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						if (transform.GetComponent<HeldItemIdentifier>())
						{
							list.Add(transform.gameObject);
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
				this.Available = new GameObject[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					this.Available[i] = list[i];
					if (Application.isPlaying)
					{
						this.Available[i].SetActive(this.isPlayerNet);
					}
				}
			}
			this.Available = (from x in this.Available
			orderby x.name
			select x).ToArray<GameObject>();
		}
	}

	public void SetupWeapons()
	{
		if (!this.fixedItems)
		{
			if (!this.isPlayerNet || !Application.isPlaying)
			{
				this.Available = new GameObject[base.transform.childCount];
				for (int i = 0; i < this.Available.Length; i++)
				{
					this.Available[i] = base.transform.GetChild(i).gameObject;
					if (Application.isPlaying)
					{
						this.Available[i].SetActive(this.isPlayerNet);
					}
				}
			}
			this.Available = (from x in this.Available
			orderby x.name
			select x).ToArray<GameObject>();
		}
	}

	public void Enable(int index, StealItemTrigger RightHandStealTrigger = null)
	{
		for (int i = 0; i < this.Available.Length; i++)
		{
			if (index != i && this.Available[i].activeSelf)
			{
				this.Available[i].SetActive(false);
			}
		}
		if (index != -1 && index < this.Available.Length)
		{
			if (!this.Available[index].activeSelf)
			{
				this.Available[index].SetActive(true);
			}
			this.ActiveItem = this.Available[index].transform;
			if (RightHandStealTrigger)
			{
				RightHandStealTrigger.ActivateIfIsStealableItem(this.Available[index]);
			}
		}
		else if (RightHandStealTrigger)
		{
			RightHandStealTrigger.gameObject.SetActive(false);
		}
	}

	public void Parent()
	{
		this.tr = base.transform;
		Transform[] componentsInChildren = base.transform.root.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "char_RightHandWeapon")
			{
				this.rightHand = transform;
			}
			if (transform.name == "char_LeftHandWeapon")
			{
				this.leftHandWeapon = transform;
			}
		}
	}

	private void parentToRightHand()
	{
		base.transform.parent = this.rightHand;
		this.tr.position = this.rightHand.position;
		this.tr.rotation = this.rightHand.rotation;
	}

	private void parentToLeftHand()
	{
		base.transform.parent = this.leftHandWeapon;
		this.tr.position = this.leftHandWeapon.position;
		this.tr.rotation = this.leftHandWeapon.rotation;
	}

	public Transform ActiveItem { get; set; }

	public float xOffset;

	public float yOffset;

	public float zOffset;

	public bool fixedItems;

	public bool toLeftHand;

	public bool isPlayerNet;

	public bool useIdentifierForSetup;

	private Transform tr;

	private Transform leftHandWeapon;

	private Transform rightHand;

	private Transform feet;

	public GameObject[] Available;
}
