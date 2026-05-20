using Verse;

namespace RimWorld;

public class CompProperties_AbilityGiveMentalState : CompProperties_AbilityEffect
{
	public MentalStateDef stateDef;

	public MentalStateDef stateDefForMechs;

	public StatDef durationMultiplier;

	public bool applyToSelf;

	public EffecterDef casterEffect;

	public EffecterDef targetEffect;

	public bool excludeNPCFactions;

	public bool forced;
}
