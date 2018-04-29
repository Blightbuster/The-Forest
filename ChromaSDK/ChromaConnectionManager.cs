using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ChromaSDK.Api;
using RazerSDK.Api;
using RazerSDK.ChromaPackage.Model;
using UnityEngine;

namespace ChromaSDK
{
	
	public class ChromaConnectionManager : MonoBehaviour, IUpdate
	{
		
		
		public static ChromaConnectionManager Instance
		{
			get
			{
				if (null == ChromaConnectionManager._sInstance)
				{
					GameObject gameObject = GameObject.Find("ChromaConnectionManager");
					if (null == gameObject)
					{
						gameObject = new GameObject("ChromaConnectionManager");
					}
					else
					{
						ChromaConnectionManager._sInstance = gameObject.GetComponent<ChromaConnectionManager>();
					}
					if (null == ChromaConnectionManager._sInstance)
					{
						ChromaConnectionManager._sInstance = gameObject.AddComponent<ChromaConnectionManager>();
					}
				}
				return ChromaConnectionManager._sInstance;
			}
		}

		
		
		public ChromaApi ApiChromaInstance
		{
			get
			{
				return ChromaConnectionManager._sApiChromaInstance;
			}
		}

		
		
		public bool Connecting
		{
			get
			{
				return ChromaConnectionManager._sConnecting;
			}
		}

		
		
