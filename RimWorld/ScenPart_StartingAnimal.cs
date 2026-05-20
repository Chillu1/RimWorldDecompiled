using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_StartingAnimal : ScenPart
{
	private PawnKindDef animalKind;

	private int count = 1;

	private float bondToRandomPlayerPawnChance = 0.5f;

	private string countBuf;

	private static readonly List<Pair<int, float>> PetCountChances = new List<Pair<int, float>>
	{
		new Pair<int, float>(1, 20f),
		new Pair<int, float>(2, 10f),
		new Pair<int, float>(3, 5f),
		new Pair<int, float>(4, 3f),
		new Pair<int, float>(5, 1f),
		new Pair<int, float>(6, 1f),
		new Pair<int, float>(7, 1f),
		new Pair<int, float>(8, 1f),
		new Pair<int, float>(9, 1f),
		new Pair<int, float>(10, 0.1f),
		new Pair<int, float>(11, 0.1f),
		new Pair<int, float>(12, 0.1f),
		new Pair<int, float>(13, 0.1f),
		new Pair<int, float>(14, 0.1f)
	};

	public const float VeneratedAnimalWeight = 8f;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref animalKind, "animalKind");
		Scribe_Values.Look(ref count, "count", 0);
		Scribe_Values.Look(ref bondToRandomPlayerPawnChance, "bondToRandomPlayerPawnChance", 0f);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f + 1f);
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(scenPartRect.TopHalf());
		listing_Standard.ColumnWidth = scenPartRect.width;
		listing_Standard.TextFieldNumeric(ref count, ref countBuf, 1f);
		listing_Standard.End();
		if (!Widgets.ButtonText(scenPartRect.BottomHalf(), CurrentAnimalLabel().CapitalizeFirst()))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.Add(new FloatMenuOption("RandomPet".Translate().CapitalizeFirst(), delegate
		{
			animalKind = null;
		}));
		foreach (PawnKindDef item in PossibleAnimals(checkForTamer: false))
		{
			PawnKindDef localKind = item;
			list.Add(new FloatMenuOption(localKind.LabelCap, delegate
			{
				animalKind = localKind;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	private IEnumerable<PawnKindDef> PossibleAnimals(bool checkForTamer = true)
	{
		return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef td) => td.RaceProps.Animal && (!checkForTamer || CanKeepPetTame(td)));
	}

	private static bool CanKeepPetTame(PawnKindDef def)
	{
		int level = Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount).MaxBy((Pawn c) => c.skills.GetSkill(SkillDefOf.Animals).Level).skills.GetSkill(SkillDefOf.Animals).Level;
		float statValueAbstract = def.race.GetStatValueAbstract(StatDefOf.MinimumHandlingSkill);
		return (float)level >= statValueAbstract;
	}

	private IEnumerable<PawnKindDef> RandomPets()
	{
		IEnumerable<PawnKindDef> enumerable = from td in PossibleAnimals()
			where td.RaceProps.petness > 0f
			select td;
		if (enumerable.EnumerableNullOrEmpty())
		{
			enumerable = from td in PossibleAnimals(checkForTamer: false)
				where td.RaceProps.petness > 0f
				select td;
		}
		return enumerable;
	}

	private string CurrentAnimalLabel()
	{
		if (animalKind == null)
		{
			return "RandomPet".TranslateSimple();
		}
		return animalKind.label;
	}

	public override string Summary(Scenario scen)
	{
		return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
	}

	public override IEnumerable<string> GetSummaryListEntries(string tag)
	{
		if (tag == "PlayerStartsWith")
		{
			yield return CurrentAnimalLabel().CapitalizeFirst() + " x" + count;
		}
	}

	public override void Randomize()
	{
		if (Rand.Value < 0.5f)
		{
			animalKind = null;
		}
		else
		{
			animalKind = PossibleAnimals(checkForTamer: false).RandomElement();
		}
		count = PetCountChances.RandomElementByWeight((Pair<int, float> pa) => pa.Second).First;
		bondToRandomPlayerPawnChance = 0f;
	}

	public override bool TryMerge(ScenPart other)
	{
		if (other is ScenPart_StartingAnimal scenPart_StartingAnimal && scenPart_StartingAnimal.animalKind == animalKind)
		{
			count += scenPart_StartingAnimal.count;
			return true;
		}
		return false;
	}

	private float PetWeight(PawnKindDef animal)
	{
		Ideo ideo = Find.GameInitData.playerFaction.ideos?.PrimaryIdeo;
		if (ideo != null)
		{
			foreach (ThingDef veneratedAnimal in ideo.VeneratedAnimals)
			{
				if (veneratedAnimal == animal.race)
				{
					return 8f;
				}
			}
		}
		return animal.RaceProps.petness;
	}

	public override IEnumerable<Thing> PlayerStartingThings()
	{
		for (int i = 0; i < count; i++)
		{
			PawnKindDef kindDef = ((animalKind == null) ? RandomPets().RandomElementByWeightWithFallback((PawnKindDef td) => PetWeight(td)) : animalKind);
			Pawn animal = PawnGenerator.GeneratePawn(kindDef, Faction.OfPlayer);
			if (animal.Name == null || animal.Name.Numerical)
			{
				animal.Name = PawnBioAndNameGenerator.GeneratePawnName(animal);
			}
			if (Rand.Value < bondToRandomPlayerPawnChance && animal.training.CanAssignToTrain(TrainableDefOf.Obedience).Accepted)
			{
				Pawn pawn = (from p in Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount)
					where TrainableUtility.CanBeMaster(p, animal, checkSpawned: false) && !p.story.traits.HasTrait(TraitDefOf.Psychopath) && !p.Inhumanized()
					select p).RandomElementWithFallback();
				if (pawn != null)
				{
					animal.training.Train(TrainableDefOf.Obedience, null, complete: true);
					animal.training.SetWantedRecursive(TrainableDefOf.Obedience, checkOn: true);
					animal.playerSettings.Master = pawn;
					if (pawn.Ideo == null || pawn.Ideo.MemberWillingToDo(new HistoryEvent(HistoryEventDefOf.Bonded, pawn.Named(HistoryEventArgsNames.Doer))))
					{
						pawn.relations.AddDirectRelation(PawnRelationDefOf.Bond, animal);
					}
				}
			}
			yield return animal;
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((animalKind != null) ? animalKind.GetHashCode() : 0) ^ count ^ bondToRandomPlayerPawnChance.GetHashCode();
	}
}
