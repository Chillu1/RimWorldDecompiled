using Verse;

namespace RimWorld;

public class MutantCount : StartingPawnCount
{
	public MutantDef mutant;

	public DevelopmentalStage allowedDevelopmentalStages = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;

	[MustTranslate]
	public string description;

	public override string Summary => "PawnCount".Translate(count.Named("COUNT"), (description ?? mutant?.label ?? Find.ActiveLanguageWorker.Pluralize("Colonist".Translate(), count)).Named("KINDLABEL"));

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref mutant, "mutant");
		Scribe_Values.Look(ref allowedDevelopmentalStages, "allowedDevelopmentalStages", DevelopmentalStage.None);
		Scribe_Values.Look(ref description, "description");
	}

	public override int GetHashCode()
	{
		int hashCode = base.GetHashCode();
		hashCode ^= allowedDevelopmentalStages.GetHashCode();
		hashCode ^= description.GetHashCodeSafe();
		if (mutant != null)
		{
			hashCode ^= mutant.GetHashCode();
		}
		return hashCode;
	}
}
