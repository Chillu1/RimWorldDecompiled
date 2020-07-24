using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ClassicIntro : StorytellerComp
	{
		protected int IntervalsPassed => Find.TickManager.TicksGame / 1000;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			_003C_003Ec__DisplayClass2_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass2_0();
			CS_0024_003C_003E8__locals0.target = target;
			if (CS_0024_003C_003E8__locals0.target != Find.Maps.Find((Map x) => x.IsPlayerHome))
			{
				yield break;
			}
			if (IntervalsPassed == 150)
			{
				IncidentDef visitorGroup = IncidentDefOf.VisitorGroup;
				if (visitorGroup.TargetAllowed(CS_0024_003C_003E8__locals0.target))
				{
					FiringIncident firingIncident = new FiringIncident(visitorGroup, this);
					firingIncident.parms.target = CS_0024_003C_003E8__locals0.target;
					firingIncident.parms.points = Rand.Range(40, 100);
					yield return firingIncident;
				}
			}
			if (IntervalsPassed == 204)
			{
				_003C_003Ec__DisplayClass2_0 _003C_003Ec__DisplayClass2_ = CS_0024_003C_003E8__locals0;
				IncidentCategoryDef threatCategory = Find.Storyteller.difficulty.allowIntroThreats ? IncidentCategoryDefOf.ThreatSmall : IncidentCategoryDefOf.Misc;
				if (DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef def) => def.TargetAllowed(_003C_003Ec__DisplayClass2_.target) && def.category == threatCategory).TryRandomElementByWeight(base.IncidentChanceFinal, out IncidentDef result))
				{
					FiringIncident firingIncident2 = new FiringIncident(result, this);
					firingIncident2.parms = StorytellerUtility.DefaultParmsNow(result.category, _003C_003Ec__DisplayClass2_.target);
					yield return firingIncident2;
				}
			}
			if (IntervalsPassed == 264 && DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef def) => def.TargetAllowed(CS_0024_003C_003E8__locals0.target) && def.category == IncidentCategoryDefOf.Misc).TryRandomElementByWeight(base.IncidentChanceFinal, out IncidentDef result2))
			{
				FiringIncident firingIncident3 = new FiringIncident(result2, this);
				firingIncident3.parms = StorytellerUtility.DefaultParmsNow(result2.category, CS_0024_003C_003E8__locals0.target);
				yield return firingIncident3;
			}
			if (IntervalsPassed != 324)
			{
				yield break;
			}
			IncidentDef incidentDef = IncidentDefOf.RaidEnemy;
			if (!Find.Storyteller.difficulty.allowIntroThreats)
			{
				incidentDef = DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef def) => def.TargetAllowed(CS_0024_003C_003E8__locals0.target) && def.category == IncidentCategoryDefOf.Misc).RandomElementByWeightWithFallback(base.IncidentChanceFinal);
			}
			if (incidentDef != null && incidentDef.TargetAllowed(CS_0024_003C_003E8__locals0.target))
			{
				FiringIncident firingIncident4 = new FiringIncident(incidentDef, this);
				firingIncident4.parms = GenerateParms(incidentDef.category, CS_0024_003C_003E8__locals0.target);
				firingIncident4.parms.points = 40f;
				firingIncident4.parms.raidForceOneIncap = true;
				firingIncident4.parms.raidNeverFleeIndividual = true;
				yield return firingIncident4;
			}
		}
	}
}
