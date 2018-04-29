using System;
using UnityEngine;


[RequireComponent(typeof(GUIText))]
public class AeroplaneGUI : MonoBehaviour
{
	
	private void Update()
	{
		object[] args = new object[]
		{
			this.plane.Throttle,
			this.plane.ForwardSpeed * 3.6f,
			this.plane.Altitude
		};
		base.GetComponent<GUIText>().text = string.Format(this.displayText, args);
	}

	
	public AeroplaneController plane;

	
	private const float MpsToKph = 3.6f;

	
	private string displayText = "\n\n\nThrottle: {0:0%}\nSpeed: {1:0000}KM/H\nAltitude: {2:0000}M";
}
