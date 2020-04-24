using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_PlantsHarvested : QuestPartActivable
	{
		public ThingDef plant;

		public int count;

		private int harvested;

		public override string DescriptionPart => (string)("PlantsHarvested".Translate().CapitalizeFirst() + ": ") + harvested + " / " + count;

		public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
				{
					yield return hyperlink;
				}
				yield return new Dialog_InfoCard.Hyperlink(plant);
			}
		}

		protected override void Enable(SignalArgs receivedArgs)
		{
			base.Enable(receivedArgs);
			harvested = 0;
		}

		public override void Notify_PlantHarvested(Pawn actor, Thing harvested)
		{
			base.Notify_PlantHarvested(actor, harvested);
			if (base.State == QuestPartState.Enabled && harvested.def == plant)
			{
				this.harvested += harvested.stackCount;
				if (this.harvested >= count)
				{
					this.harvested = count;
					Complete();
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref plant, "plant");
			Scribe_Values.Look(ref count, "count", 0);
			Scribe_Values.Look(ref harvested, "harvested", 0);
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			plant = ThingDefOf.RawPotatoes;
			count = 10;
		}
	}
}
