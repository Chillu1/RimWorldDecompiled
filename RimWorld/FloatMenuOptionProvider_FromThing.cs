using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class FloatMenuOptionProvider_FromThing : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => true;

	protected override bool CanSelfTarget => true;

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (context.IsMultiselect)
		{
			foreach (FloatMenuOption multiSelectFloatMenuOption in clickedThing.GetMultiSelectFloatMenuOptions(context.ValidSelectedPawns))
			{
				yield return multiSelectFloatMenuOption;
			}
			yield break;
		}
		foreach (FloatMenuOption floatMenuOption in clickedThing.GetFloatMenuOptions(context.FirstSelectedPawn))
		{
			yield return floatMenuOption;
		}
	}
}
