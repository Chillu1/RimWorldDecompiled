using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_PawnKilled : QuestPart
	{
		public Faction faction;

		public MapParent mapParent;

		public string outSignal;

		public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
		{
			base.Notify_PawnKilled(pawn, dinfo);
			if (pawn.Faction == faction && pawn.MapHeld != null && pawn.MapHeld.Parent == mapParent)
			{
				Find.SignalManager.SendSignal(new Signal(outSignal));
			}
		}

		public override void Notify_FactionRemoved(Faction f)
		{
			if (faction == f)
			{
				faction = null;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref faction, "faction");
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_Values.Look(ref outSignal, "outSignal");
		}
	}
}
