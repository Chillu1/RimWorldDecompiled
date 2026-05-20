using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_Filter_AnyOnTransporter : QuestPart_Filter
{
	public Thing transporter;

	public List<Pawn> pawns;

	protected override bool Pass(SignalArgs args)
	{
		if (pawns.NullOrEmpty())
		{
			return false;
		}
		ThingOwner directlyHeldThings = transporter.TryGetComp<CompTransporter>().GetDirectlyHeldThings();
		foreach (Pawn pawn in pawns)
		{
			if (directlyHeldThings.Contains(pawn))
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref transporter, "transporter");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
