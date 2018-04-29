using System;
using Steamworks;
using TheForest.Items.World;
using TheForest.Utils;
using UdpKit;
using UnityEngine;


public class CoopVoice : MonoBehaviour
{
	
	
	public sceneTracker SceneTracker
	{
		get
		{
			if (!this.tracker)
			{
				this.tracker = UnityEngine.Object.FindObjectOfType<sceneTracker>();
			}
			return this.tracker;
		}
	}

	
	
	public bool IsLocal
	{
		get
		{
			return base.gameObject.CompareTag("Player");
		}
	}

	
	private void Update()
	{
		if (this.IsLocal)
		{
			if (base.GetComponentInParent<BoltEntity>().IsOwner() && this.WalkieTalkie.gameObject.activeInHierarchy)
			{
				if (this.recording)
				{
					uint num;
					uint num2;
					if (SteamUser.GetVoice(true, this.vc_cmp, 65536u, out num, false, null, 0u, out num2, 0u) == EVoiceResult.k_EVoiceResultOK && num > 0u)
					{
						BoltEntity component = base.GetComponent<BoltEntity>();
						if (component && component.isAttached)
						{
							if (BoltNetwork.isServer)
							{
								this.ForwardVoiceData(this.vc_cmp, (int)num);
							}
							else
							{
								this.SendVoiceData(this.vc_cmp, (int)num, BoltNetwork.server);
							}
						}
					}
				}
				else
				{
					SteamUser.StartVoiceRecording();
					SteamFriends.SetInGameVoiceSpeaking(SteamUser.GetSteamID(), true);
					this.recording = true;
				}
			}
			else if (this.recording)
			{
				this.recording = false;
				SteamFriends.SetInGameVoiceSpeaking(SteamUser.GetSteamID(), false);
				SteamUser.StopVoiceRecording();
			}
		}
	}

	
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			base.enabled = false;
		}
		else if (this.IsLocal)
		{
			CoopVoice.LocalVoice = this;
			base.enabled = true;
		}
	}

	
	private void OnDestroy()
	{
		if (object.ReferenceEquals(CoopVoice.LocalVoice, this))
		{
			CoopVoice.LocalVoice = null;
		}
	}

	
	public void ReceiveVoiceData(byte[] packet, int o)
	{
		BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
		if (componentInParent.IsAttached())
		{
			int num = Blit.ReadI32(packet, ref o);
			byte[] array = new byte[num];
			Blit.ReadBytes(packet, ref o, array, 0, num);
			this.ReceiveVoiceData_Unpacked(array, num);
		}
	}

	
	private void ReceiveVoiceData_Unpacked(byte[] voice, int size)
	{
		if (BoltNetwork.isServer)
		{
			this.ForwardVoiceData(voice, size);
			if (!CoopPeerStarter.DedicatedHost)
			{
				base.GetComponent<CoopSteamVoicePlayer>().DataReceived(voice, size);
			}
		}
		else if (LocalPlayer.Stats != null)
		{
			base.GetComponent<CoopSteamVoicePlayer>().DataReceived(voice, size);
		}
	}

	
	private void ForwardVoiceData(byte[] data, int size)
	{
		BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
		if (componentInParent && componentInParent.isAttached && this.SceneTracker)
		{
			foreach (GameObject gameObject in this.SceneTracker.allPlayers)
			{
				BoltEntity boltEntity = gameObject.GetComponentInParent<BoltEntity>();
				if (boltEntity == null)
				{
					boltEntity = gameObject.GetComponent<BoltEntity>();
				}
				if (!object.ReferenceEquals(boltEntity, componentInParent) && boltEntity.source != null)
				{
					this.SendVoiceData(data, size, boltEntity.source);
				}
			}
		}
	}

	
	private void SendVoiceData(byte[] voice, int size, BoltConnection sendTo)
	{
		BoltEntity component = base.GetComponent<BoltEntity>();
		try
		{
			int num = 0;
			byte[] array = new byte[size + 12];
			Blit.PackU64(array, ref num, component.networkId.PackedValue);
			Blit.PackI32(array, ref num, size);
			Blit.PackBytes(array, ref num, voice, 0, size);
			sendTo.StreamBytes(CoopVoice.VoiceChannel, array);
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
		}
	}

	
	public static UdpChannelName VoiceChannel;

	
	private byte[] vc_cmp = new byte[65536];

	
	private bool recording;

	
	private sceneTracker tracker;

	
	public static CoopVoice LocalVoice;

	
	public BatteryBasedTalkyWalky WalkieTalkie;
}
