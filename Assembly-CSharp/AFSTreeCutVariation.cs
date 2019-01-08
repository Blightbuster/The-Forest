using System;
using UnityEngine;

public class AFSTreeCutVariation : MonoBehaviour
{
	private void Awake()
	{
		this.m_renderer = base.GetComponent<Renderer>();
		this.m_rb = base.GetComponent<Rigidbody>();
		if (!this.m_rb)
		{
			this.m_rb = base.transform.root.GetComponent<Rigidbody>();
		}
		this.CutVersionBlock = new MaterialPropertyBlock();
		this.setProperty();
	}

	private void OnEnable()
	{
		this.lastPos = this.m_rb.transform.position;
		this.lastRot = this.m_rb.transform.rotation;
		this.velocity = new Vector3(0.001f, 0.001f, 0.001f);
		this.updateBending();
	}

	private void FixedUpdate()
	{
		this.updateBending();
	}

	private void updateBending()
	{
		Vector3 target = this.m_rb.velocity;
		Vector3 target2 = this.m_rb.angularVelocity;
		if (BoltNetwork.isClient)
		{
			Quaternion quaternion = this.m_rb.transform.rotation * Quaternion.Inverse(this.lastRot);
			Vector3 a = new Vector3(Mathf.DeltaAngle(0f, quaternion.eulerAngles.x), Mathf.DeltaAngle(0f, quaternion.eulerAngles.y), Mathf.DeltaAngle(0f, quaternion.eulerAngles.z));
			target2 = a / Time.fixedDeltaTime * 0.0174532924f;
			this.lastRot = this.m_rb.transform.rotation;
			target = (this.m_rb.worldCenterOfMass - this.lastPos) / Time.fixedDeltaTime;
			this.lastPos = this.m_rb.worldCenterOfMass;
		}
		this.velocity = Vector3.SmoothDamp(this.velocity, target, ref this.smoothVelocity, this.smoothTime);
		this.angularVelocity = Vector3.SmoothDamp(this.angularVelocity, target2, ref this.smoothAngularVelocity, this.smoothTime);
		this.finalBendingStrength = Mathf.Clamp(this.angularVelocity.magnitude * this.BendingMultiplier, 0f, this.maxBendingStrength);
		this.finalTumblingStrength = Mathf.Clamp(this.angularVelocity.magnitude * this.TumblingMultiplier, 0f, this.maxTumblingStrength);
		this.t_TumbleFrequency = Mathf.Lerp(1f, this.FrequencyMultiplier, this.finalBendingStrength / this.maxBendingStrength);
		this.TumbleFrequency = Mathf.SmoothDamp(this.TumbleFrequency, this.t_TumbleFrequency, ref this.smoothTumbleFrequency, this.smoothTime);
		this.n_fallingWindVec = new Vector4(0f, this.finalTumblingStrength / base.transform.lossyScale.y, this.finalBendingStrength / base.transform.lossyScale.y, this.TumbleFrequency);
		this.n_angularVelocity = this.angularVelocity;
		this.n_angularVelocity.Normalize();
		this.n_velocity = this.velocity;
		this.n_velocity.Normalize();
		this.m_renderer.GetPropertyBlock(this.CutVersionBlock);
		this.CutVersionBlock.SetVector("_FallingDir", this.n_velocity);
		this.CutVersionBlock.SetVector("_FallingWind", this.n_fallingWindVec);
		this.CutVersionBlock.SetVector("_FallingRotDir", this.n_angularVelocity);
		this.m_renderer.SetPropertyBlock(this.CutVersionBlock);
	}

	private void setProperty()
	{
		this.m_renderer.GetPropertyBlock(this.CutVersionBlock);
		this.CutVersionBlock.SetFloat("_TreeBendingMode", 1f);
		this.CutVersionBlock.SetFloat("_Variation", Mathf.Abs(Mathf.Abs(base.transform.position.x + base.transform.position.z) * 0.1f % 1f - 0.5f) * 2f);
		Quaternion q = Quaternion.AngleAxis(-base.transform.rotation.eulerAngles.y, Vector3.up);
		this.m_Matrix.SetTRS(Vector3.zero, q, new Vector3(1f, 1f, 1f));
		this.CutVersionBlock.SetMatrix("_TreeRotMatrix", this.m_Matrix);
		this.m_renderer.SetPropertyBlock(this.CutVersionBlock);
	}

	[Space(5f)]
	public float BendingMultiplier = 10f;

	public float maxBendingStrength = 10f;

	[Space(5f)]
	public float TumblingMultiplier = 400f;

	public float maxTumblingStrength = 100f;

	[Range(0.1f, 4f)]
	public float FrequencyMultiplier = 1.5f;

	[Space(5f)]
	public float smoothTime = 0.35f;

	[Space(10f)]
	[Header("Debug")]
	public float finalBendingStrength;

	public float finalTumblingStrength;

	private MaterialPropertyBlock CutVersionBlock;

	private Matrix4x4 m_Matrix;

	private Renderer m_renderer;

	private Rigidbody m_rb;

	private Vector3 smoothVelocity = Vector3.zero;

	private Vector3 smoothAngularVelocity = Vector3.zero;

	private float smoothTumbleFrequency;

	private float TumbleFrequency;

	private float t_TumbleFrequency;

	private Vector3 velocity;

	private Vector3 angularVelocity;

	private Vector3 lastPos;

	private Quaternion lastRot;

	private Vector3 n_velocity;

	private Vector3 n_angularVelocity;

	private Vector4 n_fallingWindVec;
}
