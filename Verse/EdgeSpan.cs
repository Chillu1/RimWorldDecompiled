using System.Collections.Generic;

namespace Verse;

public struct EdgeSpan
{
	public IntVec3 root;

	public SpanDirection dir;

	public int length;

	public bool IsValid => length > 0;

	public IEnumerable<IntVec3> Cells
	{
		get
		{
			for (int i = 0; i < length; i++)
			{
				if (dir == SpanDirection.North)
				{
					yield return new IntVec3(root.x, 0, root.z + i);
				}
				else if (dir == SpanDirection.East)
				{
					yield return new IntVec3(root.x + i, 0, root.z);
				}
			}
		}
	}

	public override string ToString()
	{
		string[] obj = new string[7] { "(root=", null, null, null, null, null, null };
		IntVec3 intVec = root;
		obj[1] = intVec.ToString();
		obj[2] = ", dir=";
		obj[3] = dir.ToString();
		obj[4] = " + length=";
		obj[5] = length.ToString();
		obj[6] = ")";
		return string.Concat(obj);
	}

	public EdgeSpan(IntVec3 root, SpanDirection dir, int length)
	{
		this.root = root;
		this.dir = dir;
		this.length = length;
	}

	public ulong UniqueHashCode()
	{
		ulong num = root.UniqueHashCode();
		if (dir == SpanDirection.East)
		{
			num += 17592186044416L;
		}
		return num + (ulong)(281474976710656L * length);
	}
}
