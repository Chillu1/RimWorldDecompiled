using System;

namespace RimWorld;

[Flags]
public enum AllowedThreatsGeneratorThreats
{
	None = 0,
	Raids = 1,
	MechClusters = 2,
	All = 3
}
