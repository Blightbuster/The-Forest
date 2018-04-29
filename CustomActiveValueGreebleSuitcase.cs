using System;
using System.Collections;
using UnityEngine;


public class CustomActiveValueGreebleSuitcase : CustomActiveValueGreeble
{
	
	private void Awake()
	{
		this.LidDefaultRotation = this._lid.transform.localRotation;
	}

	
	private void OnDisable()
	{
		this.UpdateGreebleData();
		this._lid.transform.localRotation = this.LidDefaultRotation;
		this._interior.SetActive(false);
		this._trigger.SetActive(true);
		this._pickup.SetActive(true);
	}

	
	private void OnEnable()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this._initializing = true;
		base.StartCoroutine(this.OnEnableRoutine());
	}

	
	private IEnumerator OnEnableRoutine()
	{
		yield return null;
		if (base.Data != null)
		{
			ISuitcaseState iss = null;
			if (BoltNetwork.isRunning)
			{
				BoltEntity be = base.GetComponent<BoltEntity>();
				if (be.isAttached)
				{
					iss = be.GetState<ISuitcaseState>();
				}
			}
			byte b = base.Data._instancesState[base.Index];
			switch (b)
			{
			case 0:
				break;
			case 1:
				this._lid.Play();
				if (!this._interior.activeSelf)
				{
					this._interior.SetActive(true);
				}
				if (!this._pickup.activeSelf)
				{
					this._pickup.SetActive(true);
				}
				if (this._trigger.activeSelf)
				{
					this._trigger.SetActive(false);
				}
				if (iss != null)
				{
					iss.Open = true;
					iss.ClothPickedUp = false;
				}
				goto IL_2D1;
			case 2:
				this._lid.Play();
				if (this._interior.activeSelf)
				{
					this._interior.SetActive(false);
				}
				if (this._trigger.activeSelf)
				{
					this._trigger.SetActive(false);
				}
				if (iss != null)
				{
					iss.Open = true;
					iss.ClothPickedUp = true;
				}
				goto IL_2D1;
			default:
				if (b != 252)
				{
					goto IL_2D1;
				}
				break;
			}
			base.Data._instancesState[base.Index] = 252;
			this._lid.transform.localRotation = this.LidDefaultRotation;
			if (this._interior.activeSelf)
			{
				this._interior.SetActive(false);
			}
			if (!this._pickup.activeSelf)
			{
				this._pickup.SetActive(true);
			}
			if (!this._trigger.activeSelf)
			{
				this._trigger.SetActive(true);
			}
			if (iss != null)
			{
				iss.Open = false;
				iss.ClothPickedUp = false;
			}
			IL_2D1:;
		}
		else
		{
			this._lid.transform.localRotation = this.LidDefaultRotation;
			if (this._interior.activeSelf)
			{
				this._interior.SetActive(false);
			}
			if (!this._pickup.activeSelf)
			{
				this._pickup.SetActive(true);
			}
			if (!this._trigger.activeSelf)
			{
				this._trigger.SetActive(true);
			}
		}
		this._initializing = false;
		yield break;
	}

	
	public override void UpdateGreebleData()
	{
		if (base.Data != null && !this._initializing)
		{
			if (!this._interior.activeSelf)
			{
				if (this._trigger.activeSelf)
				{
					base.Data._instancesState[base.Index] = 252;
				}
				else
				{
					base.Data._instancesState[base.Index] = 2;
				}
			}
			else if (!this._pickup.activeSelf)
			{
				base.Data._instancesState[base.Index] = 2;
			}
			else
			{
				base.Data._instancesState[base.Index] = 1;
			}
		}
	}

	
	public GameObject _interior;

	
	public GameObject _trigger;

	
	public GameObject _pickup;

	
	public Animation _lid;

	
	private bool _initializing;

	
	private Quaternion LidDefaultRotation;
}
