using System;
using System.Collections;
using Bolt;
using FMOD.Studio;
using UniLinq;
using UnityEngine;

[ExecuteInEditMode]
public class CoopTreeId : EntityBehaviour<ITreeCutState>, IPriorityCalculator
{
	private void Awake()
	{
		this.lod = base.GetComponent<LOD_Trees>();
		base.enabled = false;
	}

	private void Update()
	{
		base.state.Fuel += Time.deltaTime;
		if (this.lod)
		{
			if (base.state.Fuel >= this.lod.High.GetComponent<FireDamage>().FuelSeconds)
			{
				base.enabled = false;
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		this.c1 = null;
		this.c2 = null;
		this.c3 = null;
		this.c4 = null;
		this.dmg = null;
		this.lod = null;
		this.cut = null;
		this.fall = null;
		this.cut_chunks = null;
		this.NetworkPrefab = null;
		this.NetworkFallPrefab = null;
	}

	public void CheckBurning()
	{
		Debug.Log(string.Concat(new object[]
		{
			"CheckBurning ",
			base.name,
			" : ",
			base.state.Burning
		}));
		base.enabled = base.state.Burning;
	}

	private void Burnt(GameObject trunk)
	{
		if (!BoltNetwork.isClient && this.lod)
		{
			Debug.Log("Burnt " + base.name);
			this.lod.SendMessageToTargets("OnTreeCutDown", trunk);
			if (BoltNetwork.isRunning)
			{
				this.Goto_Removed();
			}
			UnityEngine.Object.Destroy(this.lod);
		}
	}

	public override void Attached()
	{
		base.state.AddCallback("State", new PropertyCallbackSimple(this.State));
		base.state.AddCallback("Chunk1", this.c1 = new PropertyCallbackSimple(this.ChunkUpdater1));
		base.state.AddCallback("Chunk2", this.c2 = new PropertyCallbackSimple(this.ChunkUpdater2));
		base.state.AddCallback("Chunk3", this.c3 = new PropertyCallbackSimple(this.ChunkUpdater3));
		base.state.AddCallback("Chunk4", this.c4 = new PropertyCallbackSimple(this.ChunkUpdater4));
		base.state.AddCallback("Damage", this.dmg = new PropertyCallbackSimple(this.Damage));
		if (BoltNetwork.isServer)
		{
			base.state.AddCallback("Burning", new PropertyCallbackSimple(this.CheckBurning));
		}
		base.enabled = false;
		if (BoltNetwork.isServer)
		{
			if (!this.lod)
			{
				base.state.State = 4;
			}
			else if (!this.lod.enabled)
			{
				base.state.State = 3;
			}
		}
		else
		{
			this.State();
		}
	}

	private void Damage()
	{
		if (base.state.Damage >= 16f && base.entity.isOwner && base.state.State == 1)
		{
			base.state.State = 2;
		}
	}

	private void Goto_Destroyed()
	{
		base.state.State = 3;
	}

	public void Goto_Removed()
	{
		if (base.entity.isAttached)
		{
			base.entity.Freeze(false);
			base.state.State = 4;
		}
	}

	public void State()
	{
		switch (base.state.State)
		{
		case 0:
			this.State_Pristine();
			break;
		case 1:
			this.State_Damaged();
			break;
		case 2:
			this.State_Falling();
			break;
		case 3:
			this.State_Destroyed();
			break;
		case 4:
			this.State_Removed();
			break;
		}
	}

	private void State_Pristine()
	{
		this.lod.enabled = true;
		this.lod.DontSpawn = false;
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				UnityEngine.Object.Destroy(transform.gameObject);
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
	}

	private void State_Destroyed()
	{
		base.state.FallingTransform.SetTransforms(null);
		if (base.entity.isOwner)
		{
			if (this.lod)
			{
				this.lod.enabled = false;
			}
			return;
		}
		bool flag = !this.fall;
		if (this.fall)
		{
			this.fall.BroadcastMessage("ActivateLeafParticles", SendMessageOptions.DontRequireReceiver);
			UnityEngine.Object.Destroy(this.fall);
			this.fall = null;
		}
		if (this.lod)
		{
			this.lod.enabled = false;
			this.lod.DontSpawn = true;
			if (this.lod.CurrentLodTransform)
			{
				UnityEngine.Object.Destroy(this.lod.CurrentLodTransform.gameObject);
			}
			if (flag)
			{
				this.lod.SpawnStumpLod();
			}
			else if (this.cut != null)
			{
				TreeHealth component = this.cut.GetComponent<TreeHealth>();
				if (component)
				{
					component.DestroyTrunk();
				}
			}
		}
		this.FinalCleanup();
	}

	private void State_Removed()
	{
		base.state.FallingTransform.SetTransforms(null);
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				UnityEngine.Object.Destroy(transform.gameObject);
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
		if (this.lod)
		{
			this.lod.enabled = false;
			if (this.lod.CurrentLodTransform)
			{
				this.lod.DespawnCurrent();
			}
			UnityEngine.Object.Destroy(this.lod);
			this.lod = null;
		}
		if (base.entity.isOwner)
		{
			return;
		}
		if (this.fall)
		{
			UnityEngine.Object.Destroy(this.fall);
			this.fall = null;
		}
		if (this.cut)
		{
			UnityEngine.Object.Destroy(this.cut);
			this.cut = null;
		}
		this.FinalCleanup();
	}

	private void FinalCleanup()
	{
		if (!base.entity.IsOwner())
		{
			this.cut_chunks = null;
			if (!PlayerPreferences.TreeRegrowth)
			{
				base.state.RemoveCallback("Chunk1", this.c1);
				base.state.RemoveCallback("Chunk1", this.c2);
				base.state.RemoveCallback("Chunk1", this.c3);
				base.state.RemoveCallback("Chunk1", this.c4);
				base.state.RemoveCallback("Damaged", this.dmg);
				base.state.RemoveCallback("Burning", new PropertyCallbackSimple(this.CheckBurning));
				this.c1 = null;
				this.c2 = null;
				this.c3 = null;
				this.c4 = null;
				this.dmg = null;
			}
		}
	}

	private void SpawnCutTree()
	{
		if (this.cut)
		{
			return;
		}
		this.cut = UnityEngine.Object.Instantiate<GameObject>(this.NetworkPrefab, base.transform.position, base.transform.rotation);
		TreeHealth component = this.cut.GetComponent<TreeHealth>();
		component.LodEntity = base.entity;
		component.SetLodBase(this.lod);
		this.cut_chunks = (from x in this.cut.GetComponentsInChildren<TreeCutChunk>()
		orderby int.Parse(x.transform.parent.gameObject.name)
		select x).ToArray<TreeCutChunk>();
	}

	private void State_Damaged()
	{
		EventInstance windEvent = TreeWindSfx.BeginTransfer(this.lod.CurrentLodTransform);
		this.SpawnCutTree();
		if (this.lod.CurrentLodTransform)
		{
			UnityEngine.Object.Destroy(this.lod.CurrentLodTransform.gameObject);
			this.lod.CurrentLodTransform = null;
		}
		if (this.cut)
		{
			TreeWindSfx.CompleteTransfer(this.cut.transform, windEvent);
			this.ChunkUpdater1();
			this.ChunkUpdater2();
			this.ChunkUpdater3();
			this.ChunkUpdater4();
		}
	}

	private float GetChunk1()
	{
		return base.state.Chunk1;
	}

	private float GetChunk2()
	{
		return base.state.Chunk2;
	}

	private float GetChunk3()
	{
		return base.state.Chunk3;
	}

	private float GetChunk4()
	{
		return base.state.Chunk4;
	}

	private void HideAllChunks()
	{
		this.UpdateChunks(0, 4f);
		this.UpdateChunks(1, 4f);
		this.UpdateChunks(2, 4f);
		this.UpdateChunks(3, 4f);
	}

	private void State_Falling()
	{
		this.State_Damaged();
		this.HideAllChunks();
		if (!this.fall)
		{
			this.fall = this.cut.GetComponent<TreeHealth>().DoFallTree();
			this.fall.gameObject.AddComponent<CoopOnDestroyCallback>().Callback = new Action(this.OnDestroyCallback);
			if (BoltNetwork.isClient)
			{
				base.StartCoroutine(this.AssignFallTransformDelayed());
			}
			else
			{
				base.state.FallingTransform.SetTransforms(this.fall.transform);
			}
		}
	}

	private IEnumerator AssignFallTransformDelayed()
	{
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		if (!this.fall)
		{
			yield break;
		}
		Vector3 pos = this.fall.transform.position;
		base.state.FallingTransform.SetTransforms(this.fall.transform);
		Rigidbody rb = this.fall.transform.GetComponentInChildren<Rigidbody>();
		if (rb)
		{
			rb.useGravity = false;
			rb.isKinematic = true;
		}
		this.fall.transform.position = pos;
		yield return null;
		this.fall.transform.position = pos;
		yield return null;
		this.fall.transform.position = pos;
		yield return null;
		this.fall.transform.position = pos;
		yield break;
	}

	private void OnDestroyCallback()
	{
		if (this && base.entity && base.entity.isOwner)
		{
			base.state.State = 3;
		}
	}

	private void LodChanged(int newLOD)
	{
		if (newLOD == 0 && base.entity.IsAttached() && base.state.Damage > 0f)
		{
			this.State_Damaged();
		}
	}

	private void ChunkUpdater1()
	{
		if (base.state.State != 1 || !this.cut || this.cut_chunks == null)
		{
			return;
		}
		this.UpdateChunks(0, this.GetChunk1());
	}

	private void ChunkUpdater2()
	{
		if (base.state.State != 1 || !this.cut || this.cut_chunks == null)
		{
			return;
		}
		this.UpdateChunks(1, this.GetChunk2());
	}

	private void ChunkUpdater3()
	{
		if (base.state.State != 1 || !this.cut || this.cut_chunks == null)
		{
			return;
		}
		this.UpdateChunks(2, this.GetChunk3());
	}

	private void ChunkUpdater4()
	{
		if (base.state.State != 1 || !this.cut || this.cut_chunks == null)
		{
			return;
		}
		this.UpdateChunks(3, this.GetChunk4());
	}

	private void UpdateChunks(int c, float value)
	{
		if (this.cut_chunks != null && c >= 0 && c < this.cut_chunks.Length && this.cut_chunks[c])
		{
			if (value < 1f)
			{
				this.cut_chunks[c].Fake1.SetActive(true);
				this.cut_chunks[c].Fake2.SetActive(false);
				this.cut_chunks[c].Fake3.SetActive(false);
				this.cut_chunks[c].Fake4.SetActive(false);
			}
			if (value >= 1f && value < 2f)
			{
				this.cut_chunks[c].Fake1.SetActive(false);
				this.cut_chunks[c].Fake2.SetActive(true);
			}
			if (value >= 2f && value < 3f)
			{
				this.cut_chunks[c].Fake1.SetActive(false);
				this.cut_chunks[c].Fake2.SetActive(false);
				this.cut_chunks[c].Fake3.SetActive(true);
			}
			if (value >= 3f && value < 4f)
			{
				this.cut_chunks[c].Fake1.SetActive(false);
				this.cut_chunks[c].Fake2.SetActive(false);
				this.cut_chunks[c].Fake3.SetActive(false);
				this.cut_chunks[c].Fake4.SetActive(true);
			}
			if (value >= 4f)
			{
				UnityEngine.Object.Destroy(this.cut_chunks[c].transform.parent.gameObject);
				base.state.Damage += 4f;
			}
		}
	}

	public void RegrowTree()
	{
		if (base.entity.isAttached)
		{
			base.entity.Freeze(false);
			base.state.State = 0;
			base.state.Damage = 0f;
			base.state.Chunk1 = 0f;
			base.state.Chunk2 = 0f;
			base.state.Chunk3 = 0f;
			base.state.Chunk4 = 0f;
		}
	}

	bool IPriorityCalculator.Always
	{
		get
		{
			return false;
		}
	}

	float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Bolt.Event evnt)
	{
		return (float)((base.state.State != 2) ? 256 : 8192);
	}

	float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped)
	{
		return (float)((base.state.State != 2) ? 256 : 8192);
	}

	public const int STATE_PRISTINE = 0;

	public const int STATE_DAMAGED = 1;

	public const int STATE_FALLING = 2;

	public const int STATE_DESTROYED = 3;

	public const int STATE_REMOVED = 4;

	[HideInInspector]
	public LOD_Trees lod;

	private GameObject cut;

	private GameObject fall;

	private TreeCutChunk[] cut_chunks;

	[SerializeField]
	public int Id;

	[SerializeField]
	public GameObject NetworkPrefab;

	[SerializeField]
	public GameObject NetworkFallPrefab;

	private PropertyCallbackSimple c1;

	private PropertyCallbackSimple c2;

	private PropertyCallbackSimple c3;

	private PropertyCallbackSimple c4;

	private PropertyCallbackSimple dmg;
}
