using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AllThingsHacked : QuestPart_Filter
	{
		public List<Thing> things = new List<Thing>();

		protected override bool Pass(SignalArgs args)
		{
			if (things.NullOrEmpty())
			{
				return false;
			}
			foreach (Thing thing in things)
			{
				if (!thing.IsHackable() || !thing.IsHacked())
				{
					return false;
				}
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref things, "things", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				things.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
