using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetSiteThreatPoints : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<Site> site;

	public SlateRef<IEnumerable<SitePartDefWithParams>> sitePartsParams;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (site.GetValue(slate) != null)
		{
			slate.Set(storeAs.GetValue(slate), site.GetValue(slate).ActualThreatPoints);
			return;
		}
		float num = 0f;
		IEnumerable<SitePartDefWithParams> value = sitePartsParams.GetValue(slate);
		if (value != null)
		{
			foreach (SitePartDefWithParams item in value)
			{
				num += item.parms.threatPoints;
			}
		}
		slate.Set(storeAs.GetValue(slate), num);
	}
}
