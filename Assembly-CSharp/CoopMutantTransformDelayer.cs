using System;
using Bolt;
using UnityEngine;

public class CoopMutantTransformDelayer : EntityBehaviour<IMutantState>
{
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			base.enabled = false;
		}
		this.animControl = base.transform.GetComponentInChildren<mutantNetAnimatorControl>();
		this.rb = base.transform.GetComponent<Rigidbody>();
		this.tr = base.transform;
	}

	public void SetTemporaryRotation(Quaternion rotation, bool ignore)
	{
		base.state.RotationTransform.SetTransforms(null);
		if (ignore)
		{
			this.ignoreRotation = ignore;
		}
		else
		{
			this.ignoreRotation = false;
		}
		if (this.ignoreRotation)
		{
			return;
		}
		this.realRotation.transform.localRotation = base.transform.localRotation;
	}

	public void RestoreRotationReplication()
	{
		this.ignoreRotation = false;
		base.state.RotationTransform.SetTransforms(this.realRotation.transform);
	}

	public override void Attached()
	{
		if (base.entity.isOwner)
		{
			if (this.Creepy)
			{
				base.state.RotationTransform.SetTransforms(base.transform);
			}
			else
			{
				base.state.Transform.SetTransforms(base.transform);
				base.state.RotationTransform.SetTransforms(this.RotationTransform);
			}
		}
		else
		{
			this.timer = 0.55f;
			this.InterpolationDelay = 0.08f;
			this.MecanimReplicator.TargetAnimator.applyRootMotion = false;
			if (this.Creepy)
			{
				this.realRotation = new GameObject(base.entity.networkId + "_REAL_ROTATION");
				this.realRotation.transform.localPosition = base.transform.localPosition;
				this.realRotation.transform.localRotation = base.transform.localRotation;
				base.state.RotationTransform.SetTransforms(this.realRotation.transform);
			}
			else
			{
				this.realPosition = new GameObject(base.entity.networkId + "_REAL_POSITION");
				this.realPosition.transform.position = base.transform.position;
				base.state.Transform.SetTransforms(this.realPosition.transform);
				this.realRotation = new GameObject(base.entity.networkId + "_REAL_ROTATION");
				this.realRotation.transform.localPosition = this.RotationTransform.localPosition;
				this.realRotation.transform.localRotation = this.RotationTransform.localRotation;
				base.state.RotationTransform.SetTransforms(this.realRotation.transform);
			}
			if (this.realPosition != null)
			{
				this.pos = this.realPosition.transform.position;
			}
		}
	}

	private void Update()
	{
		if (base.entity.IsAttached() && !base.entity.IsOwner())
		{
			if (this.timer > 0f)
			{
				this.timer -= Time.deltaTime;
				if (this.Creepy)
				{
					base.transform.position = this.realRotation.transform.position;
					base.transform.rotation = this.realRotation.transform.rotation;
				}
				else
				{
					base.transform.position = this.realPosition.transform.position;
					this.RotationTransform.localPosition = this.realRotation.transform.localPosition;
					if (!this.ignoreRotation)
					{
						this.RotationTransform.localRotation = this.realRotation.transform.localRotation;
					}
				}
			}
			else
			{
				float num = 1f / this.InterpolationDelay;
				if (this.Creepy)
				{
					if (this.rb)
					{
						base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this.realRotation.transform.localPosition, Mathf.Clamp01(Time.deltaTime * num));
					}
					else
					{
						base.transform.localPosition = this.realRotation.transform.localPosition;
					}
					base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, this.realRotation.transform.localRotation, Mathf.Clamp01(Time.deltaTime * num));
				}
				else
				{
					if (this.ignoreDelay)
					{
						this.rb.MovePosition(this.realPosition.transform.position);
					}
					else if (this.realPosition != null)
					{
						this.rb.MovePosition(this.realPosition.transform.position);
					}
					if (this.realRotation != null)
					{
						this.RotationTransform.localPosition = this.realRotation.transform.localPosition;
					}
					if (!this.ignoreRotation && this.realRotation != null)
					{
						this.RotationTransform.rotation = this.realRotation.transform.rotation;
					}
				}
			}
		}
	}

	private float timer;

	private GameObject realPosition;

	private GameObject realRotation;

	private mutantNetAnimatorControl animControl;

	private Rigidbody rb;

	private Transform tr;

	[SerializeField]
	public Transform RotationTransform;

	[SerializeField]
	public CoopMecanimReplicator MecanimReplicator;

	[SerializeField]
	public bool Creepy;

	public bool ignoreDelay;

	[Range(0f, 1f)]
	[Header("Interpolation Delay (Ignored On Host)")]
	[SerializeField]
	public float InterpolationDelay = 0.08f;

	public bool ignoreRotation;

	private Vector3 pos;

	private Vector3 velRef;

	public float smoothDamp = 0.02f;
}
