using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Buildings.World;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class WallDefensiveGateArchitect : WallDefensiveChunkArchitect
	{
		
		protected override void Awake()
		{
			this._addition = WallChunkArchitect.Additions.Door1;
			base.Awake();
		}

		
		protected override void OnBuilt(GameObject built)
		{
			base.OnBuilt(built);
			WallDefensiveGate.AutoOpenDoor = !BoltNetwork.isClient;
		}

		
		protected override void CreateStructure(bool isRepair = false)
		{
			if (isRepair)
			{
				base.Clear();
				base.StartCoroutine(base.DelayedAwake(true));
			}
			Vector3 size = this._logRenderer.bounds.size;
			this._logLength = size.y;
			this._logWidth = size.z;
			this._wallRoot = this.SpawnStructure();
			this._wallRoot.parent = base.transform;
			if (this._wasBuilt)
			{
				this._gridToken = InsideCheck.AddWallChunk(this._p1, this._p2, this._logLength);
				Vector3 vector = this._wallRoot.InverseTransformVector(this._p2 - this._p1);
				Vector3 vector2 = new Vector3(this._logWidth, this._logLength, (!this.Is2Sided) ? Mathf.Abs(vector.z) : Mathf.Abs(vector.z / 2f));
				Vector3 center = vector2 / 2f;
				center.x = 0f;
				center.y -= 1f;
				center.z -= this._logWidth / 2f;
				GameObject gameObject = this._wallRoot.GetChild(0).gameObject;
				gameObject.tag = "structure";
				gameObject.layer = LayerMask.NameToLayer("Prop");
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
				BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
				boxCollider.size = vector2;
				boxCollider.center = center;
				gameObject.AddComponent<gridObjectBlocker>();
				gameObject.AddComponent<BuildingHealthHitRelay>();
				getStructureStrength getStructureStrength = gameObject.AddComponent<getStructureStrength>();
				getStructureStrength._type = getStructureStrength.structureType.wall;
				getStructureStrength._strength = getStructureStrength.strength.veryStrong;
				GameObject gameObject2;
				if (this._wallRoot.childCount > 1)
				{
					gameObject2 = this._wallRoot.GetChild(1).gameObject;
					gameObject2.tag = "structure";
					gameObject2.layer = LayerMask.NameToLayer("Prop");
					Rigidbody rigidbody2 = gameObject2.AddComponent<Rigidbody>();
					rigidbody2.useGravity = false;
					rigidbody2.isKinematic = true;
					boxCollider = gameObject2.AddComponent<BoxCollider>();
					boxCollider.size = vector2;
					boxCollider.center = center;
					gameObject2.AddComponent<gridObjectBlocker>();
					gameObject2.AddComponent<BuildingHealthHitRelay>();
					getStructureStrength getStructureStrength2 = gameObject2.AddComponent<getStructureStrength>();
					getStructureStrength2._type = getStructureStrength.structureType.wall;
					getStructureStrength2._strength = getStructureStrength.strength.veryStrong;
				}
				else
				{
					gameObject2 = null;
				}
				WallDefensiveGate wallDefensiveGate = UnityEngine.Object.Instantiate<WallDefensiveGate>(Prefabs.Instance.WallDefensiveGateTriggerPrefab);
				wallDefensiveGate.transform.parent = this._wallRoot;
				wallDefensiveGate.transform.position = new Vector3(this._wallRoot.position.x, this._wallRoot.position.y + 4f, this._wallRoot.position.z);
				wallDefensiveGate.transform.localRotation = Quaternion.identity;
				wallDefensiveGate._target1 = gameObject.transform;
				wallDefensiveGate._target2 = ((!gameObject2) ? null : gameObject2.transform);
				BuildingHealth component = base.GetComponent<BuildingHealth>();
				if (component)
				{
					component._renderersRoot = this._wallRoot.gameObject;
					if (BoltNetwork.isRunning)
					{
						component.SetMpRandomDistortColliders(new Collider[]
						{
							boxCollider
						});
					}
				}
			}
		}

		
		public override Transform SpawnStructure()
		{
			Vector3 position = Vector3.Lerp(this._p1, this._p2, 0.5f);
			Transform transform = new GameObject("WallChunk").transform;
			transform.position = position;
			transform.LookAt((!this.Inversed) ? new Vector3(this._p2.x, position.y, this._p2.z) : new Vector3(this._p1.x, position.y, this._p1.z));
			this.SpawnGate(this._p1, this._p2, transform);
			return transform;
		}

		
		private void SpawnGate(Vector3 p1, Vector3 p2, Transform wallTr)
		{
			Transform transform = new GameObject("GateChunk1").transform;
			Vector3 vector = p2 - p1;
			Vector3 normalized = Vector3.Scale(vector, new Vector3(1f, 0f, 1f)).normalized;
			float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._logWidth;
			Quaternion rotation = Quaternion.LookRotation(Vector3.forward);
			Vector3 a = Vector3.Lerp(p1, p2, 0.5f) + Vector3.down * 4f;
			Vector3 b = Vector3.right * (this._logWidth * 0.9f * (float)((!this.Inversed) ? 1 : -1));
			float num = Vector3.Distance(p1, p2);
			int num2 = Mathf.RoundToInt(num / this._logWidth);
			int num3 = num2 / 2 - 1;
			float num4 = (float)num2 / 2f;
			bool flag = num2 >= this._2SidedGateMinSize;
			Vector3 vector2 = normalized * num / (float)num2;
			vector2.y = y;
			if (vector.y < 0f)
			{
				vector2.y *= -1f;
			}
			Vector3 one = Vector3.one;
			float a2 = (!flag) ? 1.2f : 1.45f;
			Vector3 vector3 = p1;
			transform.position = vector3;
			transform.LookAt(new Vector3(p2.x, vector3.y, p2.z));
			transform.parent = wallTr;
			Transform transform2 = base.NewLog(vector3, Quaternion.LookRotation(a - vector3));
			transform2.parent = transform;
			transform2.localPosition += b;
			transform2.localScale = one;
			for (int i = 0; i < num2; i++)
			{
				Transform transform3 = base.NewLog(vector3, rotation);
				transform3.parent = transform;
				transform3.localScale = one;
				vector3 += vector2;
				if (i == num3)
				{
					if (flag)
					{
						transform = new GameObject("GateChunk2").transform;
						transform.position = p1 + (float)(num2 - 1) * vector2;
						transform.LookAt(new Vector3(p1.x, transform.position.y, p1.z));
						transform.parent = wallTr;
					}
				}
				else if (flag)
				{
					one.y = Mathf.Lerp(a2, 1f, (float)Mathf.Abs(num3 - i) / (float)num3);
				}
				else
				{
					one.y = Mathf.Lerp(a2, 1f, Mathf.Abs(num4 - (float)i) / num4);
				}
			}
			if (flag)
			{
				vector3 -= vector2;
				Transform transform4 = base.NewLog(transform.position, Quaternion.LookRotation(a - transform.position));
				transform4.parent = transform;
				transform4.localPosition -= b;
				transform4.localScale = one;
			}
		}

		
		public override void ShowToggleAdditionIcon()
		{
			WallChunkArchitect.Additions additions = this.SegmentNextAddition(this._addition);
			if (additions != WallChunkArchitect.Additions.Wall)
			{
				if (additions != WallChunkArchitect.Additions.Door1)
				{
					if (additions == WallChunkArchitect.Additions.Door2)
					{
						if (Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
						{
							Scene.HudGui.ToggleDefensiveWallIcon.SetActive(false);
						}
						if (Scene.HudGui.ToggleGate1Icon.activeSelf)
						{
							Scene.HudGui.ToggleGate1Icon.SetActive(false);
						}
						if (!Scene.HudGui.ToggleGate2Icon.activeSelf)
						{
							Scene.HudGui.ToggleGate2Icon.SetActive(true);
						}
					}
				}
				else
				{
					if (Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
					{
						Scene.HudGui.ToggleDefensiveWallIcon.SetActive(false);
					}
					if (!Scene.HudGui.ToggleGate1Icon.activeSelf)
					{
						Scene.HudGui.ToggleGate1Icon.SetActive(true);
					}
					if (Scene.HudGui.ToggleGate2Icon.activeSelf)
					{
						Scene.HudGui.ToggleGate2Icon.SetActive(false);
					}
				}
			}
			else
			{
				if (!Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleDefensiveWallIcon.SetActive(true);
				}
				if (Scene.HudGui.ToggleGate1Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate1Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleGate2Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate2Icon.SetActive(false);
				}
			}
		}

		
		public override void HideToggleAdditionIcon()
		{
			if (Scene.HudGui)
			{
				if (Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleDefensiveWallIcon.SetActive(false);
				}
				if (Scene.HudGui.ToggleGate1Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate1Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleGate2Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate2Icon.SetActive(false);
				}
			}
		}

		
		protected override WallChunkArchitect.Additions SegmentNextAddition(WallChunkArchitect.Additions addition)
		{
			if (addition == WallChunkArchitect.Additions.Door1)
			{
				return WallChunkArchitect.Additions.Door2;
			}
			return WallChunkArchitect.Additions.Wall;
		}

		
		protected override void InitAdditionTrigger()
		{
			base.GetComponentInChildren<Craft_Structure>().gameObject.AddComponent<WallAdditionTrigger>();
		}

		
		public override void UpdateAddition(WallChunkArchitect.Additions addition)
		{
			this._addition = addition;
			if (!this._wasBuilt)
			{
				if (!base.IsDoor(this._addition) && !BoltNetwork.isClient)
				{
					base.gameObject.GetComponent<WallDefensiveGateAddition>().Start();
				}
				else
				{
					UnityEngine.Object.Destroy(this._wallRoot.gameObject);
					this._wallRoot = this.SpawnStructure();
					this._wallRoot.parent = base.transform;
					Craft_Structure.BuildIngredients buildIngredients = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this._logItemId);
					if (buildIngredients == null)
					{
						buildIngredients = new Craft_Structure.BuildIngredients();
						buildIngredients._itemID = this._logItemId;
						buildIngredients._amount = 0;
						buildIngredients._renderers = new GameObject[0];
						this._craftStructure._requiredIngredients.Insert(0, buildIngredients);
					}
					else
					{
						buildIngredients._subIngredients.Clear();
					}
					buildIngredients._amount = this.GetLogCost();
					List<GameObject> builtRenderers = this.GetBuiltRenderers(this._wallRoot);
					buildIngredients.AddRuntimeObjects(builtRenderers, this.BuiltLogPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				}
			}
		}

		
		protected override List<GameObject> GetBuiltRenderers(Transform wallRoot)
		{
			List<GameObject> list = new List<GameObject>(12);
			IEnumerator enumerator = wallRoot.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					IEnumerator enumerator2 = transform.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Transform transform2 = (Transform)obj2;
							list.Add(transform2.GetComponentInChildren<Renderer>().gameObject);
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator2 as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = (enumerator as IDisposable)) != null)
				{
					disposable2.Dispose();
				}
			}
			return list;
		}

		
		protected override int GetLogCost()
		{
			return this._wallRoot.GetChild(0).childCount + ((this._wallRoot.childCount <= 1) ? 0 : this._wallRoot.GetChild(1).childCount);
		}

		
		
		private bool Inversed
		{
			get
			{
				return this._addition == WallChunkArchitect.Additions.Door2 || this._addition == WallChunkArchitect.Additions.LockedDoor2;
			}
		}

		
		
		public bool Is2Sided
		{
			get
			{
				return Mathf.RoundToInt(Vector3.Distance(this._p1, this._p2) / this._logWidth) >= this._2SidedGateMinSize;
			}
		}

		
		
		
		public override WallDefensiveChunkReinforcement Reinforcement
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		
		public int _2SidedGateMinSize = 8;
	}
}
