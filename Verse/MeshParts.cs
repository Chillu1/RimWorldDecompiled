using System;

namespace Verse
{
	[Flags]
	public enum MeshParts : byte
	{
		None = 0x0,
		Verts = 0x1,
		Tris = 0x2,
		Colors = 0x4,
		UVs = 0x8,
		All = 0x7F
	}
}
