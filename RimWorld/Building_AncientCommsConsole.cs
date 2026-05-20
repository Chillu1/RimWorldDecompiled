using Verse;

namespace RimWorld
{
	public class Building_AncientCommsConsole : Building
	{
		public override string GetInspectString()
		{
			return base.GetInspectString() + ("\n" + "LinkedTo".Translate() + ": " + "SupplySatellite".Translate());
		}
	}
}
