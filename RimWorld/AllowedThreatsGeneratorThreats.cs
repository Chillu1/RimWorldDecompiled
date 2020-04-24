using System;

namespace RimWorld
{
	[Flags]
	public enum AllowedThreatsGeneratorThreats
	{
		None = 0x0,
		Raids = 0x1,
		MechClusters = 0x2,
		All = 0x3
	}
}
