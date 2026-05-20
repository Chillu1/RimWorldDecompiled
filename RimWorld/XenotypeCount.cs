using Verse;

namespace RimWorld
{
	public class XenotypeCount : StartingPawnCount
	{
		public XenotypeDef xenotype;

		public DevelopmentalStage allowedDevelopmentalStages = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;

		[MustTranslate]
		public string description;

		public override string Summary => "PawnCount".Translate(count.Named("COUNT"), (description.NullOrEmpty() ? xenotype.label : description).Named("KINDLABEL"));

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref xenotype, "xenotype");
			Scribe_Values.Look(ref allowedDevelopmentalStages, "allowedDevelopmentalStages", DevelopmentalStage.None);
			Scribe_Values.Look(ref description, "description");
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode ^= allowedDevelopmentalStages.GetHashCode();
			hashCode ^= description.GetHashCodeSafe();
			if (xenotype != null)
			{
				hashCode ^= xenotype.GetHashCode();
			}
			return hashCode;
		}
	}
}
