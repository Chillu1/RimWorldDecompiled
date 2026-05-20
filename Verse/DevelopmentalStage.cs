using System;

namespace Verse;

[Flags]
public enum DevelopmentalStage : uint
{
	None = 0u,
	Newborn = 1u,
	Baby = 2u,
	Child = 4u,
	Adult = 8u
}
