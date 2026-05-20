using System;

namespace Verse;

[Flags]
public enum MeshParts : byte
{
	None = 0,
	Verts = 1,
	Tris = 2,
	Colors = 4,
	UVs1 = 8,
	UVs2 = 0x10,
	Normals = 0x20,
	All = 0x3F
}
