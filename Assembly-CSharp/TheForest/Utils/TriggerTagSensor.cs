using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class TriggerTagSensor : MonoBehaviour
	{
		public string TargetTag { get; set; }

		public TriggerTagSensor.ITarget Target { get; set; }

		private void OnTriggerEnter(Collider other)
		{
			if (this.Target != null && other.gameObject.CompareTag(this.TargetTag))
			{
				this.Target.OnTargetTagTrigerEnter(other);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (this.Target != null && other.gameObject.CompareTag(this.TargetTag))
			{
				this.Target.OnTargetTagTrigerExit(other);
			}
		}

		public interface ITarget
		{
			void OnTargetTagTrigerEnter(Collider other);

			void OnTargetTagTrigerExit(Collider other);
		}
	}
}
