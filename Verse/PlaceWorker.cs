using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class PlaceWorker
	{
		public virtual AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			return AcceptanceReport.WasAccepted;
		}

		public virtual void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
		}

		public virtual void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
		}

		public virtual bool ForceAllowPlaceOver(BuildableDef other)
		{
			return false;
		}

		public virtual IEnumerable<TerrainAffordanceDef> DisplayAffordances()
		{
			yield break;
		}
	}
}
