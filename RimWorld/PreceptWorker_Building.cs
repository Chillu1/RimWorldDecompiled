using Verse;

namespace RimWorld;

public class PreceptWorker_Building : PreceptWorker
{
	public override AcceptanceReport CanUse(ThingDef def, Ideo ideo, FactionDef generatingFor)
	{
		bool flag = false;
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item is Precept_Building { ThingDef: not null } precept_Building && precept_Building.ThingDef.isAltar)
			{
				flag = true;
				break;
			}
		}
		if (flag && def.isAltar)
		{
			return new AcceptanceReport("IdeoAlreadyHasAltar".Translate());
		}
		if (!flag)
		{
			return def.isAltar;
		}
		if (!def.isAltar)
		{
			return true;
		}
		if (flag && def.isAltar)
		{
			return false;
		}
		return true;
	}
}
