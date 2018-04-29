using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/Creation/Anchorable Structure")]
	public class SingleAnchorStructure : MonoBehaviour, IAnchorValidation
	{
		
		private void Awake()
		{
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		
		private IEnumerator DelayedAwake(bool isDeserializing)
		{
			yield return null;
			yield return null;
			if (BoltNetwork.isClient && base.GetComponent<BoltEntity>().isAttached)
			{
				this.OnDeserialized();
				yield break;
			}
			if (!this._wasBuilt && !this._wasPlaced)
			{
				while (!LocalPlayer.Create)
				{
					yield return null;
				}
				LocalPlayer.Create.Grabber.ClosePlace();
				if (this._renderer)
				{
					this._renderer.sharedMaterial = LocalPlayer.Create.BuildingPlacer.RedMat;
				}
				base.enabled = true;
			}
			else
			{
				if (this._wasPlaced && base.GetComponent<Collider>())
				{
					base.GetComponent<Collider>().enabled = false;
				}
				base.enabled = false;
			}
			yield break;
		}

		
		private void Update()
		{
			if (!base.transform.parent && Vector3.Distance(LocalPlayer.Create.BuildingPlacer.transform.position, base.transform.position) > 3.75f)
			{
				base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
				if (this._anchor1._hookedInStructure == this)
				{
					this._anchor1._hookedInStructure = null;
				}
				this._anchor1 = null;
			}
			this._caster.CastForAnchors<StructureAnchor>(new Action<StructureAnchor>(this.CheckLockAnchor));
			bool flag = this._anchor1;
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag || Scene.HudGui.PlaceIcon.activeSelf != flag || Scene.HudGui.RotateIcon.activeSelf == flag)
			{
				Scene.HudGui.PlaceIcon.SetActive(flag);
				Scene.HudGui.RotateIcon.SetActive(!flag);
				LocalPlayer.Create.BuildingPlacer.Clear = flag;
			}
			if (this._renderer)
			{
				this._renderer.sharedMaterial = ((!flag) ? LocalPlayer.Create.BuildingPlacer.RedMat : LocalPlayer.Create.BuildingPlacer.ClearMat);
			}
		}

		
		private void OnDestroy()
		{
			if (this._anchor1 && this._anchor1._hookedInStructure == this)
			{
				this._anchor1._hookedInStructure = null;
			}
		}

		
		private void OnDeserialized()
		{
			this.OnPlaced();
		}

		
		private void OnPlaced()
		{
			if (this._destroyWhenPlaced)
			{
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				this._wasPlaced = true;
				base.GetComponent<Collider>().enabled = false;
				base.enabled = false;
			}
		}

		
		private void CheckLockAnchor(StructureAnchor anchor)
		{
			if (anchor && anchor._hookedInStructure == null && anchor != this._anchor1 && this.ValidateAnchor(anchor.transform))
			{
				base.transform.parent = null;
				base.transform.position = anchor.transform.TransformPoint(anchor._upperPositionOffset);
				base.transform.rotation = anchor.transform.rotation;
				this._anchor1 = anchor;
			}
		}

		
		public bool ValidateAnchor(Transform anchor)
		{
			RaycastHit raycastHit;
			return !Physics.Raycast(anchor.position + anchor.forward, Vector3.down, out raycastHit, this._minHeight, this._floorLayers);
		}

		
		
		
		public StructureAnchor Anchor1
		{
			get
			{
				return this._anchor1;
			}
			set
			{
				this._anchor1 = value;
			}
		}

		
		public void AnchorDestroyed(StructureAnchor anchor)
		{
			if (base.gameObject)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		public SpherecastAnchoring _caster;

		
		public LayerMask _floorLayers;

		
		public float _minHeight = 10f;

		
		[SerializeThis]
		public bool _wasPlaced;

		
		public bool _wasBuilt;

		
		public bool _destroyWhenPlaced;

		
		public Renderer _renderer;

		
		[SerializeThis]
		private StructureAnchor _anchor1;
	}
}
