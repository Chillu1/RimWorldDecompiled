using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public static class RitualUtility
{
	public static bool IsRitualTarget(this Thing thing)
	{
		return thing.TargetOfRitual() != null;
	}

	public static LordJob_Ritual TargetOfRitual(this Thing thing)
	{
		if (!thing.Spawned)
		{
			return null;
		}
		foreach (Lord lord in thing.Map.lordManager.lords)
		{
			if (lord.LordJob is LordJob_Ritual lordJob_Ritual && lordJob_Ritual.selectedTarget.Thing == thing && lordJob_Ritual.lord.CurLordToil.data is LordToilData_Gathering lordToilData_Gathering && lordToilData_Gathering.presentForTicks.Any())
			{
				return lordJob_Ritual;
			}
		}
		return null;
	}

	public static bool GoodSpectateCellForRitual(IntVec3 spot, Pawn p, Map map)
	{
		if (!spot.ContainsStaticFire(map) && !spot.GetTerrain(map).avoidWander)
		{
			return !PawnUtility.KnownDangerAt(spot, map, p);
		}
		return false;
	}

	public static float CalculateQualityAbstract(Precept_Ritual ritual, TargetInfo ritualTarget, RitualRoleAssignments assignments, RitualObligation obligation = null)
	{
		RitualOutcomeEffectDef def = ritual.outcomeEffect.def;
		float num = def.startingQuality;
		foreach (RitualOutcomeComp comp in def.comps)
		{
			QualityFactor qualityFactor = comp.GetQualityFactor(ritual, ritualTarget, obligation, assignments, null);
			if (qualityFactor != null)
			{
				if (qualityFactor.uncertainOutcome)
				{
					Log.Warning("Tried to calculate abstract quality for a ritual with uncertain quality offset, that can't be calculated without actually executing the ritual.");
				}
				else
				{
					num += qualityFactor.quality;
				}
			}
		}
		return num;
	}

	public static string QualityBreakdownAbstract(Precept_Ritual ritual, TargetInfo ritualTarget, RitualRoleAssignments assignments, RitualObligation obligation = null)
	{
		RitualOutcomeEffectDef def = ritual.outcomeEffect.def;
		float f = CalculateQualityAbstract(ritual, ritualTarget, assignments, obligation);
		TaggedString taggedString = "RitualOutcomeQualitySpecific".Translate(ritual.Label, f.ToStringPercent()).CapitalizeFirst() + ":\n";
		if (def.startingQuality > 0f)
		{
			taggedString += "\n  - " + "StartingRitualQuality".Translate(def.startingQuality.ToStringPercent()) + ".";
		}
		foreach (RitualOutcomeComp comp in def.comps)
		{
			QualityFactor qualityFactor = comp.GetQualityFactor(ritual, ritualTarget, obligation, assignments, null);
			if (qualityFactor != null)
			{
				taggedString += "\n  - " + qualityFactor.label;
				taggedString += ": " + comp.GetDescAbstract(qualityFactor.positive, qualityFactor.quality) + ".";
			}
		}
		if (ritual.RepeatPenaltyActive)
		{
			taggedString += "\n  - " + "RitualOutcomePerformedRecently".Translate() + ": " + ritual.RepeatQualityPenalty.ToStringPercent();
		}
		Tuple<ExpectationDef, float> expectationsOffset = RitualOutcomeEffectWorker_FromQuality.GetExpectationsOffset(ritualTarget.Map, ritual.def);
		if (expectationsOffset != null)
		{
			taggedString += "\n  - " + "RitualQualityExpectations".Translate(expectationsOffset.Item1.LabelCap) + ": " + expectationsOffset.Item2.ToStringPercent();
		}
		return taggedString;
	}

	public static void AddArrivalTag(Pawn pawn)
	{
		if (pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual && !lordJob_Ritual.PawnTagSet(pawn, "Arrived"))
		{
			lordJob_Ritual.AddTagForPawn(pawn, "Arrived");
		}
	}

	public static IEnumerable<LordJob_Ritual> GetActiveRitualsForPrecept(Precept_Ritual precept)
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			List<Lord> lords = maps[i].lordManager.lords;
			for (int j = 0; j < lords.Count; j++)
			{
				if (lords[j].LordJob is LordJob_Ritual lordJob_Ritual && lordJob_Ritual.Ritual == precept)
				{
					yield return lordJob_Ritual;
				}
			}
		}
	}

	public static string RoleChangeConfirmation(Pawn pawn, Precept_Role oldRole, Precept_Role newRole)
	{
		string text = "";
		if (oldRole != null)
		{
			text += " " + "ChooseRoleConfirmAssignHasOtherRole".Translate(pawn.Named("PAWN"), oldRole.Named("ROLE"));
		}
		Pawn pawn2 = newRole.ChosenPawns().FirstOrDefault();
		if (pawn2 != null && newRole is Precept_RoleSingle)
		{
			text += " " + "ChooseRoleConfirmAssignReplace".Translate(pawn2.Named("PAWN"));
		}
		else if (newRole.def.leaderRole)
		{
			foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
			{
				foreach (Precept item in allIdeo.PreceptsListForReading)
				{
					if (item != oldRole && item is Precept_Role precept_Role)
					{
						Pawn pawn3 = precept_Role.ChosenPawnSingle();
						if (precept_Role.def.leaderRole && pawn3 != null && pawn3 != pawn && pawn3.IsFreeColonist)
						{
							text += " " + "ChooseRoleConfirmAssignReplaceLeader".Translate(pawn3.Named("PAWN"), newRole.Named("ROLE"), precept_Role.Named("OTHERROLE"));
							break;
						}
					}
				}
			}
		}
		IEnumerable<WorkTypeDef> disabledWorkTypes = newRole.DisabledWorkTypes;
		if (disabledWorkTypes.Any())
		{
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			text += "ChooseRoleListWorkTypeRestrictions".Translate(pawn.Named("PAWN")) + ": \n" + disabledWorkTypes.Select((WorkTypeDef x) => x.labelShort.CapitalizeFirst()).ToLineList("  - ");
		}
		if (!newRole.def.grantedAbilities.NullOrEmpty())
		{
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			text += "ChooseRoleListAbilities".Translate(pawn.Named("PAWN")) + ": \n" + newRole.def.grantedAbilities.Select((AbilityDef x) => x.LabelCap.ToString()).ToLineList("  - ");
		}
		if (!newRole.apparelRequirements.NullOrEmpty())
		{
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			text += "ChooseRoleListApparelDemands".Translate(newRole.Named("ROLE")) + ": \n" + newRole.AllApparelRequirementLabels(pawn.gender, pawn).ToLineList("  - ");
		}
		if (newRole.def.roleRequiredWorkTags != WorkTags.None)
		{
			List<string> list = new List<string>();
			foreach (WorkTags allSelectedItem in newRole.def.roleRequiredWorkTags.GetAllSelectedItems<WorkTags>())
			{
				if (pawn.WorkTagIsDisabled(allSelectedItem))
				{
					list.Add(allSelectedItem.LabelTranslated().CapitalizeFirst());
				}
			}
			if (list.Any())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += "ChooseRoleRequiredWorkTagsDisabled".Translate(pawn.Named("PAWN"), newRole.Named("ROLE")) + ": \n" + list.ToLineList("  - ");
			}
		}
		else if (newRole.def.roleRequiredWorkTagAny != WorkTags.None)
		{
			bool flag = false;
			List<string> list2 = new List<string>();
			foreach (WorkTags allSelectedItem2 in newRole.def.roleRequiredWorkTagAny.GetAllSelectedItems<WorkTags>())
			{
				if (!pawn.WorkTagIsDisabled(allSelectedItem2))
				{
					flag = true;
					break;
				}
				list2.Add(allSelectedItem2.LabelTranslated().CapitalizeFirst());
			}
			if (!flag)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += "ChooseRoleRequiredWorkTagsDisabled".Translate(pawn.Named("PAWN"), newRole.Named("ROLE")) + ": \n" + list2.ToLineList("  - ");
			}
		}
		if (newRole.ChosenPawnSingle() == null && newRole is Precept_RoleSingle)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			text += "ChooseRoleHint".Translate();
		}
		return text;
	}

	public static IEnumerable<Precept_Role> AllRolesForPawn(Pawn p)
	{
		if (p.Ideo == null)
		{
			yield break;
		}
		foreach (Precept_Role item in p.Ideo.RolesListForReading.Where((Precept_Role r) => !r.def.leaderRole))
		{
			yield return item;
		}
		Precept_Role precept_Role = Faction.OfPlayer.ideos.PrimaryIdeo.RolesListForReading.FirstOrDefault((Precept_Role r) => r.def.leaderRole);
		if (precept_Role != null)
		{
			yield return precept_Role;
		}
	}

	public static IntVec3 RitualCrowdCenterFor(Pawn pawn)
	{
		if (pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual && pawn.CurJob != null && pawn.CurJob.speechFaceSpectatorsIfPossible)
		{
			return lordJob_Ritual.CurrentSpectatorCrowdCenter();
		}
		return IntVec3.Invalid;
	}
}
