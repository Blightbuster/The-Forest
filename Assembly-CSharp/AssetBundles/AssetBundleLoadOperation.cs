using System;
using System.Collections;

namespace AssetBundles
{
	public abstract class AssetBundleLoadOperation : IEnumerator
	{
		public object Current
		{
			get
			{
				return null;
			}
		}

		public bool MoveNext()
		{
			return !this.IsDone() && !this.aborted;
		}

		public void Reset()
		{
		}

		public abstract bool Update();

		public abstract bool IsDone();

		public abstract bool MatchBundle(string assetBundleName);

		public void Abort()
		{
			this.aborted = true;
		}

		private bool aborted;
	}
}
