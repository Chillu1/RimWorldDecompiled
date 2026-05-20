using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_BuiltNearSettlement : QuestPart_Filter
	{
		public MapParent mapParent;

		public Faction settlementFaction;

		protected override bool Pass(SignalArgs args)
		{
			if (settlementFaction == null)
			{
				return false;
			}
			return args.GetArg<Thing>("SUBJECT").MapHeld.Parent == mapParent;
		}

		public override void Notify_FactionRemoved(Faction faction)
		{
			base.Notify_FactionRemoved(faction);
			if (settlementFaction == faction)
			{
				settlementFaction = null;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_References.Look(ref settlementFaction, "settlementFaction");
		}
	}
}
