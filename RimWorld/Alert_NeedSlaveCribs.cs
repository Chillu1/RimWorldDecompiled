using Verse;

namespace RimWorld;

public class Alert_NeedSlaveCribs : Alert
{
	public Alert_NeedSlaveCribs()
	{
		defaultLabel = "AlertNeedSlaveCribs".Translate();
		defaultExplanation = "AlertNeedSlaveCribsDesc".Translate();
		defaultPriority = AlertPriority.High;
		requireIdeology = true;
		requireBiotech = true;
	}

	public override AlertReport GetReport()
	{
		foreach (Map map in Find.Maps)
		{
			if (map.IsPlayerHome)
			{
				Alert_NeedSlaveBeds.CheckSlaveBeds(map, out var _, out var enoughBabyCribs);
				if (!enoughBabyCribs)
				{
					return true;
				}
			}
		}
		return false;
	}
}
