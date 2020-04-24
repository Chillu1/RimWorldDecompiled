using System;

namespace Verse
{
	[Flags]
	public enum RegionType
	{
		None = 0x0,
		ImpassableFreeAirExchange = 0x1,
		Normal = 0x2,
		Portal = 0x4,
		Set_Passable = 0x6,
		Set_Impassable = 0x1,
		Set_All = 0x7
	}
}
