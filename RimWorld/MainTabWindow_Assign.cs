using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class MainTabWindow_Assign : MainTabWindow_PawnTable
{
	protected override PawnTableDef PawnTableDef => PawnTableDefOf.Assign;

	protected override IEnumerable<Pawn> Pawns => PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Where((Pawn pawn) => !pawn.DevelopmentalStage.Baby());
}
