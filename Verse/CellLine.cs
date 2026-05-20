namespace Verse;

public struct CellLine
{
	private float zIntercept;

	private float slope;

	public float ZIntercept => zIntercept;

	public float Slope => slope;

	public CellLine(float zIntercept, float slope)
	{
		this.zIntercept = zIntercept;
		this.slope = slope;
	}

	public CellLine(IntVec3 cell, float slope)
	{
		this.slope = slope;
		zIntercept = (float)cell.z - (float)cell.x * slope;
	}

	public static CellLine Between(IntVec3 a, IntVec3 b)
	{
		float num = ((a.x != b.x) ? ((float)(b.z - a.z) / (float)(b.x - a.x)) : 100000000f);
		return new CellLine((float)a.z - (float)a.x * num, num);
	}

	public bool CellIsAbove(IntVec3 c)
	{
		return (float)c.z > slope * (float)c.x + zIntercept;
	}
}
