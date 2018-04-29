using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class GeoHashUnique : MonoBehaviour
	{
		
		public void Start()
		{
			GeoHashUnique fromHash = GeoHashHelper<GeoHashUnique>.GetFromHash(base.transform.ToGeoHash(), Lookup.ManualOnly);
			if (fromHash && fromHash != this)
			{
				if (!this._keepLatest)
				{
					Debug.Log(string.Format("[GeoHash] Older: Switching \"{0}\" -> \"{1}\"", GeoHashUnique.GetFullPath(base.transform.gameObject), GeoHashUnique.GetFullPath(fromHash.gameObject)));
					if (this._adoptParent)
					{
						this.AdoptParent(base.gameObject.transform, fromHash.transform);
					}
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				Debug.Log(string.Format("[GeoHash] Latest: Switching \"{0}\" -> \"{1}\"", GeoHashUnique.GetFullPath(fromHash.gameObject), GeoHashUnique.GetFullPath(base.transform.gameObject)));
				if (this._adoptParent)
				{
					this.AdoptParent(fromHash.transform, base.gameObject.transform);
				}
				UnityEngine.Object.Destroy(fromHash.gameObject);
			}
			else
			{
				Debug.Log(string.Format("[GeoHash] First: \"{0}\"", GeoHashUnique.GetFullPath(base.transform.gameObject)));
			}
			GeoHashHelper<GeoHashUnique>.Register(this);
		}

		
		private void AdoptParent(Transform replaceTarget, Transform replaceSource)
		{
			Transform parent = replaceTarget.parent;
			if (replaceSource.parent != parent)
			{
				replaceSource.parent = replaceTarget;
				replaceSource.parent = parent;
			}
		}

		
		private static string GetFullPath(GameObject existingGameObject)
		{
			return string.Format(":{0}:{1}", existingGameObject.scene.name, existingGameObject.GetFullName());
		}

		
		private void OnDestroy()
		{
			GeoHashHelper<GeoHashUnique>.Unregister(this);
		}

		
		public bool _keepLatest;

		
		public bool _adoptParent;
	}
}
