using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThingSetMaker_MapGen_AncientPodContents : ThingSetMaker
{
	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		PodContentsType podContentsType = parms.podContentsType ?? Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
		switch (podContentsType)
		{
		case PodContentsType.AncientFriendly:
			outThings.Add(GenerateFriendlyAncient());
			break;
		case PodContentsType.AncientIncapped:
			outThings.Add(GenerateIncappedAncient());
			break;
		case PodContentsType.AncientHostile:
			outThings.Add(GenerateAngryAncient());
			break;
		case PodContentsType.Slave:
			outThings.Add(GenerateSlave());
			break;
		case PodContentsType.AncientHalfEaten:
			outThings.Add(GenerateHalfEatenAncient());
			outThings.AddRange(GenerateScarabs());
			break;
		default:
			Log.Error("Pod contents type not handled: " + podContentsType);
			break;
		case PodContentsType.Empty:
			break;
		}
	}

	private Pawn GenerateFriendlyAncient()
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true));
		GiveRandomLootInventoryForTombPawn(pawn);
		return pawn;
	}

	private Pawn GenerateIncappedAncient()
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true));
		HealthUtility.DamageUntilDowned(pawn);
		GiveRandomLootInventoryForTombPawn(pawn);
		return pawn;
	}

	private Pawn GenerateSlave()
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Slave, Faction.OfAncients, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true));
		HealthUtility.DamageUntilDowned(pawn);
		GiveRandomLootInventoryForTombPawn(pawn);
		if (Rand.Value < 0.5f)
		{
			HealthUtility.DamageUntilDead(pawn);
		}
		return pawn;
	}

	private Pawn GenerateAngryAncient()
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true));
		GiveRandomLootInventoryForTombPawn(pawn);
		return pawn;
	}

	private Pawn GenerateHalfEatenAncient()
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true));
		int num = Rand.Range(6, 10);
		for (int i = 0; i < num; i++)
		{
			pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(3, 8), 0f, -1f, pawn));
		}
		GiveRandomLootInventoryForTombPawn(pawn);
		return pawn;
	}

	private List<Thing> GenerateScarabs()
	{
		List<Thing> list = new List<Thing>();
		int num = Rand.Range(3, 6);
		for (int i = 0; i < num; i++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab);
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
			list.Add(pawn);
		}
		return list;
	}

	private void GiveRandomLootInventoryForTombPawn(Pawn p)
	{
		if (Rand.Value < 0.65f)
		{
			MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Gold, Rand.Range(10, 50));
		}
		else
		{
			MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Plasteel, Rand.Range(10, 50));
		}
		if (Rand.Value < 0.7f)
		{
			MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentIndustrial, Rand.Range(-2, 4));
		}
		else
		{
			MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentSpacer, Rand.Range(-2, 4));
		}
	}

	private void MakeIntoContainer(ThingOwner container, ThingDef def, int count)
	{
		if (count > 0)
		{
			Thing thing = ThingMaker.MakeThing(def);
			thing.stackCount = count;
			container.TryAdd(thing);
		}
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		yield return PawnKindDefOf.AncientSoldier.race;
		yield return PawnKindDefOf.Slave.race;
		yield return PawnKindDefOf.Megascarab.race;
		yield return ThingDefOf.Gold;
		yield return ThingDefOf.Plasteel;
		yield return ThingDefOf.ComponentIndustrial;
		yield return ThingDefOf.ComponentSpacer;
	}
}
