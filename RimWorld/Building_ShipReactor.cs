using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_ShipReactor : Building
	{
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo item in ShipUtility.ShipStartupGizmos(this))
			{
				yield return item;
			}
		}
	}
}
