using Verse;

namespace RimWorld;

public class QuestPart_Filter_AllThingsHackedOrDestroyed : QuestPart_Filter_AllThingsHacked
{
	protected override bool Pass(SignalArgs args)
	{
		if (things.Count == 0)
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i].DestroyedOrNull() || things[i].IsHacked())
			{
				num++;
			}
		}
		return num >= things.Count;
	}
}
