using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Wildlife : MainTabWindow_PawnTable
	{
		protected override PawnTableDef PawnTableDef => PawnTableDefOf.Wildlife;

		protected override IEnumerable<Pawn> Pawns => Find.CurrentMap.mapPawns.AllPawns.Where((Pawn p) => p.Spawned && (p.Faction == null || p.Faction == Faction.OfInsects) && p.AnimalOrWildMan() && !p.Position.Fogged(p.Map) && !p.IsPrisonerInPrisonCell());

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
