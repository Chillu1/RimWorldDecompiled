using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_ViewArt : JoyGiver
	{
		private static List<Thing> candidates = new List<Thing>();

		public override Job TryGiveJob(Pawn pawn)
		{
			bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
			try
			{
				candidates.AddRange(pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Art).Where(delegate(Thing thing)
				{
					if (thing.Faction != Faction.OfPlayer || thing.IsForbidden(pawn) || (!allowedOutside && !thing.Position.Roofed(thing.Map)) || !pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None) || !thing.IsPoliticallyProper(pawn))
					{
						return false;
					}
					CompArt compArt = thing.TryGetComp<CompArt>();
					if (compArt == null)
					{
						Log.Error("No CompArt on thing being considered for viewing: " + thing);
						return false;
					}
					if (!compArt.CanShowArt || !compArt.Props.canBeEnjoyedAsArt)
					{
						return false;
					}
					Room room = thing.GetRoom();
					if (room == null)
					{
						return false;
					}
					return ((room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.Barracks && room.Role != RoomRoleDefOf.PrisonCell && room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.Hospital) || (pawn.ownership != null && pawn.ownership.OwnedRoom != null && pawn.ownership.OwnedRoom == room)) ? true : false;
				}));
				if (!candidates.TryRandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(StatDefOf.Beauty), 0.5f), out Thing result))
				{
					return null;
				}
				return JobMaker.MakeJob(def.jobDef, result);
			}
			finally
			{
				candidates.Clear();
			}
		}
	}
}
