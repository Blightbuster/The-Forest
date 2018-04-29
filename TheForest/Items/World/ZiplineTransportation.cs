using System;
using Bolt;
using FMOD.Studio;
using PathologicalGames;
using TheForest.Items.Utils;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class ZiplineTransportation : EntityBehaviour<IRigidbodyState>
	{
		
		private void Update()
		{
			if (!BoltNetwork.isRunning || (base.entity.isAttached && base.entity.isOwner))
			{
				Rigidbody component = base.GetComponent<Rigidbody>();
				component.velocity += base.transform.forward * this._accelerationSpeed * Time.deltaTime;
				if (component.velocity.sqrMagnitude == 0f)
				{
					this._notMovingFrames++;
				}
				else
				{
					this._notMovingFrames = 0;
				}
				if (this._notMovingFrames >= 30 || !this._targetTr)
				{
					this.Replace();
					return;
				}
				base.transform.position = this._targetTr.position + this._targetTr.forward * Vector3.Distance(this._targetTr.position, base.transform.position);
				base.transform.LookAt(this._targetTr);
			}
			PLAYBACK_STATE state;
			UnityUtil.ERRCHECK(this._sfxLoopInstance.getPlaybackState(out state));
			ATTRIBUTES_3D attributes_3D;
			UnityUtil.ERRCHECK(this._sfxLoopInstance.get3DAttributes(out attributes_3D));
			Vector3 vector = attributes_3D.position.toUnityVector();
			Vector3 vector2 = Vector3.SmoothDamp(vector, base.transform.position, ref this._sfxLoopVelocity, 0.2f);
			UnityUtil.ERRCHECK(this._sfxLoopInstance.set3DAttributes(vector2.to3DAttributes()));
			ParameterInstance parameterInstance;
			this._sfxLoopInstance.getParameter("speed", out parameterInstance);
			UnityUtil.ERRCHECK(parameterInstance.setValue(Vector3.Distance(vector, vector2) / Time.deltaTime / 50f));
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this._sfxLoopInstance.start());
			}
		}

		
		private void OnEnable()
		{
			if (this._sfxLoopInstance == null && !string.IsNullOrEmpty(this._sfxLoopPath))
			{
				this._sfxLoopInstance = FMOD_StudioSystem.instance.GetEvent(this._sfxLoopPath);
				UnityUtil.ERRCHECK(this._sfxLoopInstance.set3DAttributes(base.transform.to3DAttributes()));
			}
			this._notMovingFrames = 0;
			base.GetComponent<Rigidbody>().isKinematic = false;
			if (LocalPlayer.FpCharacter)
			{
				Physics.IgnoreCollision(base.GetComponent<Collider>(), LocalPlayer.FpCharacter.capsule);
				Physics.IgnoreCollision(base.GetComponent<Collider>(), LocalPlayer.FpCharacter.HeadBlock);
			}
		}

		
		private void OnDisable()
		{
			if (this._sfxLoopInstance != null && !string.IsNullOrEmpty(this._sfxLoopPath))
			{
				PLAYBACK_STATE state;
				UnityUtil.ERRCHECK(this._sfxLoopInstance.getPlaybackState(out state));
				if (state.isPlaying())
				{
					UnityUtil.ERRCHECK(this._sfxLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
					this._sfxLoopInstance = null;
				}
			}
			base.GetComponent<Rigidbody>().isKinematic = true;
		}

		
		private void OnCollisionEnter(Collision col)
		{
			if (base.isActiveAndEnabled && (!BoltNetwork.isRunning || (base.entity.isAttached && base.entity.isOwner)))
			{
				if (!col.collider.CompareTag("Player"))
				{
					this.Replace();
				}
				else
				{
					Physics.IgnoreCollision(base.GetComponent<Collider>(), col.collider);
				}
			}
		}

		
		private void Replace()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			GameObject gameObject = ItemUtils.SpawnItem(this._itemId, this._spawnTr.position, this._spawnTr.rotation, false);
			if (gameObject)
			{
				Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
				if (component2)
				{
					component2.velocity = component.velocity / 2f;
					component2.AddRelativeTorque(0f, 0f, -component.velocity.sqrMagnitude / 2f, ForceMode.Impulse);
				}
			}
			if (PoolManager.Pools["misc"].IsSpawned(base.transform))
			{
				component.velocity = Vector3.zero;
				if (BoltNetwork.isRunning)
				{
					BoltNetwork.Detach(base.gameObject);
				}
				PoolManager.Pools["misc"].Despawn(base.transform);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		public override void Attached()
		{
			base.state.Transform.SetTransforms(base.transform);
		}

		
		[ItemIdPicker]
		public int _itemId;

		
		public Transform _spawnTr;

		
		public float _accelerationSpeed = 5f;

		
		public string _sfxLoopPath;

		
		public Transform _targetTr;

		
		private EventInstance _sfxLoopInstance;

		
		private Vector3 _sfxLoopVelocity;

		
		private int _notMovingFrames;
	}
}
