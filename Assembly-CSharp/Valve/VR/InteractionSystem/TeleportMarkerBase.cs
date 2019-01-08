using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public abstract class TeleportMarkerBase : MonoBehaviour
	{
		public virtual bool showReticle
		{
			get
			{
				return true;
			}
		}

		public void SetLocked(bool locked)
		{
			this.locked = locked;
			this.UpdateVisuals();
		}

		public virtual void TeleportPlayer(Vector3 pointedAtPosition)
		{
		}

		public abstract void UpdateVisuals();

		public abstract void Highlight(bool highlight);

		public abstract void SetAlpha(float tintAlpha, float alphaPercent);

		public abstract bool ShouldActivate(Vector3 playerPosition);

		public abstract bool ShouldMovePlayer();

		public bool locked;

		public bool markerActive = true;
	}
}
