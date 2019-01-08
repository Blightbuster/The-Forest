using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Audio;
using TheForest.Buildings.World;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[AddComponentMenu("Buildings/Creation/Rock Fence Chunk Architect")]
	[DoNotSerializePublic]
	public class RockFenceChunkArchitect : WallChunkArchitect
	{
		protected override void OnBeginCollapse()
		{
			if (!BoltNetwork.isClient)
			{
				float num = Vector3.Distance(this._p1, this._p2);
				int num2 = Mathf.RoundToInt(num / this._offset);
				if (num2 < 1)
				{
					num2 = 1;
				}
				while (this._wallRoot.childCount > 0)
				{
					int num3 = Mathf.Min(num2, 5);
					num2 -= num3;
					Transform child = this._wallRoot.GetChild(0);
					Prefabs.Instance.SpawnFromPoolMP("misc", Prefabs.Instance.RockFenceChunkDestroyPrefabs[num3 - 1], child.position, child.rotation, false);
					child.transform.parent = null;
					UnityEngine.Object.Destroy(child.gameObject);
					if (num2 == 0)
					{
						break;
					}
				}
			}
			if (this._wallRoot)
			{
				UnityEngine.Object.Destroy(this._wallRoot.gameObject);
				this._wallRoot = null;
			}
			base.OnBeginCollapse();
		}

		protected override IEnumerator OnDeserialized()
		{
			if (!this._initialized)
			{
				if (this._p1 == Vector3.zero)
				{
					this._p1 = base.transform.position;
				}
				if (this._p2 == Vector3.zero)
				{
					this._p2 = base.transform.position + base.transform.forward * (this._offset * 7f);
				}
				yield return base.StartCoroutine(this.<OnDeserialized>__BaseCallProxy0());
			}
			yield break;
		}

		protected override void CreateStructure(bool isRepair = false)
		{
			Renderer renderer = null;
			if (isRepair)
			{
				LOD_GroupToggle component = this._wallRoot.GetComponent<LOD_GroupToggle>();
				if (component)
				{
					renderer = component._levels[1].Renderers[0];
				}
				base.Clear();
				base.StartCoroutine(base.DelayedAwake(true));
			}
			this._logLength = this._offset;
			this._logWidth = this._offset;
			int layer = LayerMask.NameToLayer("Prop");
			this._wallRoot = this.SpawnStructure();
			this._wallRoot.parent = base.transform;
			if (this._wasBuilt)
			{
				this._gridToken = InsideCheck.AddWallChunk(this._p1, this._p2, this._colliderHeight + 0.5f);
				GameObject gameObject = this._wallRoot.gameObject;
				gameObject.tag = "jumpObject";
				gameObject.layer = layer;
				BoxCollider boxCollider = this._wallRoot.gameObject.AddComponent<BoxCollider>();
				Vector3 vector = base.transform.InverseTransformVector(this._p2 - this._p1);
				boxCollider.size = new Vector3(this._logWidth, this._colliderHeight, Mathf.Abs(vector.z));
				Vector3 center = boxCollider.size / 2f;
				center.x = 0f;
				boxCollider.center = center;
				this._wallRoot.gameObject.AddComponent<WeaponHitSfxInfo>()._sfx = SfxInfo.SfxTypes.HitRock;
				this._wallRoot.gameObject.AddComponent<BuildingHealthHitRelay>();
				this._wallRoot.gameObject.AddComponent<gridObjectBlocker>();
				foreach (Renderer renderer2 in this._wallRoot.GetComponentsInChildren<Renderer>())
				{
					renderer2.transform.rotation *= Quaternion.Euler(UnityEngine.Random.Range(-this._randomFactor, this._randomFactor), UnityEngine.Random.Range(-this._randomFactor, this._randomFactor), UnityEngine.Random.Range(-this._randomFactor, this._randomFactor));
				}
				BuildingHealth component2 = base.GetComponent<BuildingHealth>();
				if (component2)
				{
					component2._renderersRoot = this._wallRoot.gameObject;
					if (BoltNetwork.isRunning)
					{
						component2.SetMpRandomDistortColliders(new Collider[]
						{
							boxCollider
						});
					}
				}
				if (!renderer)
				{
					Transform transform = new GameObject("RockWall LOD").transform;
					transform.gameObject.layer = 21;
					transform.parent = base.transform;
					transform.localRotation = Quaternion.identity;
					transform.localPosition = Vector3.zero;
					renderer = transform.gameObject.AddComponent<MeshRenderer>();
					renderer.sharedMaterial = Prefabs.Instance.RockFenceChunksBuiltPrefabsLOD1[0].sharedMaterial;
					MeshFilter meshFilter = transform.gameObject.AddComponent<MeshFilter>();
					int value = Mathf.RoundToInt(Vector3.Distance(this._p1, this._p2) / this._offset);
					int num = Mathf.Clamp(value, 0, Prefabs.Instance.RockFenceChunksBuiltPrefabsLOD1.Length - 1);
					meshFilter.sharedMesh = Prefabs.Instance.RockFenceChunksBuiltPrefabsLOD1[num].GetComponent<MeshFilter>().sharedMesh;
				}
				LOD_GroupToggle lod_GroupToggle = this._wallRoot.gameObject.AddComponent<LOD_GroupToggle>();
				lod_GroupToggle.enabled = false;
				lod_GroupToggle._levels = new LOD_GroupToggle.LodLevel[2];
				Renderer[] componentsInChildren;
				lod_GroupToggle._levels[0] = new LOD_GroupToggle.LodLevel
				{
					Renderers = componentsInChildren,
					VisibleDistance = 50f
				};
				lod_GroupToggle._levels[1] = new LOD_GroupToggle.LodLevel
				{
					Renderers = new Renderer[]
					{
						renderer
					},
					VisibleDistance = 10000f
				};
			}
		}

		public override Transform SpawnStructure()
		{
			Transform transform = new GameObject("FenceChunk").transform;
			transform.position = this._p1;
			transform.LookAt(this._p2);
			Vector3 vector = this._p1;
			Vector3 vector2 = this._p2 - this._p1;
			Vector3 normalized = Vector3.Scale(vector2, new Vector3(1f, 0f, 1f)).normalized;
			float y = Mathf.Tan(Vector3.Angle(vector2, normalized) * 0.0174532924f) * this._offset;
			Quaternion rotation = Quaternion.LookRotation(-vector2);
			bool flag = this._p1.y < this._p2.y;
			float num = Vector3.Distance(this._p1, this._p2);
			int i = Mathf.RoundToInt(num / this._offset);
			if (i < 1)
			{
				i = 1;
			}
			Vector3 a = normalized * this._offset;
			a.y = y;
			if (!flag)
			{
				a.y *= -1f;
			}
			vector.y -= 0.15f;
			while (i > 0)
			{
				int num2 = Mathf.Min(i, 5);
				i -= num2;
				Transform transform2;
				if (!this._wasBuilt)
				{
					transform2 = UnityEngine.Object.Instantiate<Transform>(Prefabs.Instance.RockFenceChunksGhostFillPrefabs[num2 - 1], vector, rotation);
				}
				else
				{
					transform2 = UnityEngine.Object.Instantiate<Transform>(Prefabs.Instance.RockFenceChunksBuiltPrefabs[num2 - 1], vector, rotation);
				}
				transform2.parent = transform;
				vector += a * (float)num2;
			}
			return transform;
		}

		protected override Quaternion RandomizeLogRotation(Quaternion logRot)
		{
			return logRot;
		}

		protected override void InitAdditionTrigger()
		{
		}

		protected override int GetLogCost()
		{
			int num = 0;
			float num2 = Vector3.Distance(this._p1, this._p2);
			int i = Mathf.RoundToInt(num2 / this._offset);
			if (i < 1)
			{
				i = 1;
			}
			while (i > 0)
			{
				int num3 = Mathf.Min(i, 5);
				i -= num3;
				switch (num3)
				{
				case 1:
					num += 2;
					break;
				case 2:
					num += 4;
					break;
				case 3:
					num += 4;
					break;
				case 4:
					num += 8;
					break;
				case 5:
					num += 9;
					break;
				}
			}
			return num;
		}

		protected override List<GameObject> GetBuiltRenderers(Transform wallRoot)
		{
			return (from r in wallRoot.GetComponentsInChildren<Renderer>()
			select r.gameObject).ToList<GameObject>();
		}

		public override void UpdateAddition(WallChunkArchitect.Additions addition)
		{
		}

		public override Transform BuiltLogPrefab
		{
			get
			{
				return Prefabs.Instance.RockFenceChunksBuiltPrefabs[0];
			}
		}

		public override float GetLevel()
		{
			return this._p1.y + this.GetHeight();
		}

		public override float GetHeight()
		{
			return this._colliderHeight + 0.2f;
		}

		public float _offset = 1f;

		public float _colliderHeight = 2.5f;

		public float _randomFactor = 20f;
	}
}
