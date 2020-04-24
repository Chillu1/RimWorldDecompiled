using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class GenStep_ConditionCauser : GenStep_Scatterer
	{
		private const int Size = 10;

		private GenStepParams currentParams;

		public override int SeedPart => 1068345639;

		public override void Generate(Map map, GenStepParams parms)
		{
			currentParams = parms;
			count = 1;
			base.Generate(map, parms);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
		{
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();
			CellRect rect = CellRect.CenteredOn(loc, 10, 10).ClipInsideMap(map);
			SitePart sitePart = currentParams.sitePart;
			sitePart.conditionCauserWasSpawned = true;
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			resolveParams.conditionCauser = sitePart.conditionCauser;
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("conditionCauserRoom", resolveParams);
			RimWorld.BaseGen.BaseGen.Generate();
		}
	}
}
