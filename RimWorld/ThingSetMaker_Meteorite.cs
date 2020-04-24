using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Meteorite : ThingSetMaker
	{
		public static List<ThingDef> nonSmoothedMineables = new List<ThingDef>();

		public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

		private const float PreciousMineableMarketValue = 5f;

		public static void Reset()
		{
			nonSmoothedMineables.Clear();
			nonSmoothedMineables.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.mineable && x != ThingDefOf.CollapsedRocks && x != ThingDefOf.RaisedRocks && !x.IsSmoothed));
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			int randomInRange = (parms.countRange ?? MineablesCountRange).RandomInRange;
			ThingDef def = FindRandomMineableDef();
			for (int i = 0; i < randomInRange; i++)
			{
				Building building = (Building)ThingMaker.MakeThing(def);
				building.canChangeTerrainOnDestroyed = false;
				outThings.Add(building);
			}
		}

		private ThingDef FindRandomMineableDef()
		{
			float value = Rand.Value;
			if (value < 0.4f)
			{
				return nonSmoothedMineables.Where((ThingDef x) => !x.building.isResourceRock).RandomElement();
			}
			if (value < 0.75f)
			{
				return nonSmoothedMineables.Where((ThingDef x) => x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f).RandomElement();
			}
			return nonSmoothedMineables.Where((ThingDef x) => x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f).RandomElement();
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return nonSmoothedMineables;
		}
	}
}
