using System;

namespace Verse;

[Flags]
public enum RegionType
{
	None = 0,
	ImpassableFreeAirExchange = 1,
	Normal = 2,
	Portal = 4,
	Fence = 8,
	Set_Passable = 0xE,
	Set_Impassable = 1,
	Set_All = 0xF
}
