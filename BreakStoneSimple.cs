using System;
using System.Collections;
using TheForest.Networking;
using TheForest.Utils;
using UnityEngine;


public class BreakStoneSimple : MonoBehaviour
{
	
	public void ClientExplodeCheck(bool isLoad = false)
	{
		if (!this.blownUp && CoopWeatherProxy.Instance && CoopWeatherProxy.Instance.state.ExplodeCaches[this.CacheIndex] == 1)
		{
			if (isLoad)
			{
				this.DoExplodeFinal();
			}
			else
			{
				this.DoExplode();
			}
		}
	}

	
	public void ClientHashExplodeCheck(string hash)
	{
		if (!this.blownUp && hash.Equals(HashPositionToName.GetHash(base.transform.position)))
		{
			this.DoExplode();
		}
	}

	
	private IEnumerator Start()
	{
		if (!BoltNetwork.isClient && !this.HiddenCache)
		{
			bool exploded = GlobalDataSaver.GetInt(base.transform.ToGeoHash() + "_exploded", 0) == 1;
			if (exploded)
			{
				this.SetExplodedMp();
				this.DoExplodeFinal();
			}
		}
		if (BoltNetwork.isClient || (BoltNetwork.isServer && !this.HiddenCache))
		{
			this.PositionHashBlownUpCheck();
		}
		else if (this.HiddenCache)
		{
			while (LevelSerializer.IsDeserializing)
			{
				yield return null;
			}
			if (GlobalDataSaver.GetInt("HiddenCache" + this.CacheIndex, 0) == 1)
			{
				if (this.InsidePos)
				{
					this.InsidePos.gameObject.SetActive(true);
				}
				UnityEngine.Object.Destroy(base.gameObject);
				this.SetExplodedMp();
			}
		}
		yield break;
	}

	
	private void PositionHashBlownUpCheck()
	{
		if (!this.blownUp && GameObject.Find(HashPositionToName.GetHash(base.transform.position)))
		{
			this.DoExplodeFinal();
		}
	}

	
	private void DoExplode()
	{
		this.blownUp = true;
		this.Cut1.SetActive(true);
		this.Cut2.SetActive(true);
		this.Cut3.SetActive(true);
		this.Cut4.SetActive(true);
		this.Cut5.SetActive(true);
		this.Cut1.transform.parent = null;
		this.Cut2.transform.parent = null;
		this.Cut3.transform.parent = null;
		this.Cut4.transform.parent = null;
		this.Cut5.transform.parent = null;
		this.Cut1.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(15, 50), (float)UnityEngine.Random.Range(25, 50), (float)UnityEngine.Random.Range(15, 50), ForceMode.Impulse);
		this.Cut2.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(15, 50), (float)UnityEngine.Random.Range(25, 50), (float)UnityEngine.Random.Range(15, 50), ForceMode.Impulse);
		this.Cut3.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(15, 50), (float)UnityEngine.Random.Range(25, 50), (float)UnityEngine.Random.Range(15, 50), ForceMode.Impulse);
		this.Cut4.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(15, 50), (float)UnityEngine.Random.Range(25, 50), (float)UnityEngine.Random.Range(15, 50), ForceMode.Impulse);
		this.Cut5.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(15, 50), (float)UnityEngine.Random.Range(25, 50), (float)UnityEngine.Random.Range(15, 50), ForceMode.Impulse);
		if (this.InsidePos)
		{
			this.InsidePos.gameObject.SetActive(true);
			if (this.HiddenCache && this.WorldItem.Length > 0)
			{
				this.MyItem = (UnityEngine.Object.Instantiate(this.WorldItem[UnityEngine.Random.Range(0, this.WorldItem.Length - 1)], this.InsidePos.position, this.InsidePos.rotation) as GameObject);
				this.MyItem.transform.parent = this.InsidePos.transform;
			}
			if (this.SpawnWhenExplode != null && this.SpawnWhenExplode.Length > 0)
			{
				foreach (GameObject original in this.SpawnWhenExplode)
				{
					Vector3 vector = this.InsidePos.position + new Vector3(UnityEngine.Random.value * 0.5f, 0f, UnityEngine.Random.value * 0.5f);
					float num = Terrain.activeTerrain.SampleHeight(vector);
					if (vector.y < num)
					{
						vector.y = num + 0.2f;
					}
					this.MyItem = (UnityEngine.Object.Instantiate(original, vector, this.InsidePos.rotation) as GameObject);
					this.MyItem.transform.parent = this.InsidePos.transform;
				}
			}
		}
		if (this.RopePlace)
		{
			this.MyRopeMaker.SetActive(true);
		}
		if (!BoltNetwork.isClient)
		{
			if (this.HiddenCache)
			{
				GlobalDataSaver.SetInt("HiddenCache" + this.CacheIndex, 1);
			}
			else
			{
				GlobalDataSaver.SetInt(base.transform.ToGeoHash() + "_exploded", 1);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	private void DoExplodeFinal()
	{
		this.blownUp = true;
		UnityEngine.Object.Destroy(this.Cut1);
		UnityEngine.Object.Destroy(this.Cut2);
		UnityEngine.Object.Destroy(this.Cut3);
		UnityEngine.Object.Destroy(this.Cut4);
		UnityEngine.Object.Destroy(this.Cut5);
		if (this.RopePlace)
		{
			this.MyRopeMaker.SetActive(true);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	private void Explosion()
	{
		if ((this.HiddenCache && BoltNetwork.isClient) || this.blownUp)
		{
			return;
		}
		this.SetExplodedMp();
		this.DoExplode();
	}

	
	private void SetExplodedMp()
	{
		if (BoltNetwork.isServer && this.HiddenCache)
		{
			CoopWeatherProxy.Instance.state.ExplodeCaches[this.CacheIndex] = 1;
		}
		else if (BoltNetwork.isRunning && !GameObject.Find(HashPositionToName.GetHash(base.transform.position)))
		{
			BoltNetwork.Instantiate(Prefabs.Instance.HashPositionToNamePrefab, base.transform.position, base.transform.rotation);
		}
	}

	
	public bool HiddenCache;

	
	public bool RopePlace;

	
	public int CacheIndex;

	
	public GameObject Cut1;

	
	public GameObject Cut2;

	
	public GameObject Cut3;

	
	public GameObject Cut4;

	
	public GameObject Cut5;

	
	public Transform InsidePos;

	
	private GameObject MyItem;

	
	public GameObject[] WorldItem;

	
	public GameObject[] SpawnWhenExplode;

	
	public GameObject MyRopeMaker;

	
	private bool blownUp;
}
