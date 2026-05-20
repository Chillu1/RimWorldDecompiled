using RimWorld;

namespace Verse;

public class HediffComp_FleshbeastEmerge : HediffComp
{
	private static readonly IntRange StunDuration = new IntRange(120, 240);

	public HediffCompProperties_FleshbeastEmerge Props => (HediffCompProperties_FleshbeastEmerge)props;

	public override void Notify_SurgicallyRemoved(Pawn surgeon)
	{
		TentacleAttack(surgeon);
	}

	public override void Notify_SurgicallyReplaced(Pawn surgeon)
	{
		TentacleAttack(surgeon);
	}

	private void TentacleAttack(Pawn surgeon)
	{
		if (ModsConfig.AnomalyActive)
		{
			Pawn pawn = parent.pawn;
			PawnKindDef fingerspike = PawnKindDefOf.Fingerspike;
			Faction ofEntities = Faction.OfEntities;
			float? fixedBiologicalAge = 0f;
			float? fixedChronologicalAge = 0f;
			Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(fingerspike, ofEntities, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge, fixedChronologicalAge));
			GenSpawn.Spawn(pawn2, CellFinder.StandableCellNear(pawn.Position, pawn.Map, 2f), pawn.Map);
			pawn2.stances.stunner.StunFor(StunDuration.RandomInRange, surgeon);
			CompInspectStringEmergence compInspectStringEmergence = pawn2.TryGetComp<CompInspectStringEmergence>();
			if (compInspectStringEmergence != null)
			{
				compInspectStringEmergence.sourcePawn = pawn;
			}
			TaggedString label = Props.letterLabel.Formatted(pawn.Named("PAWN"));
			TaggedString text = Props.letterText.Formatted(pawn.Named("PAWN"));
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatBig, pawn2);
		}
	}
}
