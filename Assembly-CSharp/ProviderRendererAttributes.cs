﻿using System;
using Serialization;
using UnityEngine;

[AttributeListProvider(typeof(Renderer))]
[AttributeListProvider(typeof(AudioListener))]
[AttributeListProvider(typeof(ParticleEmitter))]
[AttributeListProvider(typeof(Cloth))]
[AttributeListProvider(typeof(Light))]
[AttributeListProvider(typeof(Joint))]
[AttributeListProvider(typeof(MeshFilter))]
[AttributeListProvider(typeof(TextMesh))]
public class ProviderRendererAttributes : ProvideAttributes
{
	public ProviderRendererAttributes() : base(new string[]
	{
		"active",
		"text",
		"anchor",
		"sharedMesh",
		"targetPosition",
		"alignment",
		"lineSpacing",
		"spring",
		"useSpring",
		"motor",
		"useMotor",
		"limits",
		"useLimits",
		"axis",
		"breakForce",
		"breakTorque",
		"connectedBody",
		"offsetZ",
		"playAutomatically",
		"animatePhysics",
		"tabSize",
		"enabled",
		"isTrigger",
		"emit",
		"minSize",
		"maxSize",
		"clip",
		"loop",
		"playOnAwake",
		"bypassEffects",
		"volume",
		"priority",
		"pitch",
		"mute",
		"dopplerLevel",
		"spread",
		"panLevel",
		"volumeRolloff",
		"minDistance",
		"maxDistance",
		"pan2D",
		"castShadows",
		"receiveShadows",
		"slopeLimit",
		"stepOffset",
		"skinWidth",
		"minMoveDistance",
		"center",
		"radius",
		"height",
		"canControl",
		"damper",
		"useFixedUpdate",
		"movement",
		"jumping",
		"movingPlatform",
		"sliding",
		"autoRotate",
		"maxRotationSpeed",
		"range",
		"angle",
		"velocity",
		"intensity",
		"secondaryAxis",
		"xMotion",
		"yMotion",
		"zMotion",
		"angularXMotion",
		"angularYMotion",
		"angularZMotion",
		"linearLimit",
		"lowAngularXLimit",
		"highAngularXLimit",
		"angularYLimit",
		"angularZLimit",
		"targetVelocity",
		"xDrive",
		"yDrive",
		"zDrive",
		"targetAngularVelocity",
		"rotationDriveMode",
		"angularXDrive",
		"angularYZDrive",
		"slerpDrive",
		"projectionMode",
		"projectionDistance",
		"projectionAngle",
		"configuredInWorldSpace",
		"swapBodies",
		"cookie",
		"color",
		"drawHalo",
		"shadowType",
		"renderMode",
		"cullingMask",
		"lightmapping",
		"type",
		"lineSpacing",
		"text",
		"anchor",
		"alignment",
		"tabSize",
		"fontSize",
		"fontStyle",
		"font",
		"characterSize",
		"minEnergy",
		"maxEnergy",
		"minEmission",
		"maxEmission",
		"rndRotation",
		"rndVelocity",
		"rndAngularVelocity",
		"angularVelocity",
		"emitterVelocityScale",
		"localVelocity",
		"worldVelocity",
		"useWorldVelocity"
	}, false)
	{
	}
}