using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_RefugeePod : ThingSetMaker
	{
		private const float RelationWithColonistWeight = 20f;

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, DownedRefugeeQuestUtility.GetRandomFactionForRefugee(), PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f));
			outThings.Add(pawn);
			HealthUtility.DamageUntilDowned(pawn);
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			yield return PawnKindDefOf.SpaceRefugee.race;
		}
	}
}
