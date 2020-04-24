using System;

namespace Verse
{
	[Flags]
	public enum MapMeshFlag
	{
		None = 0x0,
		Things = 0x1,
		FogOfWar = 0x2,
		Buildings = 0x4,
		GroundGlow = 0x8,
		Terrain = 0x10,
		Roofs = 0x20,
		Snow = 0x40,
		Zone = 0x80,
		PowerGrid = 0x100,
		BuildingsDamage = 0x200
	}
}
