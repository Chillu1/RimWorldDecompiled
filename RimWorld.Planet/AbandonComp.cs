using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class AbandonComp : WorldObjectComp
	{
		public override IEnumerable<Gizmo> GetGizmos()
		{
			MapParent mapParent = parent as MapParent;
			if (mapParent.HasMap && mapParent.Faction == Faction.OfPlayer)
			{
				yield return SettlementAbandonUtility.AbandonCommand(mapParent);
			}
		}
	}
}
