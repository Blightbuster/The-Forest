using System;
using UnityEngine;

namespace Pathfinding
{
	[Serializable]
	public abstract class MonoModifier : MonoBehaviour, IPathModifier
	{
		public void OnEnable()
		{
		}

		public void OnDisable()
		{
		}

		public abstract int Order { get; }

		public void Awake()
		{
			this.seeker = base.GetComponent<Seeker>();
			if (this.seeker != null)
			{
				this.seeker.RegisterModifier(this);
			}
		}

		public void OnDestroy()
		{
			if (this.seeker != null)
			{
				this.seeker.DeregisterModifier(this);
			}
		}

		public virtual void PreProcess(Path p)
		{
		}

		public abstract void Apply(Path p);

		[NonSerialized]
		public Seeker seeker;
	}
}
