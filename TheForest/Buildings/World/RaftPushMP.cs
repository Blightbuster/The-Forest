using System;
using System.Collections.Generic;
using TheForest.World;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class RaftPushMP : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
			if (BoltNetwork.isClient)
			{
				base.GetComponent<Buoyancy>().enabled = false;
				UnityEngine.Object.Destroy(base.GetComponent<raftOnLand>());
				base.GetComponent<Rigidbody>().isKinematic = true;
				UnityEngine.Object.Destroy(this);
			}
			this._floor = base.transform.GetComponent<DynamicFloor>();
		}

		
		private void FixedUpdate()
		{
			bool flag = false;
			for (int i = 0; i < this._commands.Length; i++)
			{
				RaftPushMP.OarCommand oarCommand = this._commands[i];
				if (oarCommand._activeCommandTime > Time.realtimeSinceStartup)
				{
					this._oars[i].PushRaft(oarCommand._direction);
					this._oars[i].TurnRaft(oarCommand._rotation);
					flag = true;
				}
			}
			if (!flag)
			{
				base.enabled = false;
			}
		}

		
		private void OnDestroy()
		{
			this._commands = null;
		}

		
		public void ReceivedCommand(int oarId, RaftPush.MoveDirection direction, float rotation)
		{
			this.InitOarCommand();
			this._commands[oarId]._direction = direction;
			this._commands[oarId]._rotation = rotation;
			this._commands[oarId]._activeCommandTime = Time.realtimeSinceStartup + this._commandDuration;
			base.enabled = true;
		}

		
		private void InitOarCommand()
		{
			if (this._commands == null || this._commands.Length != this._oars.Count)
			{
				this._commands = new RaftPushMP.OarCommand[this._oars.Count];
				for (int i = 0; i < this._commands.Length; i++)
				{
					this._commands[i] = new RaftPushMP.OarCommand();
				}
			}
		}

		
		public float _commandDuration = 0.35f;

		
		public List<RaftPush> _oars;

		
		private DynamicFloor _floor;

		
		private RaftPushMP.OarCommand[] _commands;

		
		[Serializable]
		public class OarCommand
		{
			
			public RaftPush.MoveDirection _direction;

			
			public float _rotation;

			
			public float _activeCommandTime;
		}
	}
}
