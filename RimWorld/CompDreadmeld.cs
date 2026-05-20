using Verse;

namespace RimWorld;

public class CompDreadmeld : ThingComp
{
	private static readonly IntRange FingerSpikeSpawnCountRange = new IntRange(1, 3);

	private static readonly IntRange ThresholdSpawnPointsRange = new IntRange(100, 300);

	private static readonly IntRange BloodFilthCountRange = new IntRange(1, 2);

	private const int InitialJumpDistance = 5;

	private const int DamageSpawnThresholds = 200;

	private float totalDamageTaken;

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (!ModLister.CheckAnomaly("Dreadmeld") || !parent.Spawned)
		{
			return;
		}
		float num = totalDamageTaken;
		totalDamageTaken += totalDamageDealt;
		if (num == 0f)
		{
			int randomInRange = FingerSpikeSpawnCountRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				PawnKindDef fingerspike = PawnKindDefOf.Fingerspike;
				Faction faction = parent.Faction;
				float? fixedBiologicalAge = 0f;
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(fingerspike, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge));
				SpawnPawn(pawn, parent.PositionHeld, parent.MapHeld);
			}
			FleshbeastUtility.MeatSplatter(BloodFilthCountRange.RandomInRange, parent.PositionHeld, parent.MapHeld);
			FilthMaker.TryMakeFilth(parent.PositionHeld, parent.MapHeld, ThingDefOf.Filth_TwistedFlesh);
		}
		if ((int)num / 200 >= (int)totalDamageTaken / 200)
		{
			return;
		}
		foreach (Pawn fleshbeastsForPoint in FleshbeastUtility.GetFleshbeastsForPoints(ThresholdSpawnPointsRange.RandomInRange, parent.Map))
		{
			SpawnPawn(fleshbeastsForPoint, parent.PositionHeld, parent.MapHeld);
		}
		FleshbeastUtility.MeatSplatter(BloodFilthCountRange.RandomInRange, parent.PositionHeld, parent.MapHeld);
		FilthMaker.TryMakeFilth(parent.PositionHeld, parent.MapHeld, ThingDefOf.Filth_TwistedFlesh);
	}

	private void SpawnPawn(Pawn pawn, IntVec3 position, Map map)
	{
		GenSpawn.Spawn(pawn, position, map, WipeMode.VanishOrMoveAside);
		FleshbeastUtility.SpawnPawnAsFlyer(pawn, map, position);
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		if (ModLister.CheckAnomaly("Dreadmeld"))
		{
			UndercaveMapComponent undercaveMapComponent = prevMap?.GetComponent<UndercaveMapComponent>();
			if (undercaveMapComponent != null)
			{
				Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("LetterLabelUndercaveCollapsing".Translate(), "LetterUndercaveCollapsing".Translate(), LetterDefOf.NeutralEvent));
				undercaveMapComponent.pitGate?.BeginCollapsing();
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref totalDamageTaken, "totalDamageTaken", 0f);
	}
}
