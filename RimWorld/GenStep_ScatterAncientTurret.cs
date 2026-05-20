using Verse;

namespace RimWorld
{
	public class GenStep_ScatterAncientTurret : GenStep_Scatterer
	{
		private const int ShellScatterRadius = 10;

		private static readonly IntRange MaxShellsRange = new IntRange(1, 2);

		public override int SeedPart => 344678634;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (ModLister.CheckIdeology("Scatter ancient turret") && Find.World.HasCaves(map.Tile))
			{
				count = 1;
				warnOnFail = false;
				base.Generate(map, parms);
			}
		}

		protected override bool CanScatterAt(IntVec3 loc, Map map)
		{
			if (!base.CanScatterAt(loc, map) || !loc.InBounds(map))
			{
				return false;
			}
			if (loc.GetTerrain(map).IsWater)
			{
				return false;
			}
			RoofDef roof = loc.GetRoof(map);
			if (roof == null || !roof.isNatural)
			{
				return false;
			}
			if (loc.GetEdifice(map) != null)
			{
				return false;
			}
			if (!map.reachability.CanReachMapEdge(loc, TraverseParms.For(TraverseMode.NoPassClosedDoors)))
			{
				return false;
			}
			foreach (IntVec3 item in GenRadial.RadialCellsAround(loc, 2f, useCenter: false))
			{
				if (item.InBounds(map) && !item.Roofed(map) && item.GetEdifice(map) == null && GenSight.LineOfSight(loc, item, map))
				{
					return true;
				}
			}
			return false;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
		{
			ScatterDebrisUtility.ScatterFilthAroundThing(GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientSecurityTurret), loc, map, Rot4.North), map, ThingDefOf.Filth_DriedBlood, 0.5f, 1, 3);
		}
	}
}
