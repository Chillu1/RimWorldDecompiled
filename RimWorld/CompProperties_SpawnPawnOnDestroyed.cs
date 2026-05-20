using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompProperties_SpawnPawnOnDestroyed : CompProperties
{
	public PawnKindDef pawnKind;

	public Type lordJob;

	public CompProperties_SpawnPawnOnDestroyed()
	{
		compClass = typeof(CompSpawnPawnOnDestroyed);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (lordJob != null && !typeof(LordJob).IsAssignableFrom(lordJob))
		{
			yield return $"lordJob {lordJob} must be of type LordJob";
		}
	}
}
