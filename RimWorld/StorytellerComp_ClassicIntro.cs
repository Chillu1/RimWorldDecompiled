using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class StorytellerComp_ClassicIntro : StorytellerComp
{
	protected int IntervalsPassed => Find.TickManager.TicksGame / 1000;

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		if (target != Find.Maps.Find((Map x) => x.IsPlayerHome))
		{
			yield break;
		}
		if (IntervalsPassed == 150)
		{
			IncidentDef visitorGroup = IncidentDefOf.VisitorGroup;
			if (visitorGroup.TargetAllowed(target))
			{
				FiringIncident firingIncident = new FiringIncident(visitorGroup, this);
				firingIncident.parms.target = target;
				firingIncident.parms.points = Rand.Range(40, 100);
				yield return firingIncident;
			}
		}
		if (IntervalsPassed == 204)
		{
			IncidentCategoryDef threatCategory = (Find.Storyteller.difficulty.allowIntroThreats ? IncidentCategoryDefOf.ThreatSmall : IncidentCategoryDefOf.Misc);
			if (DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef def) => def.TargetAllowed(target) && def.category == threatCategory).TryRandomElementByWeight((IncidentDef def) => IncidentChanceFinal(def, target), out var result))
			{
				FiringIncident firingIncident2 = new FiringIncident(result, this);
				firingIncident2.parms = StorytellerUtility.DefaultParmsNow(result.category, target);
				yield return firingIncident2;
			}
		}
		if (IntervalsPassed == 264 && DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef def) => def.TargetAllowed(target) && def.category == IncidentCategoryDefOf.Misc).TryRandomElementByWeight((IncidentDef def) => IncidentChanceFinal(def, target), out var result2))
		{
			FiringIncident firingIncident3 = new FiringIncident(result2, this);
			firingIncident3.parms = StorytellerUtility.DefaultParmsNow(result2.category, target);
			yield return firingIncident3;
		}
		if (IntervalsPassed != 324)
		{
			yield break;
		}
		IncidentDef incidentDef = IncidentDefOf.RaidEnemy;
		if (!Find.Storyteller.difficulty.allowIntroThreats)
		{
			incidentDef = DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef def) => def.TargetAllowed(target) && def.category == IncidentCategoryDefOf.Misc).RandomElementByWeightWithFallback((IncidentDef def) => IncidentChanceFinal(def, target));
		}
		if (incidentDef != null && incidentDef.TargetAllowed(target))
		{
			FiringIncident firingIncident4 = new FiringIncident(incidentDef, this);
			firingIncident4.parms = GenerateParms(incidentDef.category, target);
			firingIncident4.parms.points = 40f;
			firingIncident4.parms.raidForceOneDowned = true;
			firingIncident4.parms.raidNeverFleeIndividual = true;
			yield return firingIncident4;
		}
	}
}
