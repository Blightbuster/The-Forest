using System;
using UnityEngine;

public class wormAttachController : MonoBehaviour
{
	[SerializeField]
	public wormAttachPoints[] AttachPoints;

	public bool canAttach = true;

	public int numAttachedWorms;
}