		public bool Connected
		{
			get
			{
				return ChromaConnectionManager._sConnected;
			}
		}

		
		
		
		public string ConnectionStatus
		{
			get
			{
				return ChromaConnectionManager._sConnectionStatus;
			}
			private set
			{
				ChromaConnectionManager._sConnectionStatus = value;
			}
		}

		
		protected void SafeStartCoroutine(string routineName, IEnumerator routine)
		{
			if (Application.isPlaying)
			{
				base.StartCoroutine(routine);
			}
			else
			{
				ChromaConnectionManager._sPendingRoutines.Add(routine);
			}
		}

		
		private void RunOnMainThread(Action action)
		{
			ChromaConnectionManager._sMainActions.Add(action);
		}

		
		public void Awake()
		{
			this.Connect();
		}

		
		public void Update()
		{
			if (ChromaConnectionManager._sMainActions.Count > 0)
			{
				Action action = ChromaConnectionManager._sMainActions[0];
				ChromaConnectionManager._sMainActions.RemoveAt(0);
				action();
			}
			int i = 0;
			while (i < ChromaConnectionManager._sPendingRoutines.Count)
			{
				IEnumerator enumerator = ChromaConnectionManager._sPendingRoutines[i];
				try
				{
					if (enumerator != null && enumerator.MoveNext())
					{
						i++;
						continue;
					}
				}
				catch (Exception)
				{
				}
				ChromaConnectionManager._sPendingRoutines.RemoveAt(i);
			}
		}

		
		private IEnumerator Initialize()
		{
			DateTime wait = DateTime.Now + TimeSpan.FromSeconds(2.0);
			while (DateTime.Now < wait)
			{
				yield return null;
			}
			ChromaUtils.RunOnThread(delegate
			{
				this.<>f__this.PostChromaSdk();
			});
			yield break;
		}

		
		private void LogOnMainThread(string text)
		{
			this.RunOnMainThread(delegate
			{
				Debug.Log(text);
			});
		}

		
		private void LogErrorOnMainThread(string text)
		{
			this.RunOnMainThread(delegate
			{
				Debug.LogError(text);
			});
		}

		
		public void SetupDefaultInfo()
		{
			this._mInfo = new ChromaSdkInput(null, null, null, null, null);
			this._mInfo.Title = "UnityPlugin";
			this._mInfo.Description = "REST client for Unity";
			this._mInfo.Author = new ChromaSdkInputAuthor(null, null);
			this._mInfo.Author.Name = "Chroma Developer";
			this._mInfo.Author.Contact = "www.razerzone.com";
			this._mInfo.DeviceSupported = new List<string>
			{
				"keyboard",
				"mouse",
				"headset",
				"mousepad",
				"keypad",
				"chromalink"
			};
			this._mInfo.Category = "application";
		}

		
		private void PostChromaSdk()
		{
			bool reconnect = false;
			try
			{
				if (ChromaConnectionManager._sApiRazerInstance != null)
				{
					return;
				}
				ChromaConnectionManager._sApiRazerInstance = new RazerApi(null);
				if (this._mInfo == null)
				{
					this.SetupDefaultInfo();
				}
				PostChromaSdkResponse result = null;
				DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(5.0);
				Thread thread = new Thread(delegate
				{
					try
					{
						this.ConnectionStatus = "Connecting";
						result = ChromaConnectionManager._sApiRazerInstance.PostChromaSdk(this._mInfo);
					}
					catch (Exception)
					{
						reconnect = true;
					}
				});
				thread.Start();
				while (ChromaConnectionManager._sWaitForExit && DateTime.Now < dateTime && thread.IsAlive)
				{
					Thread.Sleep(0);
				}
				if (ChromaConnectionManager._sWaitForExit && dateTime < DateTime.Now && thread.IsAlive)
				{
					thread.Abort();
					reconnect = true;
					this.ConnectionStatus = "Reconnnect, Connect timeout!";
					this.ThreadWaitForSecond();
				}
				if (result != null)
				{
					ChromaConnectionManager._sApiChromaInstance = new ChromaApi(result.Uri);
					this.DoHeartbeat();
				}
				else
				{
					reconnect = true;
				}
			}
			catch (Exception)
			{
				reconnect = true;
			}
			if (reconnect)
			{
				ChromaConnectionManager._sApiRazerInstance = null;
				if (ChromaConnectionManager._sWaitForExit)
				{
					this.RunOnMainThread(delegate
					{
						this.SafeStartCoroutine("Initialize", this.Initialize());
					});
				}
			}
		}

		
		private void ThreadWaitForSecond()
		{
			DateTime t = DateTime.Now + TimeSpan.FromSeconds(1.0);
			while (ChromaConnectionManager._sWaitForExit && DateTime.Now < t)
			{
				Thread.Sleep(0);
			}
		}

		
		private void DoHeartbeat()
		{
			bool flag = false;
			if (ChromaConnectionManager._sApiChromaInstance == null)
			{
				this.LogErrorOnMainThread("DoHeartbeat: ApiChromaInstance is null!");
				flag = true;
				this.ConnectionStatus = "Reconnect, ChromaAPI is null!";
				this.ThreadWaitForSecond();
			}
			else
			{
				while (ChromaConnectionManager._sWaitForExit && ChromaConnectionManager._sApiChromaInstance != null)
				{
					DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(5.0);
					try
					{
						Thread thread = new Thread(delegate
						{
							try
							{
								ChromaConnectionManager._sApiChromaInstance.Heartbeat();
							}
							catch (Exception)
							{
							}
						});
						thread.Start();
						while (ChromaConnectionManager._sWaitForExit && DateTime.Now < dateTime && thread.IsAlive)
						{
							Thread.Sleep(0);
						}
						if (ChromaConnectionManager._sWaitForExit && dateTime < DateTime.Now && thread.IsAlive)
						{
							thread.Abort();
							flag = true;
						}
					}
					catch (Exception)
					{
						this.LogErrorOnMainThread("Failed to check heartbeat!");
						flag = true;
						this.ConnectionStatus = "Reconnnect, Heartbeat failed!";
						this.ThreadWaitForSecond();
					}
					if (dateTime < DateTime.Now)
					{
						Debug.LogError("Timeout detected!");
						flag = true;
						this.ConnectionStatus = "Reconnnect, Heartbeat timeout!";
						this.ThreadWaitForSecond();
					}
					if (flag)
					{
						break;
					}
					ChromaConnectionManager._sConnected = true;
					ChromaConnectionManager._sConnecting = false;
					this.ConnectionStatus = "Connected";
					this.ThreadWaitForSecond();
				}
				ChromaConnectionManager._sConnected = false;
				ChromaConnectionManager._sConnecting = false;
				this.ConnectionStatus = "Not Connected";
				this.ThreadWaitForSecond();
			}
			if (flag)
			{
				this.ThreadWaitForSecond();
				if (ChromaConnectionManager._sWaitForExit)
				{
					this.RunOnMainThread(delegate
					{
						this.Connect();
					});
				}
			}
		}

		
		private void ResetConnections()
		{
			ChromaConnectionManager._sApiRazerInstance = null;
			ChromaConnectionManager._sApiChromaInstance = null;
		}

		
		private void DeleteChromaSdk()
		{
			try
			{
				if (ChromaConnectionManager._sApiChromaInstance != null)
				{
					ChromaConnectionManager._sApiChromaInstance.DeleteChromaSdk();
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				this.ResetConnections();
			}
		}

		
		private void UnloadSceneAnimations()
		{
			ChromaSDKAnimation1D[] array = (ChromaSDKAnimation1D[])UnityEngine.Object.FindObjectsOfType(typeof(ChromaSDKAnimation1D));
			foreach (ChromaSDKAnimation1D chromaSDKAnimation1D in array)
			{
				chromaSDKAnimation1D.Reset();
			}
			ChromaSDKAnimation2D[] array3 = (ChromaSDKAnimation2D[])UnityEngine.Object.FindObjectsOfType(typeof(ChromaSDKAnimation2D));
			foreach (ChromaSDKAnimation2D chromaSDKAnimation2D in array3)
			{
				chromaSDKAnimation2D.Reset();
			}
		}

		
		public void Connect()
		{
			if (!ChromaConnectionManager._sConnecting && !this.Connected)
			{
				this.ResetConnections();
				this.UnloadSceneAnimations();
				ChromaConnectionManager._sWaitForExit = true;
				ChromaConnectionManager._sConnecting = true;
				this.ConnectionStatus = "Connecting";
				this.SafeStartCoroutine("Initialize", this.Initialize());
			}
		}

		
		public void Disconnect()
		{
			this.UnloadSceneAnimations();
			ChromaConnectionManager._sWaitForExit = false;
		}

		
		private const string CONNECTED = "Connected";

		
		private const string CONNECTING = "Connecting";

		
		private const string NOT_CONNECTED = "Not Connected";

		
		private const string RECONNECT_CHROMA_API_NULL = "Reconnect, ChromaAPI is null!";

		
		private const string RECONNECT_CHROMA_API_HEARTBEAT_FAILURE = "Reconnnect, Heartbeat failed!";

		
		private const string RECONNECT_CHROMA_API_HEARTBEAT_TIMEOUT = "Reconnnect, Heartbeat timeout!";

		
		private const string RECONNECT_RAZER_API_TIMEOUT = "Reconnnect, Connect timeout!";

		
		private const string INSTANCE_NAME = "ChromaConnectionManager";

		
		public ChromaSdkInput _mInfo;

		
		private static ChromaConnectionManager _sInstance = null;

		
		private static RazerApi _sApiRazerInstance = null;

		
		private static ChromaApi _sApiChromaInstance = null;

		
		private static bool _sConnecting = false;

		
		private static bool _sConnected = false;

		
		private static string _sConnectionStatus = "Not Connected";

		
		private static bool _sWaitForExit = true;

		
		private static List<IEnumerator> _sPendingRoutines = new List<IEnumerator>();

		
		private static List<Action> _sMainActions = new List<Action>();

		
		private bool _mDetectedCompile;
	}
}
