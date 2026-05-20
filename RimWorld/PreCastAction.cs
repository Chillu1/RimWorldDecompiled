using System;
using Verse;

namespace RimWorld;

public struct PreCastAction
{
	public Action<LocalTargetInfo, LocalTargetInfo> action;

	public int ticksAwayFromCast;
}
