using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_RandomMain : StorytellerComp
	{
		protected StorytellerCompProperties_RandomMain Props => (StorytellerCompProperties_RandomMain)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (!Rand.MTBEventOccurs(Props.mtbDays, 60000f, 1000f))
			{
				yield break;
			}
			bool flag = target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon);
			List<IncidentCategoryDef> list = new List<IncidentCategoryDef>();
			IncidentParms parms;
			IncidentDef result;
			while (true)
			{
				IncidentCategoryDef incidentCategoryDef = ChooseRandomCategory(target, list);
				parms = GenerateParms(incidentCategoryDef, target);
				if (UsableIncidentsInCategory(incidentCategoryDef, parms).TryRandomElementByWeight(base.IncidentChanceFinal, out result))
				{
					break;
				}
				list.Add(incidentCategoryDef);
				if (list.Count >= Props.categoryWeights.Count)
				{
					yield break;
				}
			}
			if (!(Props.skipThreatBigIfRaidBeacon && flag) || result.category != IncidentCategoryDefOf.ThreatBig)
			{
				yield return new FiringIncident(result, this, parms);
			}
		}

		private IncidentCategoryDef ChooseRandomCategory(IIncidentTarget target, List<IncidentCategoryDef> skipCategories)
		{
			if (!skipCategories.Contains(IncidentCategoryDefOf.ThreatBig))
			{
				int num = Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick;
				if (target.StoryState.LastThreatBigTick >= 0 && (float)num > 60000f * Props.maxThreatBigIntervalDays)
				{
					return IncidentCategoryDefOf.ThreatBig;
				}
			}
			return Props.categoryWeights.Where((IncidentCategoryEntry cw) => !skipCategories.Contains(cw.category)).RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
		}

		public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat, target);
			if (incidentParms.points >= 0f)
			{
				incidentParms.points *= Props.randomPointsFactorRange.RandomInRange;
			}
			return incidentParms;
		}
	}
}
