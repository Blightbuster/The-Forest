using System;
using System.Runtime.InteropServices;


public class OVRNativeBuffer : IDisposable
{
	
	public OVRNativeBuffer(int numBytes)
	{
		this.Reallocate(numBytes);
	}

	
	~OVRNativeBuffer()
	{
		this.Dispose(false);
	}

	
	public void Reset(int numBytes)
	{
		this.Reallocate(numBytes);
	}

	
	public int GetCapacity()
	{
		return this.m_numBytes;
	}

	
	public IntPtr GetPointer(int byteOffset = 0)
	{
		if (byteOffset < 0 || byteOffset >= this.m_numBytes)
		{
			return IntPtr.Zero;
		}
		return (byteOffset != 0) ? new IntPtr(this.m_ptr.ToInt64() + (long)byteOffset) : this.m_ptr;
	}

	
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	
	private void Dispose(bool disposing)
	{
		if (this.disposed)
		{
			return;
		}
		if (disposing)
		{
		}
		this.Release();
		this.disposed = true;
	}

	
	private void Reallocate(int numBytes)
	{
		this.Release();
		if (numBytes > 0)
		{
			this.m_ptr = Marshal.AllocHGlobal(numBytes);
			this.m_numBytes = numBytes;
		}
		else
		{
			this.m_ptr = IntPtr.Zero;
			this.m_numBytes = 0;
		}
	}

	
	private void Release()
	{
		if (this.m_ptr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(this.m_ptr);
			this.m_ptr = IntPtr.Zero;
			this.m_numBytes = 0;
		}
	}

	
	private bool disposed;

	
	private int m_numBytes;

	
	private IntPtr m_ptr = IntPtr.Zero;
}
