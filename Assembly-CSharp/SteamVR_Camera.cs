using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.VR;

[RequireComponent(typeof(Camera))]
public class SteamVR_Camera : MonoBehaviour
{
	public Transform head
	{
		get
		{
			return this._head;
		}
	}

	public Transform offset
	{
		get
		{
			return this._head;
		}
	}

	public Transform origin
	{
		get
		{
			return this._head.parent;
		}
	}

	public Camera camera { get; private set; }

	public Transform ears
	{
		get
		{
			return this._ears;
		}
	}

	public Ray GetRay()
	{
		return new Ray(this._head.position, this._head.forward);
	}

	public static float sceneResolutionScale
	{
		get
		{
			return VRSettings.renderScale;
		}
		set
		{
			VRSettings.renderScale = value;
		}
	}

	private void OnDisable()
	{
		SteamVR_Render.Remove(this);
	}

	private void OnEnable()
	{
		if (SteamVR.instance == null)
		{
			if (this.head != null)
			{
				this.head.GetComponent<SteamVR_TrackedObject>().enabled = false;
			}
			base.enabled = false;
			return;
		}
		Transform transform = base.transform;
		if (this.head != transform)
		{
			this.Expand();
			transform.parent = this.origin;
			while (this.head.childCount > 0)
			{
				this.head.GetChild(0).parent = transform;
			}
			this.head.parent = transform;
			this.head.localPosition = Vector3.zero;
			this.head.localRotation = Quaternion.identity;
			this.head.localScale = Vector3.one;
			this.head.gameObject.SetActive(false);
			this._head = transform;
		}
		if (this.ears == null)
		{
			SteamVR_Ears componentInChildren = base.transform.GetComponentInChildren<SteamVR_Ears>();
			if (componentInChildren != null)
			{
				this._ears = componentInChildren.transform;
			}
		}
		if (this.ears != null)
		{
			this.ears.GetComponent<SteamVR_Ears>().vrcam = this;
		}
		SteamVR_Render.Add(this);
	}

	private void Awake()
	{
		this.camera = base.GetComponent<Camera>();
		this.ForceLast();
	}

	public void ForceLast()
	{
		if (SteamVR_Camera.values != null)
		{
			IDictionaryEnumerator enumerator = SteamVR_Camera.values.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
					FieldInfo fieldInfo = dictionaryEntry.Key as FieldInfo;
					fieldInfo.SetValue(this, dictionaryEntry.Value);
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
			SteamVR_Camera.values = null;
		}
		else
		{
			Component[] components = base.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				SteamVR_Camera steamVR_Camera = components[i] as SteamVR_Camera;
				if (steamVR_Camera != null && steamVR_Camera != this)
				{
					UnityEngine.Object.DestroyImmediate(steamVR_Camera);
				}
			}
			components = base.GetComponents<Component>();
			if (this != components[components.Length - 1])
			{
				SteamVR_Camera.values = new Hashtable();
				FieldInfo[] fields = base.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo2 in fields)
				{
					if (fieldInfo2.IsPublic || fieldInfo2.IsDefined(typeof(SerializeField), true))
					{
						SteamVR_Camera.values[fieldInfo2] = fieldInfo2.GetValue(this);
					}
				}
				GameObject gameObject = base.gameObject;
				UnityEngine.Object.DestroyImmediate(this);
				gameObject.AddComponent<SteamVR_Camera>().ForceLast();
			}
		}
	}

	public string baseName
	{
		get
		{
			return (!base.name.EndsWith(" (eye)")) ? base.name : base.name.Substring(0, base.name.Length - " (eye)".Length);
		}
	}

	public void Expand()
	{
		Transform transform = base.transform.parent;
		if (transform == null)
		{
			transform = new GameObject(base.name + " (origin)").transform;
			transform.localPosition = base.transform.localPosition;
			transform.localRotation = base.transform.localRotation;
			transform.localScale = base.transform.localScale;
		}
		if (this.head == null)
		{
			this._head = new GameObject(base.name + " (head)", new Type[]
			{
				typeof(SteamVR_TrackedObject)
			}).transform;
			this.head.parent = transform;
			this.head.position = base.transform.position;
			this.head.rotation = base.transform.rotation;
			this.head.localScale = Vector3.one;
			this.head.tag = base.tag;
		}
		if (base.transform.parent != this.head)
		{
			base.transform.parent = this.head;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.transform.localScale = Vector3.one;
			while (base.transform.childCount > 0)
			{
				base.transform.GetChild(0).parent = this.head;
			}
			GUILayer component = base.GetComponent<GUILayer>();
			if (component != null)
			{
				UnityEngine.Object.DestroyImmediate(component);
				this.head.gameObject.AddComponent<GUILayer>();
			}
			AudioListener component2 = base.GetComponent<AudioListener>();
			if (component2 != null)
			{
				UnityEngine.Object.DestroyImmediate(component2);
				this._ears = new GameObject(base.name + " (ears)", new Type[]
				{
					typeof(SteamVR_Ears)
				}).transform;
				this.ears.parent = this._head;
				this.ears.localPosition = Vector3.zero;
				this.ears.localRotation = Quaternion.identity;
				this.ears.localScale = Vector3.one;
			}
		}
		if (!base.name.EndsWith(" (eye)"))
		{
			base.name += " (eye)";
		}
	}

	public void Collapse()
	{
		base.transform.parent = null;
		while (this.head.childCount > 0)
		{
			this.head.GetChild(0).parent = base.transform;
		}
		GUILayer component = this.head.GetComponent<GUILayer>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
			base.gameObject.AddComponent<GUILayer>();
		}
		if (this.ears != null)
		{
			while (this.ears.childCount > 0)
			{
				this.ears.GetChild(0).parent = base.transform;
			}
			UnityEngine.Object.DestroyImmediate(this.ears.gameObject);
			this._ears = null;
			base.gameObject.AddComponent(typeof(AudioListener));
		}
		if (this.origin != null)
		{
			if (this.origin.name.EndsWith(" (origin)"))
			{
				Transform origin = this.origin;
				while (origin.childCount > 0)
				{
					origin.GetChild(0).parent = origin.parent;
				}
				UnityEngine.Object.DestroyImmediate(origin.gameObject);
			}
			else
			{
				base.transform.parent = this.origin;
			}
		}
		UnityEngine.Object.DestroyImmediate(this.head.gameObject);
		this._head = null;
		if (base.name.EndsWith(" (eye)"))
		{
			base.name = base.name.Substring(0, base.name.Length - " (eye)".Length);
		}
	}

	[SerializeField]
	private Transform _head;

	[SerializeField]
	private Transform _ears;

	public bool wireframe;

	private static Hashtable values;

	private const string eyeSuffix = " (eye)";

	private const string earsSuffix = " (ears)";

	private const string headSuffix = " (head)";

	private const string originSuffix = " (origin)";
}
