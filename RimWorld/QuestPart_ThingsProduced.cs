using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_ThingsProduced : QuestPartActivable
	{
		public ThingDef def;

		public ThingDef stuff;

		public int count;

		private int produced;

		public override string DescriptionPart => (string)("ThingsProduced".Translate().CapitalizeFirst() + ": ") + produced + " / " + count;

		public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
				{
					yield return hyperlink;
				}
				yield return new Dialog_InfoCard.Hyperlink(def);
			}
		}

		protected override void Enable(SignalArgs receivedArgs)
		{
			base.Enable(receivedArgs);
			produced = 0;
		}

		public override void Notify_ThingsProduced(Pawn actor, List<Thing> things)
		{
			base.Notify_ThingsProduced(actor, things);
			if (base.State != QuestPartState.Enabled)
			{
				return;
			}
			for (int i = 0; i < things.Count; i++)
			{
				Thing innerIfMinified = things[i].GetInnerIfMinified();
				if (innerIfMinified.def == def && innerIfMinified.Stuff == stuff)
				{
					produced += things[i].stackCount;
				}
			}
			if (produced >= count)
			{
				produced = count;
				Complete();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Defs.Look(ref stuff, "stuff");
			Scribe_Values.Look(ref count, "count", 0);
			Scribe_Values.Look(ref produced, "produced", 0);
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			def = ThingDefOf.MealSimple;
			count = 10;
		}
	}
}
