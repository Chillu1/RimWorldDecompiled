using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_VisitGrave : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
			if (!pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Grave).Where(delegate(Thing x)
			{
				Building_Grave building_Grave = (Building_Grave)x;
				return x.Faction == Faction.OfPlayer && building_Grave.HasCorpse && !building_Grave.IsForbidden(pawn) && building_Grave.Corpse.InnerPawn.Faction == Faction.OfPlayer && (allowedOutside || building_Grave.Position.Roofed(building_Grave.Map)) && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.None) && building_Grave.IsPoliticallyProper(pawn);
			}).TryRandomElementByWeight(delegate(Thing x)
			{
				float lengthHorizontal = (x.Position - pawn.Position).LengthHorizontal;
				return Mathf.Max(150f - lengthHorizontal, 5f);
			}, out Thing result))
			{
				return null;
			}
			return JobMaker.MakeJob(def.jobDef, result);
		}
	}
}
