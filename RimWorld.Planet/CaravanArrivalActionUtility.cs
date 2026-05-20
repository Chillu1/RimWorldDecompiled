using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public static class CaravanArrivalActionUtility
{
	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, Caravan caravan, PlanetTile pathDestination, WorldObject revalidateWorldClickTarget, Action<Action> confirmActionProxy = null) where T : CaravanArrivalAction
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
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
			FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
			if (floatMenuAcceptanceReport2.Accepted)
			{
				caravan.pather.StartPath(pathDestination, arrivalActionGetter(), repathImmediately: true);
			}
			else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
			{
				Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(pathDestination), MessageTypeDefOf.RejectInput, historical: false);
			}
		};
		yield return new FloatMenuOption(label, (confirmActionProxy == null) ? action : ((Action)delegate
		{
			confirmActionProxy(action);
		}), MenuOptionPriority.Default, null, null, 0f, null, revalidateWorldClickTarget);
		if (!Prefs.DevMode)
		{
			yield break;
		}
		yield return new FloatMenuOption(label + " (Dev: instantly)", delegate
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
			if (floatMenuAcceptanceReport2.Accepted)
			{
				caravan.Tile = pathDestination;
				caravan.pather.StopDead();
				arrivalActionGetter().Arrived(caravan);
			}
			else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
			{
				Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(pathDestination), MessageTypeDefOf.RejectInput, historical: false);
			}
		}, MenuOptionPriority.Default, null, null, 0f, null, revalidateWorldClickTarget);
	}
}
