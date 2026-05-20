using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class TaleData_Pawn : TaleData
{
	public Pawn pawn;

	public PawnKindDef kind;

	public Faction faction;

	public Gender gender;

	public int age;

	public int chronologicalAge;

	public string relationInfo;

	public bool everBeenColonistOrTameAnimal;

	public bool everBeenQuestLodger;

	public bool isFactionLeader;

	public List<RoyalTitle> royalTitles;

	public Name name;

	public string title;

	public ThingDef primaryEquipment;

	public ThingDef notableApparel;

	private List<Faction> tmpFactions;

	private List<RoyalTitleDef> tmpRoyalTitles;

	public override void ExposeData()
	{
		Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
		Scribe_Defs.Look(ref kind, "kind");
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref gender, "gender", Gender.None);
		Scribe_Values.Look(ref age, "age", 0);
		Scribe_Values.Look(ref chronologicalAge, "chronologicalAge", 0);
		Scribe_Values.Look(ref relationInfo, "relationInfo");
		Scribe_Values.Look(ref everBeenColonistOrTameAnimal, "everBeenColonistOrTameAnimal", defaultValue: false);
		Scribe_Values.Look(ref everBeenQuestLodger, "everBeenQuestLodger", defaultValue: false);
		Scribe_Values.Look(ref isFactionLeader, "isFactionLeader", defaultValue: false);
		Scribe_Collections.Look(ref royalTitles, "royalTitles", LookMode.Deep);
		Scribe_Deep.Look(ref name, "name");
		Scribe_Values.Look(ref title, "title");
		Scribe_Defs.Look(ref primaryEquipment, "peq");
		Scribe_Defs.Look(ref notableApparel, "app");
	}

	public override IEnumerable<Rule> GetRules(string prefix, Dictionary<string, string> constants = null)
	{
		return GrammarUtility.RulesForPawn(prefix, name, title, kind, gender, faction, age, chronologicalAge, relationInfo, everBeenColonistOrTameAnimal, everBeenQuestLodger, isFactionLeader, royalTitles, cubeInterest: false, string.Empty, constants);
	}

	public static TaleData_Pawn GenerateFrom(Pawn pawn)
	{
		TaleData_Pawn taleData_Pawn = new TaleData_Pawn();
		taleData_Pawn.pawn = pawn;
		taleData_Pawn.kind = pawn.kindDef;
		taleData_Pawn.faction = pawn.Faction;
		taleData_Pawn.gender = (pawn.RaceProps.hasGenders ? pawn.gender : Gender.None);
		taleData_Pawn.age = pawn.ageTracker.AgeBiologicalYears;
		taleData_Pawn.chronologicalAge = pawn.ageTracker.AgeChronologicalYears;
		taleData_Pawn.everBeenColonistOrTameAnimal = PawnUtility.EverBeenColonistOrTameAnimal(pawn);
		taleData_Pawn.everBeenQuestLodger = PawnUtility.EverBeenQuestLodger(pawn);
		taleData_Pawn.isFactionLeader = pawn.Faction != null && pawn.Faction.leader == pawn;
		if (pawn.royalty != null)
		{
			taleData_Pawn.royalTitles = new List<RoyalTitle>();
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
			{
				taleData_Pawn.royalTitles.Add(new RoyalTitle(item));
			}
		}
		TaggedString text = "";
		PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
		taleData_Pawn.relationInfo = text.Resolve();
		if (pawn.story != null)
		{
			taleData_Pawn.title = pawn.story.title;
		}
		if (pawn.RaceProps.Humanlike)
		{
			taleData_Pawn.name = pawn.Name;
			if (pawn.equipment.Primary != null)
			{
				taleData_Pawn.primaryEquipment = pawn.equipment.Primary.def;
			}
			if (pawn.apparel.WornApparel.TryRandomElement(out var result))
			{
				taleData_Pawn.notableApparel = result.def;
			}
		}
		return taleData_Pawn;
	}

	public static TaleData_Pawn GenerateRandom(bool humanLike = false)
	{
		PawnKindDef obj = (humanLike ? PawnKindDefOf.Drifter : DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef x) => x.Isnt<CreepJoinerFormKindDef>()).RandomElement());
		Faction faction = FactionUtility.DefaultFactionFrom(obj.defaultFactionDef);
		return GenerateFrom(PawnGenerator.GeneratePawn(obj, faction));
	}
}
