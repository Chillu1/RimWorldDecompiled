using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoyalTitlePermitWorker
	{
		public RoyalTitlePermitDef def;

		public virtual IEnumerable<Gizmo> GetPawnGizmos(Pawn pawn, Faction faction)
		{
			return null;
		}

		public virtual IEnumerable<DiaOption> GetFactionCommDialogOptions(Map map, Pawn pawn, Faction factionInFavor)
		{
			return null;
		}

		public virtual IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
		{
			return null;
		}
	}
}
