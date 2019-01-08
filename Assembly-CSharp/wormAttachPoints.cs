using System;
using UnityEngine;

[Serializable]
public class wormAttachPoints
{
	public Transform attachJoint;

	public Transform[] AttachedWorm = new Transform[4];

	public int currentEmptySlot;
}
