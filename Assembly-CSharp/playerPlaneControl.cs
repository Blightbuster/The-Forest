using System;
using TheForest.Utils;
using UnityEngine;

public class playerPlaneControl : MonoBehaviour
{
	private void Awake()
	{
		if (LevelSerializer.IsDeserializing || CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(this);
		}
		this.localHeadConstrainPos = this._headFollow.localPosition;
	}

	private void Start()
	{
		this.playerCam = LocalPlayer.MainCamTr;
		this.planeAnimator = base.GetComponent<Animator>();
		this.tr = base.transform;
		this.smoothCamX = 0f;
		if (ForestVR.Enabled)
		{
			SkinnedMeshRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				if (!skinnedMeshRenderer.gameObject.name.Contains("book"))
				{
					skinnedMeshRenderer.enabled = false;
				}
			}
			if (this.pillowVr != null)
			{
				this.pillowVr.SetActive(true);
			}
		}
	}

	private void Update()
	{
		if (!this.playerCam)
		{
			this.playerCam = LocalPlayer.MainCamTr;
		}
		else
		{
			float x = this.playerCam.eulerAngles.x;
			Vector3 forward = this.tr.forward;
			forward.y = 0f;
			Vector3 forward2 = this.playerCam.forward;
			forward2.y = 0f;
			float num = Vector3.Angle(forward, forward2);
			Vector3 lhs = Vector3.Cross(forward, forward2);
			float num2 = Vector3.Dot(lhs, this.tr.up);
			if (num2 < 0f)
			{
				num *= -1f;
			}
			if (x > 180f)
			{
				this.normCamX = x - 360f;
			}
			else
			{
				this.normCamX = x;
			}
			this.normCamX /= 90f;
			this.normCamX = (Mathf.Clamp(this.normCamX, -1f, 1f) - 0.1f) * 10f;
			Vector3 localPosition = this.localHeadConstrainPos;
			float num3 = this.normCamY;
			if (num3 > 0f && !this._client)
			{
				num3 = 0f;
			}
			localPosition.x = this.localHeadConstrainPos.x + num3 * this.scaleXFactor;
			this._headFollow.localPosition = localPosition;
			this.normCamY = num / 90f;
			this.normCamY = Mathf.Clamp(this.normCamY, -1f, 1f) * 10f;
			this.smoothCamY = this.normCamY;
			if (this.planeAnimator)
			{
				this.planeAnimator.SetFloatReflected("normCamX", this.smoothCamX);
				this.planeAnimator.SetFloatReflected("normCamY", this.smoothCamY);
			}
			if (this.timmyAnimator)
			{
				this.timmyAnimator.SetFloatReflected("normCamX", this.smoothCamX);
				this.timmyAnimator.SetFloatReflected("normCamY", this.smoothCamY);
			}
		}
	}

	private void LateUpdate()
	{
	}

	private Animator planeAnimator;

	public Animator timmyAnimator;

	private Rigidbody controller;

	private Transform playerCam;

	public Transform neckJnt;

	public GameObject book;

	public GameObject pillowVr;

	public Transform headFollowVR;

	public Transform _headFollow;

	public float torsoFollowSpeed;

	private Transform tr;

	private float normCamX;

	private float normCamY;

	private float smoothCamX;

	private float smoothCamY;

	private float mouseCurrentPosx;

	private float mouseDeltax;

	public float scaleXFactor = 0.05f;

	private Vector3 localHeadConstrainPos;

	public bool _client;
}
