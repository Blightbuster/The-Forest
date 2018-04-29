using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class FakeParent : MonoBehaviour
	{
		
		private void Awake()
		{
			this.target = base.transform.parent;
			this._pos = base.transform.localPosition;
			this._rot = base.transform.localRotation;
			if (this.disableOnAwake && Scene.ActiveMB)
			{
				Scene.ActiveMB.StartCoroutine(this.DelayedAwake());
			}
			if (base.transform.root.gameObject.CompareTag("PlayerNet") && !base.gameObject.GetComponent<fixWeaponPosition>())
			{
				base.gameObject.AddComponent<fixWeaponPosition>();
			}
		}

		
		private IEnumerator DelayedAwake()
		{
			while (LevelSerializer.IsDeserializing)
			{
				yield return null;
			}
			yield return YieldPresets.WaitForFixedUpdate;
			yield return null;
			if (this && base.gameObject)
			{
				base.gameObject.SetActive(false);
			}
			yield break;
		}

		
		private void OnEnable()
		{
			if (!base.transform.parent)
			{
				this.ReParent();
			}
		}

		
		private void OnDisable()
		{
			if (!base.gameObject.activeSelf && base.transform.parent)
			{
				Scene.ActiveMB.StartCoroutine(this.UnParentRoutine());
			}
		}

		
		private IEnumerator UnParentRoutine()
		{
			yield return null;
			if (!base.gameObject.activeSelf && base.transform.parent)
			{
				this.UnParent();
			}
			yield break;
		}

		
		public void UnParent()
		{
			base.transform.parent = null;
		}

		
		public void ReParent()
		{
			base.transform.parent = this.target;
			base.transform.localPosition = this._pos;
			base.transform.localRotation = this._rot;
		}

		
		
		public Vector3 RealPosition
		{
			get
			{
				if (base.transform.parent)
				{
					return base.transform.position;
				}
				return this.target.TransformPoint(this._pos);
			}
		}

		
		
		public Quaternion RealRotation
		{
			get
			{
				if (base.transform.parent)
				{
					return base.transform.rotation;
				}
				return Quaternion.Inverse(this._rot) * this.target.rotation;
			}
		}

		
		
		public Vector3 RealLocalPosition
		{
			get
			{
				return this._pos;
			}
		}

		
		
		public Quaternion RealLocalRotation
		{
			get
			{
				return this._rot;
			}
		}

		
		public bool disableOnAwake;

		
		public Transform target;

		
		private Vector3 _pos;

		
		private Quaternion _rot;

		
		private float timer = 2f;
	}
}
