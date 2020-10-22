using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Animals : MainTabWindow_PawnTable
	{
		protected override PawnTableDef PawnTableDef => PawnTableDefOf.Animals;

		protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
			where p.RaceProps.Animal
			select p;

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
