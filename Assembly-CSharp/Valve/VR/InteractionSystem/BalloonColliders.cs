using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class BalloonColliders : MonoBehaviour
	{
		private void Awake()
		{
			this.rb = base.GetComponent<Rigidbody>();
			this.colliderLocalPositions = new Vector3[this.colliders.Length];
			this.colliderLocalRotations = new Quaternion[this.colliders.Length];
			for (int i = 0; i < this.colliders.Length; i++)
			{
				this.colliderLocalPositions[i] = this.colliders[i].transform.localPosition;
				this.colliderLocalRotations[i] = this.colliders[i].transform.localRotation;
				this.colliders[i].name = base.gameObject.name + "." + this.colliders[i].name;
			}
		}

		private void OnEnable()
		{
			for (int i = 0; i < this.colliders.Length; i++)
			{
				this.colliders[i].transform.SetParent(base.transform);
				this.colliders[i].transform.localPosition = this.colliderLocalPositions[i];
				this.colliders[i].transform.localRotation = this.colliderLocalRotations[i];
				this.colliders[i].transform.SetParent(null);
				FixedJoint fixedJoint = this.colliders[i].AddComponent<FixedJoint>();
				fixedJoint.connectedBody = this.rb;
				fixedJoint.breakForce = float.PositiveInfinity;
				fixedJoint.breakTorque = float.PositiveInfinity;
				fixedJoint.enableCollision = false;
				fixedJoint.enablePreprocessing = true;
				this.colliders[i].SetActive(true);
			}
		}

		private void OnDisable()
		{
			for (int i = 0; i < this.colliders.Length; i++)
			{
				if (this.colliders[i] != null)
				{
					UnityEngine.Object.Destroy(this.colliders[i].GetComponent<FixedJoint>());
					this.colliders[i].SetActive(false);
				}
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < this.colliders.Length; i++)
			{
				UnityEngine.Object.Destroy(this.colliders[i]);
			}
		}

		public GameObject[] colliders;

		private Vector3[] colliderLocalPositions;

		private Quaternion[] colliderLocalRotations;

		private Rigidbody rb;
	}
}
