using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class TransportPodsArrivalActionUtility
	{
		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, CompLaunchable representative, int destinationTile, Action<Action> uiConfirmationCallback = null) where T : TransportPodsArrivalAction
		{
			Func<FloatMenuAcceptanceReport> acceptanceReportGetter2 = acceptanceReportGetter;
			Action<Action> uiConfirmationCallback2 = uiConfirmationCallback;
			CompLaunchable representative2 = representative;
			int destinationTile2 = destinationTile;
			Func<T> arrivalActionGetter2 = arrivalActionGetter;
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter2();
			if (!floatMenuAcceptanceReport.Accepted && floatMenuAcceptanceReport.FailReason.NullOrEmpty() && floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
			{
				yield break;
			}
			if (!floatMenuAcceptanceReport.FailReason.NullOrEmpty())
			{
				yield return new FloatMenuOption(label + " (" + floatMenuAcceptanceReport.FailReason + ")", null);
				yield break;
			}
			yield return new FloatMenuOption(label, delegate
			{
				FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter2();
				if (floatMenuAcceptanceReport2.Accepted)
				{
					if (uiConfirmationCallback2 == null)
					{
						representative2.TryLaunch(destinationTile2, arrivalActionGetter2());
					}
					else
					{
						uiConfirmationCallback2(delegate
						{
							representative2.TryLaunch(destinationTile2, arrivalActionGetter2());
						});
					}
				}
				else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
				{
					Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(destinationTile2), MessageTypeDefOf.RejectInput, historical: false);
				}
			});
		}

		public static bool AnyNonDownedColonist(IEnumerable<IThingHolder> pods)
		{
			foreach (IThingHolder pod in pods)
			{
				ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
				for (int i = 0; i < directlyHeldThings.Count; i++)
				{
					Pawn pawn = directlyHeldThings[i] as Pawn;
					if (pawn != null && pawn.IsColonist && !pawn.Downed)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool AnyPotentialCaravanOwner(IEnumerable<IThingHolder> pods, Faction faction)
		{
			foreach (IThingHolder pod in pods)
			{
				ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
				for (int i = 0; i < directlyHeldThings.Count; i++)
				{
					Pawn pawn = directlyHeldThings[i] as Pawn;
					if (pawn != null && CaravanUtility.IsOwner(pawn, faction))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static Thing GetLookTarget(List<ActiveDropPodInfo> pods)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				ThingOwner directlyHeldThings = pods[i].GetDirectlyHeldThings();
				for (int j = 0; j < directlyHeldThings.Count; j++)
				{
					Pawn pawn = directlyHeldThings[j] as Pawn;
					if (pawn != null && pawn.IsColonist)
					{
						return pawn;
					}
				}
			}
			for (int k = 0; k < pods.Count; k++)
			{
				Thing thing = pods[k].GetDirectlyHeldThings().FirstOrDefault();
				if (thing != null)
				{
					return thing;
				}
			}
			return null;
		}

		public static void DropTravelingTransportPods(List<ActiveDropPodInfo> dropPods, IntVec3 near, Map map)
		{
			RemovePawnsFromWorldPawns(dropPods);
			for (int i = 0; i < dropPods.Count; i++)
			{
				DropCellFinder.TryFindDropSpotNear(near, map, out IntVec3 result, allowFogged: false, canRoofPunch: true);
				DropPodUtility.MakeDropPodAt(result, map, dropPods[i]);
			}
		}

		public static void RemovePawnsFromWorldPawns(List<ActiveDropPodInfo> pods)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				ThingOwner innerContainer = pods[i].innerContainer;
				for (int j = 0; j < innerContainer.Count; j++)
				{
					Pawn pawn = innerContainer[j] as Pawn;
					if (pawn != null && pawn.IsWorldPawn())
					{
						Find.WorldPawns.RemovePawn(pawn);
					}
				}
			}
		}
	}
}
