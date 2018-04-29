﻿using System;
using TheForest.UI;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SheenBillboard : MonoBehaviour, IThreadSafeTask
{
	
	private static Mesh GetSharedQuad()
	{
		if (SheenBillboard.QuadMesh == null)
		{
			SheenBillboard.QuadMesh = new Mesh();
			SheenBillboard.GenerateMesh(SheenBillboard.QuadMesh);
		}
		return SheenBillboard.QuadMesh;
	}

	
	
	
	public bool UseFillSprite { get; set; }

	
	
	
	public DelayedAction FillSpriteAction { get; set; }

	
	public void Awake()
	{
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component == null)
		{
			Debug.LogError("MeshFilter not found!");
			return;
		}
		this.render = base.GetComponent<Renderer>();
		component.sharedMesh = SheenBillboard.GetSharedQuad();
		SheenBillboardManager.Register(this);
	}

	
	private void Start()
	{
		this.position = base.transform.position;
		this.WSRegistration();
		this.MainThreadRefresh();
	}

	
	private void OnDestroy()
	{
		try
		{
			if (this.wsToken >= 0)
			{
				WorkScheduler.Unregister(this, this.wsToken);
				this.wsToken = -1;
			}
			SheenBillboardManager.Unregister(this);
		}
		catch
		{
		}
	}

	
	public virtual void OnEnable()
	{
		if (this._action != InputMappingIcons.Actions.None)
		{
			ActionIcon actionIcon = ActionIconSystem.RegisterIcon(base.transform, this._action, this._sideIcon, ActionIconSystem.CurrentViewOptions.AllowInWorld, false, true);
			if (actionIcon)
			{
				this.FillSpriteAction = actionIcon._fillSpriteAction;
				if (this.FillSpriteAction && this.FillSpriteAction.gameObject.activeSelf != this.UseFillSprite)
				{
					this.FillSpriteAction.gameObject.SetActive(this.UseFillSprite);
				}
				if (this.UseFillSprite)
				{
					this.FillSpriteAction.SetAction(this._action);
				}
			}
		}
	}

	
	public virtual void OnDisable()
	{
		if (this._action != InputMappingIcons.Actions.None)
		{
			if (this.FillSpriteAction && this.FillSpriteAction.gameObject.activeSelf)
			{
				this.FillSpriteAction.gameObject.SetActive(false);
			}
			ActionIconSystem.UnregisterIcon(base.transform, false, true);
		}
	}

	
	public void LateUpdate()
	{
		this.position = base.transform.position;
		float sqrMagnitude = (base.transform.position - PlayerCamLocation.PlayerLoc).sqrMagnitude;
		bool flag = PlayerPreferences.ShowHud && sqrMagnitude < this._Distance * this._Distance;
		if (flag != this.render.enabled)
		{
			this.render.enabled = flag;
		}
	}

	
	private static void GenerateMesh(Mesh mesh)
	{
		int num = 1;
		Vector3[] array = new Vector3[4 * num];
		Vector2[] array2 = new Vector2[4 * num];
		int[] array3 = new int[6 * num];
		for (int i = 0; i < num; i++)
		{
			Vector3 zero = Vector3.zero;
			int num2 = 4 * i;
			array[num2] = zero;
			array[num2 + 1] = zero;
			array[num2 + 2] = zero;
			array[num2 + 3] = zero;
			array2[num2] = new Vector2(0f, 0f);
			array2[num2 + 1] = new Vector2(0f, 1f);
			array2[num2 + 2] = new Vector2(1f, 0f);
			array2[num2 + 3] = new Vector2(1f, 1f);
			int num3 = 6 * i;
			array3[num3] = num2;
			array3[num3 + 1] = 1 + num2;
			array3[num3 + 2] = 2 + num2;
			array3[num3 + 3] = 2 + num2;
			array3[num3 + 4] = 1 + num2;
			array3[num3 + 5] = 3 + num2;
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
	}

	
	public void CheckWsToken2()
	{
		this.position = base.transform.position;
		if (this.wsToken > -1 && !WorkScheduler.CheckTokenForPosition(this.position, this.wsToken))
		{
			this.WSRegistration();
			this.ShouldDoMainThreadRefresh = false;
		}
	}

	
	public void CheckWsTokenSafe()
	{
		if (this.wsToken > -1 && !WorkScheduler.CheckTokenForPosition(this.position, this.wsToken))
		{
			this.WSRegistration();
			this.ShouldDoMainThreadRefresh = false;
		}
	}

	
	private void WSRegistration()
	{
		if (this.wsToken != -1)
		{
			WorkScheduler.Unregister(this, this.wsToken);
		}
		this.wsToken = WorkScheduler.Register(this, this.position, true);
	}

	
	
	
	public bool ShouldDoMainThreadRefresh { get; set; }

	
	public void ThreadedRefresh()
	{
		this.position.y = PlayerCamLocation.PlayerLoc.y;
		float num = Vector3.Distance(this.position, PlayerCamLocation.PlayerLoc);
		if (this.shouldBeEnabled != num < this._Distance * 3f)
		{
			this.ShouldDoMainThreadRefresh = true;
			this.shouldBeEnabled = !this.shouldBeEnabled;
		}
		this.CheckWsTokenSafe();
	}

	
	public void MainThreadRefresh()
	{
		base.enabled = this.shouldBeEnabled;
	}

	
	private static Mesh QuadMesh;

	
	public InputMappingIcons.Actions _action;

	
	public ActionIcon.SideIconTypes _sideIcon;

	
	public bool PickUp;

	
	private bool shouldBeEnabled;

	
	private int wsToken = -1;

	
	private Vector3 position;

	
	private Renderer render;

	
	private float _Distance = 10f;
}
