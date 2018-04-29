using System;
using System.Runtime.InteropServices;
using UnityEngine;


public class OVRBoundary
{
	
	public bool GetConfigured()
	{
		return OVRPlugin.GetBoundaryConfigured();
	}

	
	public OVRBoundary.BoundaryTestResult TestNode(OVRBoundary.Node node, OVRBoundary.BoundaryType boundaryType)
	{
		OVRPlugin.BoundaryTestResult boundaryTestResult = OVRPlugin.TestBoundaryNode((OVRPlugin.Node)node, (OVRPlugin.BoundaryType)boundaryType);
		return new OVRBoundary.BoundaryTestResult
		{
			IsTriggering = (boundaryTestResult.IsTriggering == OVRPlugin.Bool.True),
			ClosestDistance = boundaryTestResult.ClosestDistance,
			ClosestPoint = boundaryTestResult.ClosestPoint.FromFlippedZVector3f(),
			ClosestPointNormal = boundaryTestResult.ClosestPointNormal.FromFlippedZVector3f()
		};
	}

	
	public OVRBoundary.BoundaryTestResult TestPoint(Vector3 point, OVRBoundary.BoundaryType boundaryType)
	{
		OVRPlugin.BoundaryTestResult boundaryTestResult = OVRPlugin.TestBoundaryPoint(point.ToFlippedZVector3f(), (OVRPlugin.BoundaryType)boundaryType);
		return new OVRBoundary.BoundaryTestResult
		{
			IsTriggering = (boundaryTestResult.IsTriggering == OVRPlugin.Bool.True),
			ClosestDistance = boundaryTestResult.ClosestDistance,
			ClosestPoint = boundaryTestResult.ClosestPoint.FromFlippedZVector3f(),
			ClosestPointNormal = boundaryTestResult.ClosestPointNormal.FromFlippedZVector3f()
		};
	}

	
	public void SetLookAndFeel(OVRBoundary.BoundaryLookAndFeel lookAndFeel)
	{
		OVRPlugin.BoundaryLookAndFeel boundaryLookAndFeel = new OVRPlugin.BoundaryLookAndFeel
		{
			Color = lookAndFeel.Color.ToColorf()
		};
		OVRPlugin.SetBoundaryLookAndFeel(boundaryLookAndFeel);
	}

	
	public void ResetLookAndFeel()
	{
		OVRPlugin.ResetBoundaryLookAndFeel();
	}

	
	public Vector3[] GetGeometry(OVRBoundary.BoundaryType boundaryType)
	{
		int num = 0;
		if (OVRPlugin.GetBoundaryGeometry2((OVRPlugin.BoundaryType)boundaryType, IntPtr.Zero, ref num) && num > 0)
		{
			int num2 = num * OVRBoundary.cachedVector3fSize;
			if (OVRBoundary.cachedGeometryNativeBuffer.GetCapacity() < num2)
			{
				OVRBoundary.cachedGeometryNativeBuffer.Reset(num2);
			}
			int num3 = num * 3;
			if (OVRBoundary.cachedGeometryManagedBuffer.Length < num3)
			{
				OVRBoundary.cachedGeometryManagedBuffer = new float[num3];
			}
			if (OVRPlugin.GetBoundaryGeometry2((OVRPlugin.BoundaryType)boundaryType, OVRBoundary.cachedGeometryNativeBuffer.GetPointer(0), ref num))
			{
				Marshal.Copy(OVRBoundary.cachedGeometryNativeBuffer.GetPointer(0), OVRBoundary.cachedGeometryManagedBuffer, 0, num3);
				Vector3[] array = new Vector3[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = new OVRPlugin.Vector3f
					{
						x = OVRBoundary.cachedGeometryManagedBuffer[3 * i],
						y = OVRBoundary.cachedGeometryManagedBuffer[3 * i + 1],
						z = OVRBoundary.cachedGeometryManagedBuffer[3 * i + 2]
					}.FromFlippedZVector3f();
				}
				return array;
			}
		}
		return new Vector3[0];
	}

	
	public Vector3 GetDimensions(OVRBoundary.BoundaryType boundaryType)
	{
		return OVRPlugin.GetBoundaryDimensions((OVRPlugin.BoundaryType)boundaryType).FromVector3f();
	}

	
	public bool GetVisible()
	{
		return OVRPlugin.GetBoundaryVisible();
	}

	
	public void SetVisible(bool value)
	{
		OVRPlugin.SetBoundaryVisible(value);
	}

	
	private static int cachedVector3fSize = Marshal.SizeOf(typeof(OVRPlugin.Vector3f));

	
	private static OVRNativeBuffer cachedGeometryNativeBuffer = new OVRNativeBuffer(0);

	
	private static float[] cachedGeometryManagedBuffer = new float[0];

	
	public enum Node
	{
		
		HandLeft = 3,
		
		HandRight,
		
		Head = 9
	}

	
	public enum BoundaryType
	{
		
		OuterBoundary = 1,
		
		PlayArea = 256
	}

	
	public struct BoundaryTestResult
	{
		
		public bool IsTriggering;

		
		public float ClosestDistance;

		
		public Vector3 ClosestPoint;

		
		public Vector3 ClosestPointNormal;
	}

	
	public struct BoundaryLookAndFeel
	{
		
		public Color Color;
	}
}
