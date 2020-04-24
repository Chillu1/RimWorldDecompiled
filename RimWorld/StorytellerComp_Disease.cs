using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_Disease : StorytellerComp
	{
		protected StorytellerCompProperties_Disease Props => (StorytellerCompProperties_Disease)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (!DebugSettings.enableRandomDiseases || target.Tile == -1)
			{
				yield break;
			}
			BiomeDef biome = Find.WorldGrid[target.Tile].biome;
			if (Rand.MTBEventOccurs(biome.diseaseMtbDays * Find.Storyteller.difficulty.diseaseIntervalFactor, 60000f, 1000f))
			{
				IncidentParms parms = GenerateParms(Props.category, target);
				if (UsableIncidentsInCategory(Props.category, parms).TryRandomElementByWeight((IncidentDef d) => biome.CommonalityOfDisease(d), out IncidentDef result))
				{
					yield return new FiringIncident(result, this, parms);
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.category;
		}
	}
}
