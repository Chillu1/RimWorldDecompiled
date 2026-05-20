using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_ViewArt : JoyGiver
{
	private static readonly List<Thing> candidates = new List<Thing>();

	public override Job TryGiveJob(Pawn pawn)
	{
		bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
		try
		{
			candidates.AddRange(pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Art).Where(delegate(Thing thing)
			{
				if (!Validator(thing))
				{
					return false;
				}
				CompArt compArt = thing.TryGetComp<CompArt>();
				if (compArt == null)
				{
					Log.Error($"No CompArt on thing being considered for viewing: {thing}");
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
				return (!room.Role.avoidViewingArtIfUnowned || (pawn.ownership?.OwnedRoom != null && pawn.ownership.OwnedRoom == room)) ? true : false;
			}));
			if (!candidates.TryRandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(StatDefOf.Beauty), 0.5f), out var result))
			{
				return null;
			}
			return JobMaker.MakeJob(def.jobDef, result);
		}
		finally
		{
			candidates.Clear();
		}
		bool Validator(Thing thing)
		{
			if (thing.Faction == Faction.OfPlayer && (allowedOutside || thing.Position.Roofed(thing.Map)) && !thing.Fogged() && !thing.VacuumConcernTo(pawn) && thing.IsPoliticallyProper(pawn) && pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None))
			{
				return !thing.IsForbidden(pawn);
			}
			return false;
		}
	}
}
