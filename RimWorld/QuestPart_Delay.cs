using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_Delay : QuestPartActivable
{
	public int delayTicks;

	public string expiryInfoPart;

	public string expiryInfoPartTip;

	public string inspectString;

	public List<ISelectable> inspectStringTargets;

	public bool isBad;

	public bool waitUntilPlayerHasHomeMap;

	public string alertLabel;

	public string alertExplanation;

	public List<GlobalTargetInfo> alertCulprits = new List<GlobalTargetInfo>();

	public int ticksLeftAlertCritical;

	public int TicksLeft
	{
		get
		{
			if (base.State != QuestPartState.Enabled)
			{
				return 0;
			}
			return enableTick + delayTicks - Find.TickManager.TicksGame;
		}
	}

	public override string ExpiryInfoPart
	{
		get
		{
			if (quest.Historical)
			{
				return null;
			}
			return expiryInfoPart.Formatted(TicksLeft.ToStringTicksToPeriod());
		}
	}

	public override string ExpiryInfoPartTip => expiryInfoPartTip.Formatted(GenDate.DateFullStringWithHourAt(GenDate.TickGameToAbs(enableTick + delayTicks), QuestUtility.GetLocForDates()));

	public override string AlertLabel
	{
		get
		{
			TaggedString? taggedString = alertLabel?.Formatted(TicksLeft.ToStringTicksToPeriodVerbose());
			if (!taggedString.HasValue)
			{
				return null;
			}
			return taggedString.GetValueOrDefault();
		}
	}

	public override string AlertExplanation
	{
		get
		{
			TaggedString? taggedString = alertExplanation?.Formatted(TicksLeft.ToStringTicksToPeriodVerbose());
			if (!taggedString.HasValue)
			{
				return null;
			}
			return taggedString.GetValueOrDefault();
		}
	}

	public override AlertReport AlertReport
	{
		get
		{
			if (alertCulprits.Count <= 0)
			{
				return AlertReport.Inactive;
			}
			return AlertReport.CulpritsAre(alertCulprits);
		}
	}

	public override bool AlertCritical => TicksLeft < ticksLeftAlertCritical;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (inspectStringTargets == null)
			{
				yield break;
			}
			for (int i = 0; i < inspectStringTargets.Count; i++)
			{
				ISelectable selectable = inspectStringTargets[i];
				if (selectable is Thing)
				{
					yield return (Thing)selectable;
				}
				else if (selectable is WorldObject)
				{
					yield return (WorldObject)selectable;
				}
			}
		}
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if ((!waitUntilPlayerHasHomeMap || Find.AnyPlayerHomeMap != null) && Find.TickManager.TicksGame >= enableTick + delayTicks)
		{
			DelayFinished();
		}
	}

	protected virtual void DelayFinished()
	{
		Complete();
	}

	public override string ExtraInspectString(ISelectable target)
	{
		if (inspectStringTargets != null && inspectStringTargets.Contains(target))
		{
			return inspectString.Formatted(TicksLeft.ToStringTicksToPeriod()).Resolve();
		}
		return null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref delayTicks, "delayTicks", 0);
		Scribe_Values.Look(ref expiryInfoPart, "expiryInfoPart");
		Scribe_Values.Look(ref expiryInfoPartTip, "expiryInfoPartTip");
		Scribe_Values.Look(ref inspectString, "inspectString");
		Scribe_Collections.Look(ref inspectStringTargets, "inspectStringTargets", LookMode.Reference);
		Scribe_Values.Look(ref isBad, "isBad", defaultValue: false);
		Scribe_Values.Look(ref alertLabel, "alertLabel");
		Scribe_Values.Look(ref alertExplanation, "alertExplanation");
		Scribe_Values.Look(ref ticksLeftAlertCritical, "ticksLeftAlertCritical", 0);
		Scribe_Values.Look(ref waitUntilPlayerHasHomeMap, "waitUntilPlayerHasHomeMap", defaultValue: false);
		Scribe_Collections.Look(ref alertCulprits, "alertCulprits", LookMode.GlobalTargetInfo);
		if (alertCulprits == null)
		{
			alertCulprits = new List<GlobalTargetInfo>();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			alertCulprits.RemoveAll((GlobalTargetInfo x) => !x.IsValid);
		}
	}

	public override void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
		if (base.State == QuestPartState.Enabled)
		{
			Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
			if (Widgets.ButtonText(rect, "End " + ToString()))
			{
				DelayFinished();
			}
			curY += rect.height + 4f;
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		delayTicks = Rand.RangeInclusive(833, 2500);
	}

	public void DebugForceEnd()
	{
		DelayFinished();
	}
}
