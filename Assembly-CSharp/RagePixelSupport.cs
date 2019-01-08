using System;
using Serialization;

[SerializerPlugIn]
public class RagePixelSupport
{
	static RagePixelSupport()
	{
		new SerializePrivateFieldOfType("RagePixelSprite", "animationPingPongDirection");
		new SerializePrivateFieldOfType("RagePixelSprite", "myTime");
	}
}
