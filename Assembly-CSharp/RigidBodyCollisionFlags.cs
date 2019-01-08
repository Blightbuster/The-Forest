using System;
using System.Collections.Generic;
using TheForest.Utils.Physics;
using UnityEngine;

[AddComponentMenu("Physics/Rigidbody Collision Flags")]
[RequireComponent(typeof(Collider))]
public class RigidBodyCollisionFlags : MonoBehaviour, IOnCollisionEnterProxy, IOnCollisionExitProxy, IOnCollisionStayProxy
{
	private void Awake()
	{
		this._trans = base.transform;
		this.cColl = base.GetComponent<CapsuleCollider>();
		this.collisionCount = 0;
	}

	public void OnCollisionEnterProxied(Collision other)
	{
		if (!this.collCheck.Contains(other.gameObject))
		{
			this.collCheck.Add(other.gameObject);
		}
		this.collisionCount++;
	}

	public void OnCollisionExitProxied(Collision other)
	{
		if (this.collCheck.Contains(other.gameObject))
		{
			this.collCheck.Remove(other.gameObject);
		}
		this.collisionCount--;
	}

	private void FixedUpdate()
	{
		if (this.collCheck.Count == 0)
		{
			this.collFlags = CollisionFlags.None;
			this.groundAngleVal = 0f;
		}
		if (this.testBelow)
		{
			this.anyPointBelow = true;
		}
		else
		{
			this.anyPointBelow = false;
		}
		this.testBelow = false;
	}

	public void OnCollisionStayProxied(Collision collInfo)
	{
		if (!this.coll)
		{
			this.coll = base.GetComponent<Collider>();
			this.GetCollType();
		}
		this.collFlags = CollisionFlags.None;
		Vector3 center = this.coll.bounds.center;
		switch (this.collType)
		{
		case 1:
		{
			Vector3 vector = this._trans.up * ((this.cColl.height - this.cColl.radius * 2f) * 0.5f);
			vector = Vector3.Scale(vector, this._trans.localScale);
			Vector3 lineP = center - vector;
			Vector3 lineP2 = center + vector;
			for (int i = 0; i < collInfo.contacts.Length; i++)
			{
				int num = 0;
				float num2 = UEx.SqrLineDistance(lineP, lineP2, collInfo.contacts[i].point, out num);
				if (collInfo.contacts[i].point.y < lineP.y + this.heightOffset)
				{
					Vector3 vector2 = collInfo.contacts[i].point;
					Vector3 normal = collInfo.contacts[i].normal;
					vector2 += normal;
					base.transform.position.y = lineP.y + this.heightOffset;
					RaycastHit raycastHit;
					if (collInfo.contacts[i].otherCollider.Raycast(new Ray(vector2, -normal), out raycastHit, 2f))
					{
						this.groundAngleVal = Mathf.Abs(Vector3.Angle(raycastHit.normal, Vector3.up));
					}
					else
					{
						this.groundAngleVal = 0f;
					}
					this.slopeNormal = raycastHit.normal;
					this.collFlags |= CollisionFlags.Below;
					this.testBelow = true;
				}
				if (collInfo.contacts[i].point.y < lineP.y + 0.8f)
				{
					this.stuckPointCount++;
					if (this.stuckPointCount > 2)
					{
						this.testBelow = true;
					}
				}
				else if (num == 2)
				{
					this.collFlags |= CollisionFlags.Above;
					this.groundAngleVal = 0f;
				}
				else
				{
					this.collFlags |= CollisionFlags.Sides;
					this.groundAngleVal = 0f;
				}
			}
			this.stuckPointCount = 0;
			break;
		}
		case 2:
			foreach (ContactPoint contactPoint in collInfo.contacts)
			{
				Vector3 rhs = contactPoint.point - center;
				rhs.Normalize();
				float num3 = Vector3.Dot(this._trans.up, rhs);
				if (num3 < 0.333f)
				{
					this.collFlags |= CollisionFlags.Below;
				}
				else if (num3 > 0.333f)
				{
					this.collFlags |= CollisionFlags.Above;
				}
				else
				{
					this.collFlags |= CollisionFlags.Sides;
				}
			}
			break;
		case 3:
			foreach (ContactPoint contactPoint2 in collInfo.contacts)
			{
				Vector3 rhs2 = contactPoint2.point - center;
				rhs2.Normalize();
				float num4 = Vector3.Dot(this._trans.up, rhs2);
				if (num4 < 0.5f)
				{
					this.collFlags |= CollisionFlags.Below;
				}
				else if (num4 > 0.5f)
				{
					this.collFlags |= CollisionFlags.Above;
				}
				else
				{
					this.collFlags |= CollisionFlags.Sides;
				}
			}
			break;
		}
	}

	private void GetCollType()
	{
		this.cColl = null;
		this.sColl = null;
		this.bColl = null;
		Type type = this.coll.GetType();
		if (type == typeof(CapsuleCollider))
		{
			this.collType = 1;
			this.cColl = (CapsuleCollider)this.coll;
		}
		else if (type == typeof(SphereCollider))
		{
			this.sColl = (SphereCollider)this.coll;
			this.collType = 2;
		}
		else if (type == typeof(BoxCollider))
		{
			this.bColl = (BoxCollider)this.coll;
			this.collType = 3;
		}
		else
		{
			this.collType = 0;
		}
	}

	public CollisionFlags collisionFlags
	{
		get
		{
			return this.collFlags;
		}
	}

	private Transform _trans;

	public int collisionCount;

	public float groundAngleVal = 45f;

	public float clampVal;

	public Vector3 slopeNormal;

	private bool testBelow;

	public bool anyPointBelow;

	public float heightOffset = -0.2f;

	public GameObject[] collisionGo;

	private List<GameObject> collCheck = new List<GameObject>();

	private bool inContact;

	private int stuckPointCount;

	public CapsuleCollider cColl;

	private SphereCollider sColl;

	private BoxCollider bColl;

	public Collider coll;

	private CollisionFlags collFlags;

	public int collType = 1;
}
