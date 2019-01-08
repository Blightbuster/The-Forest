using System;
using UnityEngine;

namespace TheForest.UI
{
	public class ConstructionIcons : MonoBehaviour
	{
		public void Show(bool showManualfillLockIcon, bool canToggleAutofill, bool showAutofillPlace, bool showManualPlace, bool canLock, bool canUnlock, bool showAltPlace = false)
		{
			if (this._toggleAutofill && this._toggleAutofill.activeSelf != canToggleAutofill)
			{
				this._toggleAutofill.SetActive(canToggleAutofill);
			}
			if (this._manualfillLock && this._manualfillLock.activeSelf != showManualfillLockIcon)
			{
				this._manualfillLock.SetActive(showManualfillLockIcon);
			}
			if (this._autofillPlace && this._autofillPlace.activeSelf != showAutofillPlace)
			{
				this._autofillPlace.SetActive(showAutofillPlace);
			}
			if (this._manualfillPlace && this._manualfillPlace.activeSelf != showManualPlace)
			{
				this._manualfillPlace.SetActive(showManualPlace);
			}
			if (this._altPlace && this._altPlace.activeSelf != showAltPlace)
			{
				this._altPlace.SetActive(showAltPlace);
			}
			if (this._lockPoint && this._lockPoint.activeSelf != canLock)
			{
				this._lockPoint.SetActive(canLock);
			}
			if (this._unlockPoint && this._unlockPoint.activeSelf != canUnlock)
			{
				this._unlockPoint.SetActive(canUnlock);
			}
		}

		public void Shutdown()
		{
			if (this._toggleAutofill && this._toggleAutofill.activeSelf)
			{
				this._toggleAutofill.SetActive(false);
			}
			if (this._manualfillLock && this._manualfillLock.activeSelf)
			{
				this._manualfillLock.SetActive(false);
			}
			if (this._autofillPlace && this._autofillPlace.activeSelf)
			{
				this._autofillPlace.SetActive(false);
			}
			if (this._manualfillPlace && this._manualfillPlace.activeSelf)
			{
				this._manualfillPlace.SetActive(false);
			}
			if (this._altPlace && this._altPlace.activeSelf)
			{
				this._altPlace.SetActive(false);
			}
			if (this._lockPoint && this._lockPoint.activeSelf)
			{
				this._lockPoint.SetActive(false);
			}
			if (this._unlockPoint && this._unlockPoint.activeSelf)
			{
				this._unlockPoint.SetActive(false);
			}
		}

		public GameObject _toggleAutofill;

		public GameObject _manualfillLock;

		public GameObject _autofillPlace;

		public GameObject _manualfillPlace;

		public GameObject _altPlace;

		public GameObject _lockPoint;

		public GameObject _unlockPoint;
	}
}
