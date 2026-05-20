using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class FloatMenuOptionProvider_FromLord : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
	{
		Lord lord = clickedPawn.GetLord();
		if (lord == null)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}
		return lord.CurLordToil.ExtraFloatMenuOptions(clickedPawn, context.FirstSelectedPawn);
	}
}
