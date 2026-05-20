using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_Filter_AnyOnTransporterCapableOfHacking : QuestPart_Filter
{
	public Thing transporter;

	protected override bool Pass(SignalArgs args)
	{
		foreach (Thing item in (IEnumerable<Thing>)transporter.TryGetComp<CompTransporter>().GetDirectlyHeldThings())
		{
			if (item is Pawn pawn && HackUtility.IsCapableOfHacking(pawn))
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref transporter, "transporter");
	}
}
