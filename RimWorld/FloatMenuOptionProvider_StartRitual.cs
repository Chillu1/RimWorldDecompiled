using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class FloatMenuOptionProvider_StartRitual : FloatMenuOptionProvider
{
	private static HashSet<RitualPatternDef> showingGizmosForRitualsTmp = new HashSet<RitualPatternDef>();

	protected override bool Drafted => false;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.IdeologyActive;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		showingGizmosForRitualsTmp.Clear();
		foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
		{
			for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
			{
				Precept precept = ideo.PreceptsListForReading[i];
				if (!precept.def.showRitualFloatMenuOption)
				{
					continue;
				}
				Precept_Ritual ritual = precept as Precept_Ritual;
				if (ritual == null || (ritual.def.mergeRitualGizmosFromAllIdeos && showingGizmosForRitualsTmp.Contains(ritual.sourcePattern)))
				{
					continue;
				}
				if (!ritual.activeObligations.NullOrEmpty())
				{
					bool disabledReasonSet = false;
					string disableReason = null;
					foreach (RitualObligation obligation in ritual.activeObligations)
					{
						if (!ritual.CanUseTarget(clickedThing, obligation).canUse)
						{
							continue;
						}
						if (!disabledReasonSet)
						{
							disabledReasonSet = true;
							disableReason = ritual.behavior.CanStartRitualNow(clickedThing, ritual, context.FirstSelectedPawn);
							if (ritual.abilityOnCooldownUntilTick > Find.TickManager.TicksGame)
							{
								disableReason = "AbilityOnCooldown".Translate((ritual.abilityOnCooldownUntilTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()).Resolve();
							}
						}
						string text = ritual.GetBeginRitualText(obligation);
						bool disabled = !disableReason.NullOrEmpty();
						if (disabled)
						{
							text = text + " (" + disableReason + ")";
						}
						Action action = delegate
						{
							ritual.ShowRitualBeginWindow(clickedThing, obligation, context.FirstSelectedPawn);
						};
						yield return new FloatMenuOption(text, (!disabled) ? action : null, ritual.Icon, (!ritual.def.mergeRitualGizmosFromAllIdeos) ? ideo.Color : Color.white);
						showingGizmosForRitualsTmp.Add(ritual.sourcePattern);
						if (!disabled && ritual.mergeGizmosForObligations)
						{
							break;
						}
					}
				}
				else
				{
					if (!ritual.isAnytime || !ritual.ShouldShowGizmo(clickedThing))
					{
						continue;
					}
					TaggedString beginRitualText = ritual.GetBeginRitualText();
					RitualTargetUseReport ritualTargetUseReport = ritual.CanUseTarget(clickedThing, null);
					string reason = ritual.behavior.CanStartRitualNow(clickedThing, ritual, context.FirstSelectedPawn);
					if (ritual.abilityOnCooldownUntilTick > Find.TickManager.TicksGame)
					{
						reason = "AbilityOnCooldown".Translate((ritual.abilityOnCooldownUntilTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()).Resolve();
					}
					else if (ritual.def.sourcePawnRoleDef != null && ritual.def.sourceAbilityDef != null)
					{
						Precept_Role precept_Role = ritual.ideo.RolesListForReading.FirstOrDefault((Precept_Role r) => r.def == ritual.def.sourcePawnRoleDef);
						if (precept_Role != null && precept_Role is Precept_RoleSingle precept_RoleSingle && precept_RoleSingle.ChosenPawnSingle() != null)
						{
							precept_RoleSingle.AbilitiesForReading.FirstOrDefault((Ability a) => a.def == ritual.def.sourceAbilityDef)?.GizmoDisabled(out reason);
						}
					}
					if (!reason.NullOrEmpty())
					{
						beginRitualText += " (" + reason + ")";
					}
					else if (!ritualTargetUseReport.failReason.NullOrEmpty())
					{
						beginRitualText += " (" + ritualTargetUseReport.failReason + ")";
					}
					yield return new FloatMenuOption(beginRitualText, (reason.NullOrEmpty() && ritualTargetUseReport.failReason.NullOrEmpty()) ? new Action(Action) : null, ritual.Icon, (!ritual.def.mergeRitualGizmosFromAllIdeos) ? ideo.Color : Color.white);
					showingGizmosForRitualsTmp.Add(ritual.sourcePattern);
				}
				void Action()
				{
					ritual.ShowRitualBeginWindow(clickedThing, null, context.FirstSelectedPawn);
				}
			}
		}
	}
}
