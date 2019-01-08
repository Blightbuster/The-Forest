using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
using UnityEngine;

namespace TheForest.Utils.Creation
{
	public class ManualSwap : MonoBehaviour
	{
		private IEnumerator Start()
		{
			base.enabled = false;
			yield return null;
			this._renderers = new Renderer[this._ghosts.Length];
			for (int i = 0; i < this._ghosts.Length; i++)
			{
				GameObject go = this._ghosts[i]._go;
				GameObject gameObject = go.transform.GetChild(0).Find("Trigger").gameObject;
				gameObject.SetActive(false);
				this._renderers[i] = go.transform.GetChild(0).GetComponent<Renderer>();
				this._ghosts[i]._go.SetActive(this._currentGhost == i);
			}
			Scene.HudGui.ToggleVariationIcon.SetActive(true);
			base.enabled = true;
			yield break;
		}

		private void Update()
		{
			if (Input.GetButtonDown("Craft"))
			{
				LocalPlayer.Sfx.PlayWhoosh();
				this._ghosts[this._currentGhost]._go.SetActive(false);
				this._currentGhost = (this._currentGhost + 1) % this._ghosts.Length;
				this._ghosts[this._currentGhost]._go.SetActive(true);
				LocalPlayer.Create.BuildingPlacer.SetRenderer(this._renderers[this._currentGhost]);
			}
		}

		private void OnDestroy()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.ToggleVariationIcon.SetActive(false);
			}
		}

		private void OnPlaced()
		{
			base.enabled = false;
			Transform child = this._ghosts[this._currentGhost]._go.transform.GetChild(0);
			if (!BoltNetwork.isRunning)
			{
				if (LocalPlayer.Create.BuildingPlacer.LastHit != null && LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>())
				{
					child.parent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>().transform;
				}
				else
				{
					child.parent = null;
				}
				child.SendMessage("OnPlaced", SendMessageOptions.DontRequireReceiver);
				GameObject gameObject = child.Find("Trigger").gameObject;
				gameObject.SetActive(true);
				if (this._ghosts[this._currentGhost]._initialize)
				{
					gameObject.GetComponent<Craft_Structure>().Initialize();
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				CoopConstructionEx component = child.GetComponent<CoopConstructionEx>();
				if (component)
				{
					BoltEntity component2 = child.GetComponent<BoltEntity>();
					BoltEntity parentEntity = LocalPlayer.Create.GetParentEntity(child.gameObject);
					component.SendMessage("OnSerializing");
					CoopConstructionExToken coopConstructionExToken = LocalPlayer.Create.GetCoopConstructionExToken(component, parentEntity);
					PlaceFoundationEx placeFoundationEx = PlaceFoundationEx.Create(GlobalTargets.OnlyServer);
					placeFoundationEx.Parent = parentEntity;
					placeFoundationEx.Position = child.transform.position;
					placeFoundationEx.Prefab = component2.prefabId;
					placeFoundationEx.Token = coopConstructionExToken;
					placeFoundationEx.Send();
				}
				else
				{
					PlaceConstruction placeConstruction = PlaceConstruction.Create(GlobalTargets.OnlyServer);
					if (LocalPlayer.Create.BuildingPlacer.LastHit != null)
					{
						placeConstruction.Parent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>();
					}
					placeConstruction.PrefabId = child.GetComponent<BoltEntity>().prefabId;
					placeConstruction.Position = child.position;
					placeConstruction.Rotation = child.rotation;
					FoundationArchitect component3 = child.GetComponent<FoundationArchitect>();
					if (component3)
					{
						placeConstruction.AboveGround = component3._aboveGround;
					}
					placeConstruction.Send();
				}
				UnityEngine.Object.Destroy(base.gameObject, 0.05f);
			}
		}

		private void OnDeserialized()
		{
			base.enabled = false;
		}

		public ManualSwap.GhostInfo[] _ghosts;

		private Renderer[] _renderers;

		private int _currentGhost;

		[Serializable]
		public class GhostInfo
		{
			public GameObject _go;

			public bool _initialize;
		}
	}
}
