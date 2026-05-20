using Verse;

namespace RimWorld
{
	public class PawnKindCount : StartingPawnCount
	{
		public PawnKindDef kindDef;

		public override string Summary => "PawnCount".Translate(count.Named("COUNT"), ((count > 1) ? kindDef.GetLabelPlural(count) : kindDef.label).Named("KINDLABEL"));

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref kindDef, "kindDef");
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode ^= count;
			hashCode ^= (requiredAtStart ? 1 : 0);
			if (kindDef != null)
			{
				hashCode ^= kindDef.GetHashCode();
			}
			return hashCode;
		}
	}
}
