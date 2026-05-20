using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Precept_GravshipLaunch : Precept_Ritual
{
	public override Window GetRitualBeginWindow(TargetInfo targetInfo, RitualObligation obligation = null, Action onConfirm = null, Pawn organizer = null, Dictionary<string, Pawn> forcedForRole = null, Pawn selectedPawn = null)
	{
		string text = behavior.CanStartRitualNow(targetInfo, this, selectedPawn, forcedForRole);
		if (!string.IsNullOrEmpty(text))
		{
			Messages.Message(text, targetInfo, MessageTypeDefOf.RejectInput, historical: false);
		}
		return new Dialog_BeginGravshipLaunch(Label.CapitalizeFirst(), this, targetInfo, targetInfo.Map, delegate(RitualRoleAssignments assignments)
		{
			behavior.TryExecuteOn(targetInfo, organizer, this, obligation, assignments, playerForced: true);
			onConfirm?.Invoke();
			return true;
		}, organizer, obligation, delegate(Pawn pawn, bool voluntary, bool allowOtherIdeos)
		{
			if (pawn.GetLord() != null)
			{
				return false;
			}
			if (pawn.RaceProps.Animal && !behavior.def.roles.Any((RitualRole r) => r.AppliesToPawn(pawn, out var _, targetInfo, null, null, null, skipReason: true)))
			{
				return false;
			}
			if (pawn.IsSubhuman)
			{
				return false;
			}
			return !ritualOnlyForIdeoMembers || def.allowSpectatorsFromOtherIdeos || pawn.Ideo == ideo || !voluntary || allowOtherIdeos || pawn.IsPrisonerOfColony || pawn.RaceProps.Animal || (!forcedForRole.NullOrEmpty() && forcedForRole.ContainsValue(pawn));
		}, "Begin".Translate(), (organizer != null) ? new List<Pawn> { organizer } : null, forcedForRole, null, null, selectedPawn);
	}

	public override void ExposeData()
	{
		bool flag = false;
		if (Scribe.mode == LoadSaveMode.LoadingVars && sourcePattern == null)
		{
			sourcePattern = DefDatabase<RitualPatternDef>.GetNamed("GravshipLaunch");
			flag = true;
		}
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.LoadingVars && flag)
		{
			obligationTriggers = new List<RitualObligationTrigger>();
			DefDatabase<RitualPatternDef>.GetNamed("GravshipLaunch").Fill(this);
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			ignoreExtremeTemperatures = true;
		}
	}
}
