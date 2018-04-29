using System;
using TheForest.Commons.Delegates;
using TheForest.Utils;
using TheForest.Utils.Settings;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


public class GreebleZone : MonoBehaviour, IThreadSafeTask
{
	
	private void Awake()
	{
		if (this.MpSync && BoltNetwork.isClient)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			base.enabled = false;
		}
	}

	
	private void Start()
	{
		GreebleSelection randomSelection = this.RandomSelection;
		if (randomSelection != GreebleSelection.Normal)
		{
			if (randomSelection != GreebleSelection.RandomizeChances)
			{
				if (randomSelection == GreebleSelection.SelectOne)
				{
					UnityEngine.Random.seed = this.GetRandomSeed();
					int num = GreebleUtility.ProceduralValue(0, this.GreebleDefinitions.Length);
					for (int i = 0; i < this.GreebleDefinitions.Length; i++)
					{
						this.GreebleDefinitions[i].Chance = ((i != num) ? 0 : 1);
					}
				}
			}
			else
			{
				foreach (GreebleDefinition greebleDefinition in this.GreebleDefinitions)
				{
					greebleDefinition.Chance = GreebleUtility.ProceduralValue(1, 100);
				}
			}
		}
	}

	
	private void OnEnable()
	{
		if (this.Data == null)
		{
			this.Data = new GreebleZonesManager.GZData();
			this.Data._seed = -1;
		}
		int maxInstancesModified = this.MaxInstancesModified;
		if (this.Data._instancesState == null || this.Data._instancesState.Length < maxInstancesModified)
		{
			this.Data._instancesState = new byte[maxInstancesModified];
			for (int i = 0; i < this.Data._instancesState.Length; i++)
			{
				this.Data._instancesState[i] = 254;
			}
		}
		this.Data.GZ = this;
		this.position = base.transform.position;
		if (this.wsToken == -1)
		{
			this.wsToken = WorkScheduler.Register(this, base.transform.position, false);
		}
	}

	
	private void OnDisable()
	{
		if (this.wsToken >= 0)
		{
			WorkScheduler.Unregister(this, this.wsToken);
			this.wsToken = -1;
			this.Despawn();
		}
	}

	
	public void Refresh()
	{
		this.Despawn();
		this.Spawn();
	}

	
	private int GetModifiedInstanceCount(int rawCount)
	{
		if (this.InstanceCountModifier == GreebleZone.InstanceCountModifiers.None || GameSettings.Survival == null)
		{
			return rawCount;
		}
		if (this.InstanceCountModifier == GreebleZone.InstanceCountModifiers.HardSurvivalSuitcases)
		{
			return Mathf.CeilToInt((float)rawCount * GameSettings.Survival.SuitcaseCountRatio);
		}
		return rawCount;
	}

	
	private int GetRandomSeed()
	{
		int num;
		if (this.Data == null || this.Data._seed == -1)
		{
			Vector3 vector = base.transform.position;
			num = (int)vector.x + (int)vector.y + (int)vector.z + this.RandomSeed;
			if (this.Data != null)
			{
				this.Data._seed = num;
			}
		}
		else
		{
			num = this.Data._seed;
		}
		return num;
	}

	
	private void Spawn()
	{
		this.Despawn();
		UnityEngine.Random.seed = this.GetRandomSeed();
		this.InstanceCount = GreebleUtility.ProceduralValue(this.MinInstancesModified, this.MaxInstancesModified + 1);
		if (this.InstanceData == null || this.InstanceData.Length != this.InstanceCount)
		{
			this.InstanceData = new GreebleZone.GreebleInstanceData[this.InstanceCount];
			for (int i = 0; i < this.InstanceData.Length; i++)
			{
				this.InstanceData[i].Spawned = false;
				if (this.Data != null && Application.isPlaying)
				{
					this.InstanceData[i].Destroyed = (this.Data._instancesState[i] == byte.MaxValue);
				}
				else
				{
					this.InstanceData[i].Destroyed = false;
				}
				this.InstanceData[i].CreationTime = 0f;
			}
		}
		if (this.instances == null || this.instances.Length != this.InstanceCount)
		{
			this.instances = new GameObject[this.InstanceCount];
		}
		this.currentlyVisible = true;
		this.scheduledSpawnIndex = 0;
		this.ScheduledSpawn();
		UnityEngine.Random.seed = this.GetRandomSeed();
	}

	
	private void ScheduledSpawn()
	{
		if (!this.currentlyVisible)
		{
			return;
		}
		if (this.scheduledSpawnIndex < this.InstanceCount)
		{
			this.SpawnIndex(this.scheduledSpawnIndex);
		}
		this.scheduledSpawnIndex++;
		if (this.scheduledSpawnIndex < this.InstanceCount)
		{
			WorkScheduler.RegisterOneShot(new WsTask(this.ScheduledSpawn));
		}
	}

	
	private void SpawnIndex(int index)
	{
		if (!this.AllowRegrowth && this.InstanceData[index].Destroyed)
		{
			return;
		}
		UnityEngine.Random.seed = this.GetRandomSeed() + index;
		GreebleDefinition greebleDefinition = GreebleUtility.ProceduralGreebleType(this.GreebleDefinitions, this.AllowRegrowth && this.InstanceData[index].Destroyed, (!Scene.Clock) ? 1000000f : (Scene.Clock.ElapsedGameTime - this.InstanceData[index].CreationTime));
		if (greebleDefinition == null)
		{
			UnityEngine.Random.seed = this.GetRandomSeed();
			return;
		}
		Vector3 a = Vector3.zero;
		Vector3 vector = Vector3.down;
		float distance = this.Radius;
		GreebleShape shape = this.Shape;
		if (shape != GreebleShape.Sphere)
		{
			if (shape == GreebleShape.Box)
			{
				Vector3 vector2 = this.Size * 0.5f;
				Vector3 b = new Vector3(GreebleUtility.ProceduralValue(-vector2.x, vector2.x), GreebleUtility.ProceduralValue(-vector2.y, vector2.y), GreebleUtility.ProceduralValue(-vector2.z, vector2.z));
				int num = 0;
				switch (this.Direction)
				{
				case GreebleRayDirection.Floor:
					num = 4;
					break;
				case GreebleRayDirection.Ceiling:
					num = 5;
					break;
				case GreebleRayDirection.Walls:
					num = GreebleUtility.ProceduralValue(0, 4);
					break;
				case GreebleRayDirection.AllDirections:
					num = GreebleUtility.ProceduralValue(0, 6);
					break;
				}
				switch (num)
				{
				case 0:
					vector = Vector3.left;
					b.x = vector2.x;
					distance = this.Size.x;
					break;
				case 1:
					vector = Vector3.right;
					b.x = -vector2.x;
					distance = this.Size.x;
					break;
				case 2:
					vector = Vector3.back;
					b.z = vector2.z;
					distance = this.Size.z;
					break;
				case 3:
					vector = Vector3.forward;
					b.z = -vector2.z;
					distance = this.Size.z;
					break;
				case 4:
					vector = Vector3.down;
					b.y = vector2.y;
					distance = this.Size.y;
					break;
				case 5:
					vector = Vector3.up;
					b.y = -vector2.y;
					distance = this.Size.y;
					break;
				}
				a += b;
			}
		}
		else
		{
			distance = this.Radius;
			Vector3 zero = Vector3.zero;
			do
			{
				zero = new Vector3(GreebleUtility.ProceduralValue(-this.Radius, this.Radius), GreebleUtility.ProceduralValue(-this.Radius, this.Radius), GreebleUtility.ProceduralValue(-this.Radius, this.Radius));
			}
			while (zero.magnitude > this.Radius);
			switch (this.Direction)
			{
			case GreebleRayDirection.Floor:
				a += new Vector3(zero.x, 0f, zero.y);
				vector = Vector3.down;
				break;
			case GreebleRayDirection.Ceiling:
				a += new Vector3(zero.x, 0f, zero.y);
				vector = Vector3.up;
				break;
			case GreebleRayDirection.Walls:
				vector = GreebleUtility.ProceduralDirectionFast();
				vector.y = 0f;
				vector.Normalize();
				a += zero - Vector3.Project(zero, vector);
				break;
			case GreebleRayDirection.AllDirections:
				vector = GreebleUtility.ProceduralDirection();
				break;
			}
		}
		Quaternion rotation = Quaternion.identity;
		if (greebleDefinition.RandomizeRotation)
		{
			rotation = Quaternion.Euler((!greebleDefinition.AllowRotationX) ? 0f : GreebleUtility.ProceduralAngle(), (!greebleDefinition.AllowRotationY) ? 0f : GreebleUtility.ProceduralAngle(), (!greebleDefinition.AllowRotationZ) ? 0f : GreebleUtility.ProceduralAngle());
		}
		this.instances[index] = GreebleUtility.Spawn(greebleDefinition, new Ray(base.transform.TransformPoint(a), base.transform.TransformDirection(vector)), distance, rotation, 0.5f);
		if (this.Data != null && this.instances[index] != null && Application.isPlaying)
		{
			try
			{
				if (greebleDefinition.HasCustomActiveValue)
				{
					CustomActiveValueGreeble component = this.instances[index].GetComponent<CustomActiveValueGreeble>();
					component.Data = this.Data;
					component.Index = index;
					if (this.Data._instancesState[index] > 252)
					{
						this.Data._instancesState[index] = 252;
					}
				}
				else
				{
					this.Data._instancesState[index] = 253;
				}
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}
		if (this.instances[index])
		{
			if (this.MpSync && BoltNetwork.isServer)
			{
				BoltNetwork.Attach(this.instances[index]);
			}
			this.InstanceData[index].Spawned = true;
			this.InstanceData[index].Destroyed = false;
			if (this.OnSpawned != null)
			{
				this.OnSpawned(index, this.instances[index]);
			}
		}
		UnityEngine.Random.seed = this.GetRandomSeed();
	}

	
	public void SpawnAll()
	{
		this.Despawn();
		this.Start();
		UnityEngine.Random.seed = this.GetRandomSeed();
		this.InstanceCount = GreebleUtility.ProceduralValue(this.MinInstancesModified, this.MaxInstancesModified + 1);
		if (this.InstanceData == null || this.InstanceData.Length != this.InstanceCount)
		{
			this.InstanceData = new GreebleZone.GreebleInstanceData[this.InstanceCount];
			for (int i = 0; i < this.InstanceData.Length; i++)
			{
				this.InstanceData[i].Spawned = false;
				if (this.Data != null && Application.isPlaying)
				{
					this.InstanceData[i].Destroyed = (this.Data._instancesState[i] == byte.MaxValue);
				}
				else
				{
					this.InstanceData[i].Destroyed = false;
				}
				this.InstanceData[i].CreationTime = 0f;
			}
		}
		if (this.instances == null || this.instances.Length != this.InstanceCount)
		{
			this.instances = new GameObject[this.InstanceCount];
		}
		this.currentlyVisible = true;
		for (int j = 0; j < this.InstanceCount; j++)
		{
			this.SpawnIndex(j);
		}
		UnityEngine.Random.seed = this.GetRandomSeed();
	}

	
	public void Despawn()
	{
		if (this.instances == null || this.InstanceData == null)
		{
			return;
		}
		for (int i = 0; i < this.instances.Length; i++)
		{
			if (this.currentlyVisible && i < this.scheduledSpawnIndex && (!this.instances[i] || !this.instances[i].activeSelf) && this.InstanceData != null && this.InstanceData[i].Spawned)
			{
				if (this.AllowRegrowth && Scene.Clock)
				{
					this.InstanceData[i].CreationTime = Scene.Clock.ElapsedGameTime;
				}
				this.InstanceData[i].Destroyed = true;
				if (this.Data != null)
				{
					this.Data._instancesState[i] = byte.MaxValue;
				}
			}
			if (Application.isPlaying)
			{
				if (this.MpSync && BoltNetwork.isServer && this.instances[i])
				{
					BoltNetwork.Detach(this.instances[i]);
				}
				GreeblePlugin.Destroy(this.instances[i]);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(this.instances[i]);
			}
			this.InstanceData[i].Spawned = false;
			this.instances[i] = null;
		}
		this.currentlyVisible = false;
	}

	
	private void OnDestroy()
	{
		this.instances = null;
		this.InstanceData = null;
		this.Data = null;
		this.GreebleDefinitions = null;
		this.OnSpawned = null;
	}

	
	public void ThreadedRefresh()
	{
		float num;
		if (!this.MpSync)
		{
			num = (GreeblePlugin.GetCameraPosition() - this.position).magnitude;
		}
		else
		{
			num = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.position);
		}
		float num2 = 0f;
		GreebleShape shape = this.Shape;
		if (shape != GreebleShape.Sphere)
		{
			if (shape == GreebleShape.Box)
			{
				Vector2 vector = new Vector2(this.Size.x, this.Size.z);
				num2 = vector.magnitude;
			}
		}
		else
		{
			num2 = this.Radius;
		}
		float num3 = num - num2;
		this.nextVisible = (num3 < this.ToggleDistance * TheForestQualitySettings.UserSettings.DrawDistanceGreebleRatio * ((!this.currentlyVisible) ? 1f : 1.1f));
		if (this.nextVisible != this.currentlyVisible)
		{
			this.ShouldDoMainThreadRefresh = true;
		}
		else if (this.ShouldDoMainThreadRefresh)
		{
			this.ShouldDoMainThreadRefresh = false;
		}
	}

	
	public void MainThreadRefresh()
	{
		if (this.nextVisible != this.currentlyVisible)
		{
			if (this.nextVisible)
			{
				this.OnEnable();
				this.Spawn();
			}
			else
			{
				this.Despawn();
			}
		}
	}

	
	
	public int MaxInstancesModified
	{
		get
		{
			return this.GetModifiedInstanceCount(this.MaxInstances);
		}
	}

	
	
	public int MinInstancesModified
	{
		get
		{
			return this.GetModifiedInstanceCount(this.MaxInstances);
		}
	}

	
	
	
	public GreebleZonesManager.GZData Data { get; set; }

	
	
	public bool CurrentlyVisible
	{
		get
		{
			return this.currentlyVisible;
		}
	}

	
	
	public GameObject[] Instances
	{
		get
		{
			return this.instances;
		}
	}

	
	
	
	public bool ShouldDoMainThreadRefresh { get; set; }

	
	[Range(1f, 1000f)]
	public int MinInstances = 10;

	
	[Range(1f, 1000f)]
	public int MaxInstances = 10;

	
	public GreebleZone.InstanceCountModifiers InstanceCountModifier;

	
	public GreebleShape Shape;

	
	public GreebleRayDirection Direction;

	
	[Range(1f, 50f)]
	public float Radius = 10f;

	
	public Vector3 Size = new Vector3(50f, 50f, 50f);

	
	[Range(10f, 500f)]
	public float ToggleDistance = 50f;

	
	[Range(0f, 100000f)]
	public int RandomSeed;

	
	public GreebleSelection RandomSelection;

	
	public bool AllowRegrowth;

	
	public bool MpSync;

	
	public GreebleDefinition[] GreebleDefinitions;

	
	public Action<int, GameObject> OnSpawned;

	
	private int InstanceCount;

	
	private GreebleZone.GreebleInstanceData[] InstanceData;

	
	private bool currentlyVisible;

	
	private bool nextVisible;

	
	private GameObject[] instances;

	
	private int wsToken = -1;

	
	private int scheduledSpawnIndex;

	
	private Vector3 position;

	
	public struct GreebleInstanceData
	{
		
		public bool Spawned;

		
		public bool Destroyed;

		
		public float CreationTime;
	}

	
	public enum InstanceCountModifiers
	{
		
		None,
		
		HardSurvivalSuitcases
	}
}
