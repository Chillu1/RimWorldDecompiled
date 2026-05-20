using System;

namespace Verse;

public class Command_ActionWithLimitedUseCount : Command_Action
{
	public Func<int> usesLeftGetter;

	public Func<int> maxUsesGetter;

	public override string TopRightLabel => usesLeftGetter() + " / " + maxUsesGetter();

	public void UpdateUsesLeft()
	{
		if (usesLeftGetter() == 0)
		{
			Disable("CommandNoUsesLeft".Translate());
		}
	}
}
