using Verse;

namespace RimWorld
{
	public class Building_Art : Building
	{
		public override string GetInspectString()
		{
			return base.GetInspectString() + ("\n" + StatDefOf.Beauty.LabelCap + ": " + StatDefOf.Beauty.ValueToString(this.GetStatValue(StatDefOf.Beauty)));
		}
	}
}
