using System;
using UnityEngine;

public class OVRGrabbable : MonoBehaviour
{
	public bool allowOffhandGrab
	{
		get
		{
			return this.m_allowOffhandGrab;
		}
	}

	public bool isGrabbed
	{
		get
		{
			return this.m_grabbedBy != null;
		}
	}

	public bool snapPosition
	{
		get
		{
			return this.m_snapPosition;
		}
	}

	public bool snapOrientation
	{
		get
		{
			return this.m_snapOrientation;
		}
	}

	public Transform snapOffset
	{
		get
		{
			return this.m_snapOffset;
		}
	}

	public OVRGrabber grabbedBy
	{
		get
		{
			return this.m_grabbedBy;
		}
	}

	public Transform grabbedTransform
	{
		get
		{
			return this.m_grabbedCollider.transform;
		}
	}

	public Rigidbody grabbedRigidbody
	{
		get
		{
			return this.m_grabbedCollider.attachedRigidbody;
		}
	}

	public Collider[] grabPoints
	{
		get
		{
			return this.m_grabPoints;
		}
	}

	public virtual void GrabBegin(OVRGrabber hand, Collider grabPoint)
	{
		this.m_grabbedBy = hand;
		this.m_grabbedCollider = grabPoint;
		base.gameObject.GetComponent<Rigidbody>().isKinematic = true;
	}

	public virtual void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
	{
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		component.isKinematic = this.m_grabbedKinematic;
		component.velocity = linearVelocity;
		component.angularVelocity = angularVelocity;
		this.m_grabbedBy = null;
		this.m_grabbedCollider = null;
	}

	private void Awake()
	{
		if (this.m_grabPoints.Length == 0)
		{
			Collider component = base.GetComponent<Collider>();
			if (component == null)
			{
				throw new ArgumentException("Grabbables cannot have zero grab points and no collider -- please add a grab point or collider.");
			}
			this.m_grabPoints = new Collider[]
			{
				component
			};
		}
	}

	protected virtual void Start()
	{
		this.m_grabbedKinematic = base.GetComponent<Rigidbody>().isKinematic;
	}

	private void OnDestroy()
	{
		if (this.m_grabbedBy != null)
		{
			this.m_grabbedBy.ForceRelease(this);
		}
	}

	[SerializeField]
	protected bool m_allowOffhandGrab = true;

	[SerializeField]
	protected bool m_snapPosition;

	[SerializeField]
	protected bool m_snapOrientation;

	[SerializeField]
	protected Transform m_snapOffset;

	[SerializeField]
	protected Collider[] m_grabPoints;

	protected bool m_grabbedKinematic;

	protected Collider m_grabbedCollider;

	protected OVRGrabber m_grabbedBy;
}
