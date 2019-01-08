using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class enableEffigy : MonoBehaviour
{
	private void Awake()
	{
		base.enabled = false;
		this.timer.Start();
	}

	private void Update()
	{
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false) && !this.lightBool)
		{
			Scene.HudGui.FireWidget.Shutdown();
			LocalPlayer.Inventory.SpecialItems.SendMessage("LightTheFire", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void GrabEnter()
	{
		base.enabled = !this.lightBool;
		if (base.enabled)
		{
			Scene.HudGui.FireWidget.ShowSingle(LocalPlayer.Inventory.DefaultLight._itemId, base.transform, SideIcons.Take);
		}
	}

	private void GrabExit()
	{
		base.enabled = false;
		if (Scene.HudGui)
		{
			Scene.HudGui.FireWidget.Shutdown();
		}
	}

	private void OnDestroy()
	{
		if (Scene.HudGui)
		{
			Scene.HudGui.FireWidget.Shutdown();
		}
	}

	private void OnSerializing()
	{
		this.timer.OnSerializing();
	}

	private IEnumerator OnDeserialized()
	{
		this.timer.OnDeserialized();
		if (this.lightBool && !BoltNetwork.isClient)
		{
			if (BoltNetwork.isRunning)
			{
				BoltEntity be = base.transform.parent.GetComponent<BoltEntity>();
				while (!be.isAttached)
				{
					yield return null;
				}
			}
			this.lightEffigy();
			base.Invoke("ShutdownFire", this.duration - this.timer._doneTime - 3f);
			base.Invoke("die", this.duration - this.timer._doneTime);
			if (Scene.HudGui)
			{
				Scene.HudGui.FireWidget.Shutdown();
			}
		}
		yield break;
	}

	private void receiveLightFire()
	{
		if (!this.lightBool && base.enabled)
		{
			this.lightEffigy();
			if (Scene.HudGui)
			{
				Scene.HudGui.FireWidget.Shutdown();
			}
			base.Invoke("ShutdownFire", this.duration - 3f);
			base.Invoke("die", this.duration);
			base.enabled = false;
			this.lightBool = true;
		}
	}

	private void lightEffigy()
	{
		if (BoltNetwork.isRunning)
		{
			LightEffigy lightEffigy = LightEffigy.Create(GlobalTargets.OnlyServer);
			lightEffigy.Effigy = base.GetComponentInParent<BoltEntity>();
			lightEffigy.Send();
		}
		else
		{
			this.lightEffigyReal();
		}
	}

	public void lightEffigyReal()
	{
		foreach (GameObject gameObject in this.fires)
		{
			if (gameObject)
			{
				gameObject.SetActive(true);
			}
		}
		this.effigyRange.SetActive(true);
		this.BirdMarkers.SetActive(false);
		FMODCommon.PlayOneshot(this.lightEvent, base.transform);
		if (Scene.HudGui && base.enabled)
		{
			Scene.HudGui.FireWidget.Shutdown();
			base.enabled = false;
		}
		this.lightBool = true;
	}

	public void dieReal()
	{
		this.BirdMarkers.SetActive(true);
		this.effigyRange.SetActive(false);
		this.lightBool = false;
		Transform transform = BoltNetwork.isRunning ? ItemDatabase.ItemById(this._boneItemId)._pickupPrefabMP : ItemDatabase.ItemById(this._boneItemId)._pickupPrefab;
		Transform transform2 = BoltNetwork.isRunning ? ItemDatabase.ItemById(this._skullItemId)._pickupPrefabMP : ItemDatabase.ItemById(this._skullItemId)._pickupPrefab;
		Transform transform3 = BoltNetwork.isRunning ? ItemDatabase.ItemById(this._rockItemId)._pickupPrefabMP : ItemDatabase.ItemById(this._rockItemId)._pickupPrefab;
		IEnumerator enumerator = this.Renderers.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform4 = (Transform)obj;
				if (transform && (transform4.name.StartsWith("Arm") || transform4.name.StartsWith("Leg")))
				{
					Transform transform5 = BoltNetwork.isRunning ? BoltNetwork.Instantiate(transform.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(transform);
					transform5.position = transform4.position;
					transform5.rotation = transform4.rotation;
				}
				else if (transform2 && transform4.name.StartsWith("Head"))
				{
					Transform transform6 = BoltNetwork.isRunning ? BoltNetwork.Instantiate(transform2.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(transform2);
					transform6.position = transform4.position;
					transform6.rotation = transform4.rotation;
				}
				else if (transform3 && transform4.name.StartsWith("Rock"))
				{
					Transform transform7 = BoltNetwork.isRunning ? BoltNetwork.Instantiate(transform3.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(transform3);
					transform7.position = transform4.position;
					transform7.rotation = transform4.rotation;
				}
				if (BoltNetwork.isRunning && transform4.GetComponent<BoltEntity>())
				{
					BoltNetwork.Destroy(transform4.gameObject);
				}
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
		UnityEngine.Object.Destroy(base.GetComponentInParent<PrefabIdentifier>().gameObject);
	}

	private void ShutdownFire()
	{
		foreach (GameObject gameObject in this.fires)
		{
			if (gameObject)
			{
				ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
				if (component)
				{
					component.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				}
				FMOD_StudioEventEmitter component2 = gameObject.GetComponent<FMOD_StudioEventEmitter>();
				if (component2)
				{
					component2.Stop();
				}
			}
		}
	}

	private void die()
	{
		if (BoltNetwork.isRunning)
		{
			if (BoltNetwork.isServer)
			{
				base.GetComponentInParent<BoltEntity>().GetState<IBuildingEffigyState>().Lit = false;
			}
		}
		else
		{
			this.dieReal();
		}
	}

	public GameObject[] fires;

	public GameObject effigyRange;

	public GameObject BirdMarkers;

	public GameObject Renderers;

	public GameObject LightBillboardIcon;

	public float duration;

	[ItemIdPicker]
	public int _boneItemId;

	[ItemIdPicker]
	public int _skullItemId;

	[ItemIdPicker]
	public int _rockItemId;

	[Header("FMOD")]
	public string lightEvent = "event:/fire/fire_built_start";

	[SerializeThis]
	public bool lightBool;

	[SerializeThis]
	private SerializableTimer timer = new SerializableTimer();
}
