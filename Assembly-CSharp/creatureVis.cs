using System;
using UnityEngine;

public class creatureVis : MonoBehaviour
{
	private void Awake()
	{
		this.animator = base.transform.parent.GetComponent<Animator>();
		this._renderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
		this.control = base.transform.parent.GetComponent<lizardAnimatorControl>();
	}

	private void Start()
	{
		this.thisTr = base.transform;
		base.Invoke("getAmplifyObj", 0.1f);
	}

	private void Update()
	{
		if (this.renderCam == null && Camera.main)
		{
			this.renderCam = Camera.main;
		}
	}

	private void disableAnimation()
	{
		for (int i = 0; i < this.joints.Length; i++)
		{
			this.joints[i].SetActive(false);
		}
		this._renderer.enabled = false;
		if (this.disableAnimator)
		{
			this.animator.enabled = false;
		}
	}

	private void enableAnimation()
	{
		for (int i = 0; i < this.joints.Length; i++)
		{
			this.joints[i].SetActive(true);
		}
		if (this.control)
		{
			this.control.enabled = true;
		}
		this._renderer.enabled = true;
		if (this.disableAnimator)
		{
			this.animator.enabled = true;
		}
	}

	private PlayMakerFSM pmControl;

	public bool disableAnimator;

	public Animator animator;

	private Renderer _renderer;

	public Camera renderCam;

	private lizardAnimatorControl control;

	private Transform thisTr;

	public GameObject[] joints;

	private bool trigger;

	private bool isVisible;

	private bool amplifyTrigger;
}
