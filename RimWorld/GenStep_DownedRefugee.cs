using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class GenStep_DownedRefugee : GenStep_Scatterer
	{
		public override int SeedPart => 931842770;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (base.CanScatterAt(c, map))
			{
				return c.Standable(map);
			}
			return false;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
		{
			Pawn pawn;
			if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
			{
				pawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things[0]);
			}
			else
			{
				DownedRefugeeComp component = map.Parent.GetComponent<DownedRefugeeComp>();
				pawn = ((component == null || !component.pawn.Any) ? DownedRefugeeQuestUtility.GenerateRefugee(map.Tile) : component.pawn.Take(component.pawn[0]));
			}
			HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
			HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
			GenSpawn.Spawn(pawn, loc, map);
			pawn.mindState.WillJoinColonyIfRescued = true;
			MapGenerator.rootsToUnfog.Add(loc);
			MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
		}
	}
}
