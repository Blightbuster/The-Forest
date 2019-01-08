using System;
using System.Collections;
using UnityEngine;

public class OVRGazePointer : MonoBehaviour
{
	public bool hidden { get; private set; }

	public float currentScale { get; private set; }

	public Vector3 positionDelta { get; private set; }

	public static OVRGazePointer instance
	{
		get
		{
			if (OVRGazePointer._instance == null)
			{
				Debug.Log(string.Format("Instanciating GazePointer", 0));
				OVRGazePointer._instance = UnityEngine.Object.Instantiate<OVRGazePointer>((OVRGazePointer)Resources.Load("Prefabs/GazePointerRing", typeof(OVRGazePointer)));
			}
			return OVRGazePointer._instance;
		}
	}

	public float visibilityStrength
	{
		get
		{
			float a;
			if (this.hideByDefault)
			{
				a = Mathf.Clamp01(1f - (Time.time - this.lastShowRequestTime) / this.showTimeoutPeriod);
			}
			else
			{
				a = 1f;
			}
			float b = (this.lastHideRequestTime + this.hideTimeoutPeriod <= Time.time) ? 1f : ((!this.dimOnHideRequest) ? 0f : 0.1f);
			return Mathf.Min(a, b);
		}
	}

	public float SelectionProgress
	{
		get
		{
			return (!this.progressIndicator) ? 0f : this.progressIndicator.currentProgress;
		}
		set
		{
			if (this.progressIndicator)
			{
				this.progressIndicator.currentProgress = value;
			}
		}
	}

	public void Awake()
	{
		this.currentScale = 1f;
		if (OVRGazePointer._instance != null && OVRGazePointer._instance != this)
		{
			base.enabled = false;
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		OVRGazePointer._instance = this;
		this.trailFollower = base.transform.Find("TrailFollower");
		this.progressIndicator = base.transform.GetComponent<OVRProgressIndicator>();
	}

	private void Update()
	{
		if (this.rayTransform == null && Camera.main != null)
		{
			this.rayTransform = Camera.main.transform;
		}
		base.transform.position = this.rayTransform.position + this.rayTransform.forward * this.depth;
		if (this.visibilityStrength == 0f && !this.hidden)
		{
			this.Hide();
		}
		else if (this.visibilityStrength > 0f && this.hidden)
		{
			this.Show();
		}
	}

	public void SetPosition(Vector3 pos, Vector3 normal)
	{
		base.transform.position = pos;
		Quaternion rotation = base.transform.rotation;
		rotation.SetLookRotation(normal, this.rayTransform.up);
		base.transform.rotation = rotation;
		this.depth = (this.rayTransform.position - pos).magnitude;
		this.currentScale = this.depth * this.depthScaleMultiplier;
		base.transform.localScale = new Vector3(this.currentScale, this.currentScale, this.currentScale);
		this.positionSetsThisFrame++;
	}

	public void SetPosition(Vector3 pos)
	{
		this.SetPosition(pos, this.rayTransform.forward);
	}

	public float GetCurrentRadius()
	{
		return this.cursorRadius * this.currentScale;
	}

	private void LateUpdate()
	{
		if (this.positionSetsThisFrame == 0)
		{
			Quaternion rotation = base.transform.rotation;
			rotation.SetLookRotation(this.rayTransform.forward, this.rayTransform.up);
			base.transform.rotation = rotation;
		}
		Quaternion rotation2 = this.trailFollower.rotation;
		rotation2.SetLookRotation(base.transform.rotation * new Vector3(0f, 0f, 1f), (this.lastPosition - base.transform.position).normalized);
		this.trailFollower.rotation = rotation2;
		this.positionDelta = base.transform.position - this.lastPosition;
		this.lastPosition = base.transform.position;
		this.positionSetsThisFrame = 0;
	}

	public void RequestHide()
	{
		if (!this.dimOnHideRequest)
		{
			this.Hide();
		}
		this.lastHideRequestTime = Time.time;
	}

	public void RequestShow()
	{
		this.Show();
		this.lastShowRequestTime = Time.time;
	}

	private void Hide()
	{
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				transform.gameObject.SetActive(false);
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
		if (base.GetComponent<Renderer>())
		{
			base.GetComponent<Renderer>().enabled = false;
		}
		this.hidden = true;
	}

	private void Show()
	{
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				transform.gameObject.SetActive(true);
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
		if (base.GetComponent<Renderer>())
		{
			base.GetComponent<Renderer>().enabled = true;
		}
		this.hidden = false;
	}

	private Transform trailFollower;

	[Tooltip("Should the pointer be hidden when not over interactive objects.")]
	public bool hideByDefault = true;

	[Tooltip("Time after leaving interactive object before pointer fades.")]
	public float showTimeoutPeriod = 1f;

	[Tooltip("Time after mouse pointer becoming inactive before pointer unfades.")]
	public float hideTimeoutPeriod = 0.1f;

	[Tooltip("Keep a faint version of the pointer visible while using a mouse")]
	public bool dimOnHideRequest = true;

	[Tooltip("Angular scale of pointer")]
	public float depthScaleMultiplier = 0.03f;

	public Transform rayTransform;

	private float depth;

	private float hideUntilTime;

	private int positionSetsThisFrame;

	private Vector3 lastPosition;

	private float lastShowRequestTime;

	private float lastHideRequestTime;

	[Tooltip("Radius of the cursor. Used for preventing geometry intersections.")]
	public float cursorRadius = 1f;

	private OVRProgressIndicator progressIndicator;

	private static OVRGazePointer _instance;
}
