using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	[RequireComponent(typeof(Interactable))]
	public class LinearDrive : MonoBehaviour
	{
		
		private void Awake()
		{
			this.mappingChangeSamples = new float[this.numMappingChangeSamples];
		}

		
		private void Start()
		{
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
			if (this.linearMapping == null)
			{
				this.linearMapping = base.gameObject.AddComponent<LinearMapping>();
			}
			this.initialMappingOffset = this.linearMapping.value;
			if (this.repositionGameObject)
			{
				this.UpdateLinearMapping(base.transform);
			}
		}

		
		private void HandHoverUpdate(Hand hand)
		{
			if (hand.GetStandardInteractionButtonDown())
			{
				hand.HoverLock(base.GetComponent<Interactable>());
				this.initialMappingOffset = this.linearMapping.value - this.CalculateLinearMapping(hand.transform);
				this.sampleCount = 0;
				this.mappingChangeRate = 0f;
			}
			if (hand.GetStandardInteractionButtonUp())
			{
				hand.HoverUnlock(base.GetComponent<Interactable>());
				this.CalculateMappingChangeRate();
			}
			if (hand.GetStandardInteractionButton())
			{
				this.UpdateLinearMapping(hand.transform);
			}
		}

		
		private void CalculateMappingChangeRate()
		{
			this.mappingChangeRate = 0f;
			int num = Mathf.Min(this.sampleCount, this.mappingChangeSamples.Length);
			if (num != 0)
			{
				for (int i = 0; i < num; i++)
				{
					this.mappingChangeRate += this.mappingChangeSamples[i];
				}
				this.mappingChangeRate /= (float)num;
			}
		}

		
		private void UpdateLinearMapping(Transform tr)
		{
			this.prevMapping = this.linearMapping.value;
			this.linearMapping.value = Mathf.Clamp01(this.initialMappingOffset + this.CalculateLinearMapping(tr));
			this.mappingChangeSamples[this.sampleCount % this.mappingChangeSamples.Length] = 1f / Time.deltaTime * (this.linearMapping.value - this.prevMapping);
			this.sampleCount++;
			if (this.repositionGameObject)
			{
				base.transform.position = Vector3.Lerp(this.startPosition.position, this.endPosition.position, this.linearMapping.value);
			}
		}

		
		private float CalculateLinearMapping(Transform tr)
		{
			Vector3 rhs = this.endPosition.position - this.startPosition.position;
			float magnitude = rhs.magnitude;
			rhs.Normalize();
			Vector3 lhs = tr.position - this.startPosition.position;
			return Vector3.Dot(lhs, rhs) / magnitude;
		}

		
		private void Update()
		{
			if (this.maintainMomemntum && this.mappingChangeRate != 0f)
			{
				this.mappingChangeRate = Mathf.Lerp(this.mappingChangeRate, 0f, this.momemtumDampenRate * Time.deltaTime);
				this.linearMapping.value = Mathf.Clamp01(this.linearMapping.value + this.mappingChangeRate * Time.deltaTime);
				if (this.repositionGameObject)
				{
					base.transform.position = Vector3.Lerp(this.startPosition.position, this.endPosition.position, this.linearMapping.value);
				}
			}
		}

		
		public Transform startPosition;

		
		public Transform endPosition;

		
		public LinearMapping linearMapping;

		
		public bool repositionGameObject = true;

		
		public bool maintainMomemntum = true;

		
		public float momemtumDampenRate = 5f;

		
		private float initialMappingOffset;

		
		private int numMappingChangeSamples = 5;

		
		private float[] mappingChangeSamples;

		
		private float prevMapping;

		
		private float mappingChangeRate;

		
		private int sampleCount;
	}
}
