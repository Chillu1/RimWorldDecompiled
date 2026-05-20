using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompLeavingsLetter : ThingComp
	{
		private static List<Thing> tmpLeavings = new List<Thing>();

		public CompProperties_LeavingsLetter Props => (CompProperties_LeavingsLetter)props;

		public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
		{
			if (leavings.NullOrEmpty())
			{
				return;
			}
			List<Thing> list = tmpLeavings;
			object collection;
			if (Props.leavingsFilter == null)
			{
				collection = leavings;
			}
			else
			{
				collection = leavings.Where((Thing t) => Props.leavingsFilter.Allows(t.def));
			}
			list.AddRange((IEnumerable<Thing>)collection);
			if (tmpLeavings.Count > 0)
			{
				Find.LetterStack.ReceiveLetter(Props.letterLabel.Formatted(tmpLeavings[0].Named("LEAVINGS1")), Props.letterText.Formatted(tmpLeavings[0].Named("LEAVINGS1"), parent.Named("PARENT")), Props.letterDef, tmpLeavings);
				tmpLeavings.Clear();
			}
		}
	}
}
