using System;
using UnityEngine;

public class hostEpilogueEvents : MonoBehaviour
{
	private void Awake()
	{
		this.storeBookPos = this.book.transform.localPosition;
	}

	private void Start()
	{
	}

	private void parentBookToHost(bool t)
	{
		if (t)
		{
			this.book.transform.parent = this.leftHand_book;
			this.book.transform.localPosition = Vector3.zero;
			this.book.transform.localRotation = Quaternion.identity;
		}
		else
		{
			this.book.transform.parent = null;
		}
	}

	private void parentAxeToHost(bool t)
	{
		if (t)
		{
			this.axe.transform.parent = this.leftHand_axe;
			this.axe.transform.localPosition = Vector3.zero;
			this.axe.transform.localRotation = Quaternion.identity;
		}
		else
		{
			this.axe.transform.parent = null;
		}
	}

	public GameObject book;

	public GameObject axe;

	public Transform leftHand_book;

	public Transform leftHand_axe;

	private Vector3 storeBookPos;
}
