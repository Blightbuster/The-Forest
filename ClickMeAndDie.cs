using System;
using UnityEngine;


public class ClickMeAndDie : MonoBehaviour
{
	
	private void Start()
	{
		this.id = ClickMeAndDie._id++;
	}

	
	private void Update()
	{
		RaycastHit raycastHit;
		if (Input.GetMouseButtonDown(0) && base.GetComponent<Collider>().Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, 1000f))
		{
			JSONLevelSerializer.SaveObjectTreeToServer("ftp:
			{
				if (e == null)
				{
					Loom.QueueOnMainThread(delegate
					{
						JSONLevelSerializer.LoadObjectTreeFromServer("http:
					}, 2f);
				}
				else
				{
					Debug.Log(e.ToString());
				}
			});
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	private int id;

	
	private static int _id;
}
