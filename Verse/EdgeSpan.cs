using System.Collections.Generic;

namespace Verse
{
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
			return string.Concat("(root=", root, ", dir=", dir.ToString(), " + length=", length, ")");
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
}
