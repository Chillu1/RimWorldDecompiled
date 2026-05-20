using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_Stunnable : CompProperties
{
	public List<DamageDef> affectedDamageDefs = new List<DamageDef>();

	public List<DamageDef> adaptableDamageDefs = new List<DamageDef>();

	public bool useLargeEMPEffecter;

	public Vector3? empEffecterDimensions;

	public Vector3? empEffecterOffset;

	public float? empChancePerTick;

	public CompProperties_Stunnable()
	{
		compClass = typeof(CompStunnable);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (affectedDamageDefs.Count == 0)
		{
			yield return "CompProperties_Stunnable requires at least one affectedDamageDef";
		}
		foreach (DamageDef affectedDamageDef in affectedDamageDefs)
		{
			if (!affectedDamageDef.causeStun)
			{
				yield return "CompProperties_Stunnable: " + affectedDamageDef.defName + " does not cause stun";
			}
		}
		foreach (DamageDef adaptableDamageDef in adaptableDamageDefs)
		{
			if (adaptableDamageDef.stunAdaptationTicks <= 0)
			{
				yield return "CompProperties_Stunnable: " + adaptableDamageDef.defName + " does not have stunAdaptationTicks set";
			}
		}
	}
}
