using Verse;

namespace RimWorld;

public class HediffComp_RoyalImplant : HediffComp
{
	public HediffCompProperties_RoyalImplant Props => (HediffCompProperties_RoyalImplant)props;

	public static int GetImplantLevel(Hediff implant)
	{
		if (implant is Hediff_Level hediff_Level)
		{
			return hediff_Level.level;
		}
		return 0;
	}

	public bool IsViolatingRulesOf(Faction faction, int violationSourceLevel = -1)
	{
		return ThingRequiringRoyalPermissionUtility.IsViolatingRulesOf(base.Def, parent.pawn, faction, (violationSourceLevel == -1) ? GetImplantLevel(parent) : violationSourceLevel);
	}

	public override void Notify_ImplantUsed(string violationSourceName, float detectionChance, int violationSourceLevel = -1)
	{
		base.Notify_ImplantUsed(violationSourceName, detectionChance);
		if (parent.pawn.Faction != Faction.OfPlayer || !Rand.Chance(detectionChance))
		{
			return;
		}
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (IsViolatingRulesOf(allFaction, violationSourceLevel))
			{
				allFaction.Notify_RoyalThingUseViolation(parent.def, base.Pawn, violationSourceName, detectionChance, violationSourceLevel);
			}
		}
	}
}
