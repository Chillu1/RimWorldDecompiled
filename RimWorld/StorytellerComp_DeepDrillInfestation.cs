using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorytellerComp_DeepDrillInfestation : StorytellerComp
{
	private static List<Thing> tmpDrills = new List<Thing>();

	protected StorytellerCompProperties_DeepDrillInfestation Props => (StorytellerCompProperties_DeepDrillInfestation)props;

	private float DeepDrillInfestationMTBDaysPerDrill
	{
		get
		{
			Difficulty difficulty = Find.Storyteller.difficulty;
			if (difficulty.deepDrillInfestationChanceFactor <= 0f)
			{
				return -1f;
			}
			return Props.baseMtbDaysPerDrill / difficulty.deepDrillInfestationChanceFactor;
		}
	}

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		Map map = (Map)target;
		tmpDrills.Clear();
		DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, tmpDrills);
		if (!tmpDrills.Any())
		{
			yield break;
		}
		float mtb = DeepDrillInfestationMTBDaysPerDrill;
		if (mtb < 0f)
		{
			yield break;
		}
		for (int i = 0; i < tmpDrills.Count; i++)
		{
			if (Rand.MTBEventOccurs(mtb, 60000f, 1000f))
			{
				IncidentParms parms = GenerateParms(IncidentCategoryDefOf.DeepDrillInfestation, target);
				if (UsableIncidentsInCategory(IncidentCategoryDefOf.DeepDrillInfestation, parms).TryRandomElement(out var result))
				{
					yield return new FiringIncident(result, this, parms);
				}
			}
		}
	}
}
