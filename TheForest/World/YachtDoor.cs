using System;
using System.Collections;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.World
{
	
	public class YachtDoor : MonoBehaviour
	{
		
		private void Awake()
		{
			if (LevelSerializer.IsDeserializing)
			{
				base.StartCoroutine(this.DelayedAwake());
			}
			else if (BoltNetwork.isClient)
			{
				base.StartCoroutine(this.DelayedAwakeClient());
			}
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.CompareTag("Player"))
			{
				this.ToggleInside();
			}
		}

		
		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.CompareTag("Player"))
			{
				this.Toggle();
			}
		}

		
		private IEnumerator DelayedAwake()
		{
			while (LevelSerializer.IsDeserializing)
			{
				yield return null;
			}
			if (LocalPlayer.Transform && this._mainRenderer.bounds.Contains(LocalPlayer.Transform.position))
			{
				this.ToggleInside();
			}
			else
			{
				Scene.ShoreMask.clipMask = this._outsideClip;
				this._flatOceanMat.SetTexture("TerrainFlowHeightmap", this._flatOceanOutsideFlowmap);
			}
			yield break;
		}

		
		private IEnumerator DelayedAwakeClient()
		{
			yield return null;
			while (!LocalPlayer.Transform)
			{
				yield return null;
			}
			if (LocalPlayer.Transform && this._mainRenderer.bounds.Contains(LocalPlayer.Transform.position))
			{
				this.ToggleInside();
			}
			else
			{
				Scene.ShoreMask.clipMask = this._outsideClip;
				this._flatOceanMat.SetTexture("TerrainFlowHeightmap", this._flatOceanOutsideFlowmap);
			}
			yield break;
		}

		
		private void Update()
		{
			if (this._insideCheck)
			{
				this._insideDistance = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
				if (this._insideDistance > 22f)
				{
					this.ToggleOutside();
				}
			}
		}

		
		private void Toggle()
		{
			if (base.transform.InverseTransformPoint(LocalPlayer.Transform.position).z < 0f)
			{
				this.ToggleInside();
			}
			else
			{
				this.ToggleOutside();
			}
		}

		
		private void ToggleInside()
		{
			this._insideCheck = true;
			Scene.ShoreMask.clipMask = this._insideClip;
			this._flatOceanMat.SetTexture("TerrainFlowHeightmap", this._flatOceanInsideFlowmap);
			LocalPlayer.Buoyancy.enabled = false;
			LocalPlayer.Transform.SendMessage("setInYacht", true);
			if (this._snapshotInstance == null)
			{
				this._snapshotInstance = FMODCommon.PlayOneshot("snapshot:/Inside Yacht", Vector3.zero, new object[0]);
			}
		}

		
		private void ToggleOutside()
		{
			this._insideCheck = false;
			Scene.ShoreMask.clipMask = this._outsideClip;
			this._flatOceanMat.SetTexture("TerrainFlowHeightmap", this._flatOceanOutsideFlowmap);
			LocalPlayer.Buoyancy.enabled = true;
			LocalPlayer.Transform.SendMessage("setInYacht", false);
			if (this._snapshotInstance != null)
			{
				UnityUtil.ERRCHECK(this._snapshotInstance.stop(STOP_MODE.ALLOWFADEOUT));
				this._snapshotInstance = null;
			}
		}

		
		public Renderer _mainRenderer;

		
		public Texture _outsideClip;

		
		public Texture _insideClip;

		
		public Texture _flatOceanOutsideFlowmap;

		
		public Texture _flatOceanInsideFlowmap;

		
		public Material _flatOceanMat;

		
		public float _insideDistance;

		
		private EventInstance _snapshotInstance;

		
		private bool _insideCheck;
	}
}
