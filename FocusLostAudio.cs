using System;
using System.Collections.Generic;
using System.Threading;
using FMOD.Studio;
using UnityEngine;


public class FocusLostAudio : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.commandQueue = new List<FocusLostAudio.Command>();
		this.workerThread = new Thread(new ThreadStart(this.WorkerThreadRun));
		this.workerThread.Start();
		base.transform.parent = null;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	
	private void OnApplicationFocus(bool focused)
	{
		if (focused)
		{
			this.IssueCommand(FocusLostAudio.Command.DisableSnapshot);
		}
		else
		{
			this.IssueCommand(FocusLostAudio.Command.EnableSnapshot);
		}
	}

	
	private void OnDisable()
	{
		this.IssueCommand(FocusLostAudio.Command.Shutdown);
	}

	
	private void IssueCommand(FocusLostAudio.Command command)
	{
		object obj = this.commandQueue;
		lock (obj)
		{
			this.commandQueue.Add(command);
			Monitor.Pulse(this.commandQueue);
		}
	}

	
	private void OnLevelWasLoaded()
	{
		if (Application.loadedLevelName == "TitleScene")
		{
			if (this.destroyOnTitleSceneLoad)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			this.destroyOnTitleSceneLoad = true;
		}
	}

	
	private void WorkerThreadRun()
	{
		object obj = this.commandQueue;
		lock (obj)
		{
			bool flag = false;
			while (!flag)
			{
				if (this.commandQueue.Count == 0)
				{
					Monitor.Wait(this.commandQueue);
				}
				if (this.commandQueue.Count > 0)
				{
					FocusLostAudio.Command command = this.commandQueue[0];
					if (command != FocusLostAudio.Command.EnableSnapshot)
					{
						if (command != FocusLostAudio.Command.DisableSnapshot)
						{
							if (command == FocusLostAudio.Command.Shutdown)
							{
								this.DisableSnapshot();
								flag = true;
							}
						}
						else
						{
							this.DisableSnapshot();
						}
					}
					else
					{
						this.EnableSnapshot();
					}
					this.commandQueue.RemoveAt(0);
				}
			}
		}
	}

	
	private void EnableSnapshot()
	{
		if (FMOD_StudioSystem.instance && this.snapshotInstance == null)
		{
			this.snapshotInstance = FMOD_StudioSystem.instance.GetEvent("snapshot:/Focus Lost");
			if (this.snapshotInstance != null)
			{
				UnityUtil.ERRCHECK(this.snapshotInstance.start());
			}
		}
	}

	
	private void DisableSnapshot()
	{
		if (this.snapshotInstance != null && this.snapshotInstance.isValid())
		{
			UnityUtil.ERRCHECK(this.snapshotInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.snapshotInstance.release());
			this.snapshotInstance = null;
		}
	}

	
	private List<FocusLostAudio.Command> commandQueue;

	
	private Thread workerThread;

	
	private EventInstance snapshotInstance;

	
	private bool destroyOnTitleSceneLoad;

	
	private enum Command
	{
		
		EnableSnapshot,
		
		DisableSnapshot,
		
		Shutdown
	}
}
