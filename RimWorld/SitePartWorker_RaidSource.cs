using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker_RaidSource : SitePartWorker_Outpost
{
	public override void SitePartWorkerTick(SitePart sitePart)
	{
		base.SitePartWorkerTick(sitePart);
		if (sitePart.lastRaidTick != -1 && !((float)Find.TickManager.TicksGame > (float)sitePart.lastRaidTick + 90000f))
		{
			return;
		}
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].IsPlayerHome && sitePart.site.IsHashIntervalTick(2500) && Rand.MTBEventOccurs(QuestTuning.PointsToRaidSourceRaidsMTBDaysCurve.Evaluate(sitePart.parms.threatPoints), 60000f, 2500f))
			{
				StartRaid(maps[i], sitePart);
			}
		}
	}

	private void StartRaid(Map map, SitePart sitePart)
	{
		IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
		incidentParms.forced = true;
		incidentParms.points *= 0.6f;
		if (IncidentDefOf.RaidEnemy.Worker.CanFireNow(incidentParms))
		{
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
			sitePart.lastRaidTick = Find.TickManager.TicksGame;
		}
	}

	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		int enemiesCount = GetEnemiesCount(part.site, part.parms);
		float num = QuestTuning.PointsToRaidSourceRaidsMTBDaysCurve.Evaluate(part.parms.threatPoints);
		outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
		outExtraDescriptionRules.Add(new Rule_String("mtbDays", ((int)(num * 60000f)).ToStringTicksToPeriod(allowSeconds: true, shortForm: false, canUseDecimals: false)));
		outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
	}
}
