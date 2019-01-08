using System;
using System.Collections;
using UnityEngine;

public class raftOnLand : MonoBehaviour
{
	private void OnEnable()
	{
		if (BoltNetwork.isServer)
		{
			base.StartCoroutine(this.fixBounceOnSpawn());
		}
	}

	private void Start()
	{
		this._rb = base.transform.GetComponent<Rigidbody>();
		if (!this._push)
		{
			this._push = base.transform.GetComponentInChildren<RaftPush>();
		}
		if (BoltNetwork.isServer)
		{
			base.StartCoroutine(this.WaitAttachMP());
		}
	}

	public IEnumerator fixBounceOnSpawn()
	{
		this._rb = base.transform.GetComponent<Rigidbody>();
		this._storeDrag = this._rb.drag;
		float t = 0f;
		while (t < 1f)
		{
			this._rb.drag = 100f;
			this._rb.angularDrag = 100f;
			t += Time.deltaTime;
			yield return null;
		}
		this._rb.drag = this._storeDrag;
		this._rb.angularDrag = this._storeDrag;
		yield break;
	}

	private IEnumerator WaitAttachMP()
	{
		BoltEntity be = base.GetComponent<BoltEntity>();
		if (be)
		{
			while (!be.isAttached)
			{
				yield return null;
			}
			this.raftState = be.GetState<IRaftState>();
		}
		yield break;
	}

	private void Update()
	{
		if (this.raftState != null && this.raftState.InWater != this._push._buoyancy.InWater)
		{
			this.raftState.InWater = this._push._buoyancy.InWater;
		}
		if (this.testLand)
		{
			this.onLand = true;
		}
		else
		{
			this.onLand = false;
		}
		this.testLand = false;
	}

	public float massOnLand;

	public float massOnWater;

	public float maxWobble;

	public float wobbleSpeed;

	public float noiseX;

	public float noiseY;

	public float repeat = 1f;

	public bool onLand;

	private bool testLand;

	public GameObject wobbleBox;

	public ForceMode _forceMode;

	public RaftPush _push;

	public Transform BuildContainer;

	private Rigidbody _rb;

	private float _storeDrag;

	private IRaftState raftState;
}
