using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForets.Utils
{
	public class GameObjectAutoInstancer : MonoBehaviour
	{
		public void OnEnable()
		{
			GameObjectAutoInstancer.DoAutoInstance(this.InstanceSource, this.GameObjectTarget ?? base.gameObject, this.Replace, this.CopyScale, this.InstanceName);
		}

		private static void DoAutoInstance(GameObject instanceSource, GameObject gameObjectTarget, bool replace, bool copyScale, string instanceName)
		{
			if (instanceSource == null || gameObjectTarget == null)
			{
				return;
			}
			Transform transform = gameObjectTarget.transform;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(instanceSource, transform.position, transform.rotation, transform.parent);
			Transform transform2 = gameObject.transform;
			if (copyScale)
			{
				transform2.localScale = transform.localScale;
			}
			if (replace)
			{
				GameObjectAutoInstancer.ReplaceTransform(transform, transform2);
			}
			if (!instanceName.NullOrEmpty())
			{
				gameObject.name = instanceName;
			}
		}

		private static void ReplaceTransform(Transform oldTransform, Transform newTransform)
		{
			IEnumerator enumerator = oldTransform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.parent = newTransform;
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
			oldTransform.gameObject.SetActive(false);
			UnityEngine.Object.Destroy(oldTransform.gameObject);
		}

		public GameObject InstanceSource;

		public GameObject GameObjectTarget;

		public string InstanceName;

		public bool Replace = true;

		public bool CopyScale = true;
	}
}
