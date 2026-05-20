using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GauranlenTreeModeDef : Def
	{
		public GauranlenTreeModeDef previousStage;

		public PawnKindDef pawnKindDef;

		public List<MemeDef> requiredMemes;

		public Vector2 drawPosition;

		public List<StatDef> displayedStats;

		public List<DefHyperlink> hyperlinks;

		private string cachedDescription;

		public string Description
		{
			get
			{
				if (cachedDescription == null)
				{
					cachedDescription = description;
					CompProperties_Spawner compProperties_Spawner = pawnKindDef?.race.GetCompProperties<CompProperties_Spawner>();
					if (compProperties_Spawner != null)
					{
						cachedDescription = cachedDescription + "\n\n" + "DryadProducesResourcesDesc".Translate(NamedArgumentUtility.Named(pawnKindDef, "DRYAD"), GenLabel.ThingLabel(compProperties_Spawner.thingToSpawn, null, compProperties_Spawner.spawnCount).Named("RESOURCES"), compProperties_Spawner.spawnIntervalRange.max.ToStringTicksToPeriod().Named("DURATION")).Resolve().CapitalizeFirst();
					}
				}
				return cachedDescription;
			}
		}
	}
}
