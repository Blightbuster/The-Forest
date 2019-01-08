using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class ItemPackage : MonoBehaviour
	{
		public new string name;

		public ItemPackage.ItemPackageType packageType;

		public GameObject itemPrefab;

		public GameObject otherHandItemPrefab;

		public GameObject previewPrefab;

		public GameObject fadedPreviewPrefab;

		public enum ItemPackageType
		{
			Unrestricted,
			OneHanded,
			TwoHanded
		}
	}
}
