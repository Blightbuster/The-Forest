using System;
using System.Collections;
using TheForest.Buildings.World;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[DoNotSerializePublic]
	public class SmallWallChunkArchitect : WallChunkArchitect
	{
		protected override void CreateStructure(bool isRepair = false)
		{
			if (isRepair)
			{
				base.Clear();
				base.StartCoroutine(base.DelayedAwake(true));
			}
			int num = LayerMask.NameToLayer("Prop");
			this._wallRoot = this.SpawnStructure();
			this._wallRoot.parent = base.transform;
			if (this._wasBuilt)
			{
				this._gridToken = InsideCheck.AddWallChunk(this._p1, this._p2, 4.75f * this._logWidth + 1f);
				BuildingHealth component = base.GetComponent<BuildingHealth>();
				if (component)
				{
					component._renderersRoot = this._wallRoot.gameObject;
				}
				if (!this._wallCollision)
				{
					GameObject gameObject = new GameObject("collision");
					gameObject.transform.parent = this._wallRoot.parent;
					gameObject.transform.position = this._wallRoot.position;
					gameObject.transform.rotation = this._wallRoot.rotation;
					gameObject.tag = "structure";
					this._wallCollision = gameObject.transform;
					GameObject gameObject2 = this._wallRoot.gameObject;
					int layer = num;
					gameObject.layer = layer;
					gameObject2.layer = layer;
					Vector3 vector;
					Vector3 size;
					if (this.UseHorizontalLogs)
					{
						float num2 = Vector3.Distance(this._p1, this._p2) / this._logLength;
						float num3 = 7.4f * num2;
						float num4 = 6.75f * (0.31f + (num2 - 1f) / 2f);
						vector = Vector3.Lerp(this._p1, this._p2, 0.5f);
						vector.y += this._logWidth * 0.9f;
						vector = this._wallRoot.InverseTransformPoint(vector);
						size = new Vector3(1.75f, 1.8f * this._logWidth, num3 * 1f);
					}
					else
					{
						float num3 = this._logWidth * (float)(this._wallRoot.childCount - 1) + 1.5f;
						vector = Vector3.zero;
						IEnumerator enumerator = this._wallRoot.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object obj = enumerator.Current;
								Transform transform = (Transform)obj;
								vector += transform.position;
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
						vector /= (float)this._wallRoot.childCount;
						vector.y += this.GetHeight() / 2f;
						vector = this._wallRoot.InverseTransformPoint(vector);
						size = new Vector3(1.75f, this.GetHeight(), num3);
					}
					getStructureStrength getStructureStrength = gameObject.AddComponent<getStructureStrength>();
					getStructureStrength._strength = getStructureStrength.strength.strong;
					BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
					boxCollider.center = vector;
					boxCollider.size = size;
					boxCollider.isTrigger = true;
					BoxCollider boxCollider2 = gameObject.AddComponent<BoxCollider>();
					boxCollider2.center = vector;
					boxCollider2.size = size;
					gridObjectBlocker gridObjectBlocker = gameObject.AddComponent<gridObjectBlocker>();
					gridObjectBlocker.ignoreOnDisable = true;
					addToBuilt addToBuilt = gameObject.AddComponent<addToBuilt>();
					addToBuilt.addToStructures = true;
					BuildingHealthHitRelay buildingHealthHitRelay = gameObject.AddComponent<BuildingHealthHitRelay>();
				}
			}
		}

		public override Transform SpawnStructure()
		{
			Transform transform = new GameObject("SmallWallChunk").transform;
			transform.transform.position = this._p1;
			Vector3 vector = this._p2 - this._p1;
			if (this.UseHorizontalLogs)
			{
				Vector3 b = new Vector3(0f, this._logWidth * 0.95f, 0f);
				Quaternion rotation = Quaternion.LookRotation(vector);
				Vector3 vector2 = this._p1;
				transform.position = this._p1;
				transform.LookAt(this._p2);
				Vector3 localScale = new Vector3(1f, 1f, vector.magnitude / this._logLength);
				Vector3 vector3 = new Vector3(1f, 1f, 0.31f + (localScale.z - 1f) / 2f);
				float num = 1f - vector3.z / localScale.z;
				for (int i = 0; i < 2; i++)
				{
					Transform transform2 = base.NewLog(vector2, rotation);
					transform2.parent = transform;
					transform2.localScale = localScale;
					vector2 += b;
				}
			}
			else
			{
				Vector3 normalized = Vector3.Scale(vector, new Vector3(1f, 0f, 1f)).normalized;
				float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._logWidth;
				Quaternion rotation2 = Quaternion.LookRotation(Vector3.up);
				Vector3 localScale2 = new Vector3(1f, 1f, 1.9f * this._logWidth / this._logLength);
				float num2 = this._logWidth / 2f * 0.98f;
				float num3 = Vector3.Distance(this._p1, this._p2);
				int num4 = Mathf.Max(Mathf.RoundToInt((num3 - this._logWidth * 0.96f / 2f) / this._logWidth), 1);
				Vector3 vector4 = normalized * this._logWidth;
				vector4.y = y;
				if (vector.y < 0f)
				{
					vector4.y *= -1f;
				}
				Vector3 vector5 = this._p1;
				vector5.y -= num2;
				vector5 += vector4 / 2f;
				transform.position = this._p1;
				transform.LookAt(this._p2);
				transform.eulerAngles = Vector3.Scale(transform.localEulerAngles, Vector3.up);
				for (int j = 0; j < num4; j++)
				{
					Transform transform3 = base.NewLog(vector5, rotation2);
					transform3.parent = transform;
					vector5 += vector4;
					transform3.localScale = localScale2;
				}
			}
			return transform;
		}

		protected override void InitAdditionTrigger()
		{
		}

		protected override int GetLogCost()
		{
			return this._wallRoot.childCount;
		}

		public override float GetHeight()
		{
			return 1.9f * this._logWidth;
		}
	}
}
