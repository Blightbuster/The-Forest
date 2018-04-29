using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class RaftOarSwap : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			base.enabled = false;
			this._offsetWithPlacer = base.transform.localPosition;
			yield return null;
			GameObject trigger = this._raftGo.transform.GetChild(0).Find("Trigger").gameObject;
			trigger.SetActive(false);
			GameObject trigger2 = this._oarGo.transform.GetChild(0).Find("Trigger").gameObject;
			trigger2.SetActive(false);
			yield return null;
			this._oarGo.SetActive(false);
			this._raftGo.SetActive(false);
			this._oarRenderer = this._oarGo.transform.GetChild(0).GetComponent<Renderer>();
			this._raftRenderer = this._raftGo.transform.GetChild(0).GetComponent<Renderer>();
			this._raft = this._raftGo.transform.GetChild(0).GetComponent<RaftArchitect>();
			base.enabled = true;
			yield break;
		}

		
		private void Update()
		{
			if (this._raft.MultiPointsPositions.Count == 0)
			{
				RaycastHit raycastHit;
				if (Physics.SphereCast(((this._origin != RaftOarSwap.RayCastOrigins.Player) ? LocalPlayer.Create.BuildingPlacer.transform.position : LocalPlayer.MainCamTr.position) + Vector3.up * 3f, 2f, Vector3.down, out raycastHit, this._raycastDistance, this._layers.value))
				{
					PrefabIdentifier componentInParent = raycastHit.collider.GetComponentInParent<PrefabIdentifier>();
					RaftArchitect raftArchitect = (!componentInParent) ? null : componentInParent.GetComponent<RaftArchitect>();
					bool flag = raftArchitect && raftArchitect._wasBuilt;
					if (flag)
					{
						this._oarGo.SetActive(true);
						this._raftGo.SetActive(false);
						LocalPlayer.Create.Grabber.ShowPlace();
						LocalPlayer.Create.BuildingPlacer.SetClear();
						LocalPlayer.Create.BuildingPlacer.SetRenderer(this._oarRenderer);
					}
					else if (this._oarGo.activeSelf)
					{
						this._oarGo.SetActive(false);
						this._raftGo.SetActive(true);
						LocalPlayer.Create.Grabber.ClosePlace();
						LocalPlayer.Create.BuildingPlacer.SetRenderer(this._raftRenderer);
					}
				}
				else if (!this._raftGo.activeSelf)
				{
					this._oarGo.SetActive(false);
					this._raftGo.SetActive(true);
					LocalPlayer.Create.Grabber.ClosePlace();
					LocalPlayer.Create.BuildingPlacer.SetRenderer(this._raftRenderer);
				}
			}
		}

		
		private void OnPlaced()
		{
			base.enabled = false;
			Transform child;
			if (this._oarGo.activeSelf)
			{
				child = this._oarGo.transform.GetChild(0);
				this._oarGo.transform.GetChild(1).SendMessage("OnPlaced", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				child = this._raftGo.transform.GetChild(0);
			}
			if (!BoltNetwork.isRunning)
			{
				if (this._oarGo.activeSelf && LocalPlayer.Create.BuildingPlacer.LastHit != null && LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>())
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
				if ((this._oarGo.activeSelf && this._initializeOar) || (this._raftGo.activeSelf && this._initializeRaft))
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

		
		public LayerMask _layers;

		
		public RaftOarSwap.RayCastOrigins _origin;

		
		public float _raycastDistance = 5f;

		
		public GameObject _oarGo;

		
		public GameObject _raftGo;

		
		public bool _initializeOar;

		
		public bool _initializeRaft;

		
		private RaftArchitect _raft;

		
		private Vector3 _offsetWithPlacer;

		
		private Renderer _oarRenderer;

		
		private Renderer _raftRenderer;

		
		public enum RayCastOrigins
		{
			
			Player,
			
			Placer
		}
	}
}
