using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanArrivalActionUtility
	{
		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, Caravan caravan, int pathDestination, WorldObject revalidateWorldClickTarget, Action<Action> confirmActionProxy = null) where T : CaravanArrivalAction
		{
			Func<FloatMenuAcceptanceReport> acceptanceReportGetter2 = acceptanceReportGetter;
			Caravan caravan2 = caravan;
			int pathDestination2 = pathDestination;
			Func<T> arrivalActionGetter2 = arrivalActionGetter;
			Action<Action> confirmActionProxy2 = confirmActionProxy;
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
			Action action = delegate
			{
				FloatMenuAcceptanceReport floatMenuAcceptanceReport3 = acceptanceReportGetter2();
				if (floatMenuAcceptanceReport3.Accepted)
				{
					caravan2.pather.StartPath(pathDestination2, arrivalActionGetter2(), repathImmediately: true);
				}
				else if (!floatMenuAcceptanceReport3.FailMessage.NullOrEmpty())
				{
					Messages.Message(floatMenuAcceptanceReport3.FailMessage, new GlobalTargetInfo(pathDestination2), MessageTypeDefOf.RejectInput, historical: false);
				}
			};
			yield return new FloatMenuOption(label, (confirmActionProxy2 == null) ? action : ((Action)delegate
			{
				confirmActionProxy2(action);
			}), MenuOptionPriority.Default, null, null, 0f, null, revalidateWorldClickTarget);
			if (!Prefs.DevMode)
			{
				yield break;
			}
			yield return new FloatMenuOption(label + " (Dev: instantly)", delegate
			{
				FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter2();
				if (floatMenuAcceptanceReport2.Accepted)
				{
					caravan2.Tile = pathDestination2;
					caravan2.pather.StopDead();
					arrivalActionGetter2().Arrived(caravan2);
				}
				else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
				{
					Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(pathDestination2), MessageTypeDefOf.RejectInput, historical: false);
				}
			}, MenuOptionPriority.Default, null, null, 0f, null, revalidateWorldClickTarget);
		}
	}
}
