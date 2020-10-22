using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class QuestPart_Delay : QuestPartActivable
	{
		public int delayTicks;

		public string expiryInfoPart;

		public string expiryInfoPartTip;

		public string inspectString;

		public List<ISelectable> inspectStringTargets;

		public bool isBad;

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
			if (Find.TickManager.TicksGame >= enableTick + delayTicks)
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
				return inspectString.Formatted(TicksLeft.ToStringTicksToPeriod());
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
}
