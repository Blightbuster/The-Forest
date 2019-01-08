using System;

[Serializable]
public class clsdismemberatorbonerelationsindexes
{
	public clsdismemberatorbonerelationsindexes()
	{
		this.propparentside = new clsdismemberatorindexer();
		this.propchildrenside = new clsdismemberatorindexer();
	}

	public clsdismemberatorindexer propparentside;

	public clsdismemberatorindexer propchildrenside;
}
