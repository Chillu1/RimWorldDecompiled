using System;

namespace RimWorld
{
	[Flags]
	public enum SpectateRectSide
	{
		None = 0x0,
		Up = 0x1,
		Right = 0x2,
		Down = 0x4,
		Left = 0x8,
		Vertical = 0x5,
		Horizontal = 0xA,
		All = 0xF
	}
}
