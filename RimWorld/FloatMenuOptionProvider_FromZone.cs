using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class FloatMenuOptionProvider_FromZone : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
	{
		if (context.ClickedZone == null)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}
		return context.ClickedZone.GetFloatMenuOptions(context.FirstSelectedPawn);
	}
}
