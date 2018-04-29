using System;
using System.Collections;
using UnityEngine;


public class raftOnLand : MonoBehaviour
{
	
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			AmplifyMotionObject[] componentsInChildren = base.transform.GetComponentsInChildren<AmplifyMotionObject>(true);
			AmplifyMotionObjectBase[] componentsInChildren2 = base.transform.GetComponentsInChildren<AmplifyMotionObjectBase>(true);
			foreach (AmplifyMotionObject obj in componentsInChildren)
			{
				UnityEngine.Object.Destroy(obj);
			}
			foreach (AmplifyMotionObjectBase obj2 in componentsInChildren2)
			{
				UnityEngine.Object.Destroy(obj2);
			}
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

	
	private Rigidbody _rb;

	
	private float setWobble;

	
	private float slope = 0.5f;

	
	private float interval = 1f;

	
	private float smooth = 0.5f;

	
	private float tNext;

	
	private float dir = 1f;

	
	private IRaftState raftState;
}
