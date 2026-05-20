using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef : Def
{
	public ResearchProjectDef researchPrerequisite;

	public bool allowsDrafting;

	public bool allowsFloatMenu;

	public int cooldownHours;

	public bool nonRequiredPawnsMayLeave;

	public float rolePowerFactor = 0.2f;

	public bool aiCastable;

	public bool playerCastable = true;

	public float minThreatPoints;

	public bool castableOnPocketMaps = true;

	public List<PlanetLayerDef> layerWhitelist = new List<PlanetLayerDef>();

	[MustTranslate]
	public string letterAICompleteLabel;

	[MustTranslate]
	public string letterAICompleteText;

	[MustTranslate]
	public string letterAIArrivedText;

	[NoTranslate]
	public string iconPath;

	public Texture2D uiIcon = BaseContent.BadTex;

	private List<PsychicRitualRoleDef> rolesBackingList = new List<PsychicRitualRoleDef>(8);

	private static readonly List<Pawn> tmpPawnsIterationList = new List<Pawn>(16);

	public virtual List<PsychicRitualRoleDef> Roles
	{
		get
		{
			rolesBackingList.Clear();
			return rolesBackingList;
		}
	}

	public bool Visible
	{
		get
		{
			if (!playerCastable)
			{
				return false;
			}
			if (DebugSettings.godMode)
			{
				return true;
			}
			if (researchPrerequisite == null)
			{
				return true;
			}
			if (!researchPrerequisite.IsFinished)
			{
				return false;
			}
			return true;
		}
	}

	public override void PostLoad()
	{
		if (!string.IsNullOrEmpty(iconPath))
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				uiIcon = ContentFinder<Texture2D>.Get(iconPath);
			});
		}
	}

	public virtual AcceptanceReport AllowsDrafting(Pawn pawn)
	{
		if (allowsDrafting)
		{
			return true;
		}
		return new AcceptanceReport("ParticipatingInPsychicRitual".Translate(pawn, label));
	}

	public virtual AcceptanceReport AllowsFloatMenu(Pawn pawn)
	{
		if (allowsFloatMenu)
		{
			return true;
		}
		return new AcceptanceReport("ParticipatingInPsychicRitual".Translate(pawn, label));
	}

	public virtual bool BlocksSocialInteraction(Pawn pawn)
	{
		return true;
	}

	public virtual AcceptanceReport AbilityAllowed(Ability ability)
	{
		return new AcceptanceReport("AbilityDisabledInPsychicRitual".Translate(ability.pawn, label));
	}

	public virtual PsychicRitualGraph CreateGraph()
	{
		return new PsychicRitualGraph();
	}

	public virtual List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		throw new NotImplementedException("You must subclass PsychicRitualDef and override CreateToils() to return a list of toils.");
	}

	public virtual PsychicRitualCandidatePool FindCandidatePool()
	{
		return new PsychicRitualCandidatePool(new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonistsAndPrisonersSpawned.Where((Pawn p) => !p.IsSubhuman)), new List<Pawn>());
	}

	public virtual PsychicRitualRoleAssignments BuildRoleAssignments(TargetInfo target)
	{
		return new PsychicRitualRoleAssignments(Roles, target);
	}

	public static bool OfferingReachable(Map map, List<Pawn> pawns, IngredientCount offering, out int reachableCount)
	{
		using (new ProfilerBlock("Offering reachable"))
		{
			reachableCount = 0;
			float num = offering.GetBaseCount();
			if (num <= 0f)
			{
				return true;
			}
			List<Thing> list;
			using (new ProfilerBlock("ThingsMatchingFilter"))
			{
				list = map.listerThings.ThingsMatchingFilter(offering.filter);
			}
			foreach (Thing item in list)
			{
				if (num <= 0f)
				{
					break;
				}
				foreach (Pawn pawn in pawns)
				{
					if (pawn.CanReserveAndReach(item, PathEndMode.Touch, pawn.NormalMaxDanger()) && !item.IsForbidden(pawn) && !item.Fogged())
					{
						num -= (float)item.stackCount;
						reachableCount += item.stackCount;
						break;
					}
				}
			}
			return num <= 0f;
		}
	}

	public virtual IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		return Enumerable.Empty<string>();
	}

	public virtual TaggedString PsychicRitualBegunMessage(PsychicRitualRoleAssignments assignments)
	{
		return "PsychicRitualBegun".Translate(label);
	}

	public virtual TaggedString PsychicRitualCompletedMessage()
	{
		return "PsychicRitualCompleted".Translate(label);
	}

	public virtual TaggedString PsychicRitualCanceledMessage(TaggedString reason)
	{
		if (reason.NullOrEmpty())
		{
			return "PsychicRitualCanceled".Translate(label);
		}
		return "PsychicRitualCanceledBecause".Translate(label, reason);
	}

	public virtual TaggedString LeftPsychicRitualMessage(Pawn pawn, TaggedString reason)
	{
		if (reason.NullOrEmpty())
		{
			return "PsychicRitualLeft".Translate(pawn, label);
		}
		return "PsychicRitualLeftBecause".Translate(pawn, label, reason);
	}

	public virtual string GetPawnReport(PsychicRitual psychicRitual, Pawn pawn)
	{
		return "PsychicRitualAttending".Translate(label.Named("RITUALNAME"));
	}

	public virtual Lord MakeNewLord(PsychicRitualRoleAssignments assignments)
	{
		Lord lord = LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_PsychicRitual(this, assignments), Find.CurrentMap, assignments.AllAssignedPawns);
		if (assignments.Target.Thing is Building b)
		{
			lord.AddBuilding(b);
		}
		return lord;
	}

	public virtual bool IsValidTarget(TargetInfo target, out AnyEnum reason)
	{
		reason = AnyEnum.None;
		return true;
	}

	public virtual TaggedString InvalidTargetReason(AnyEnum reason)
	{
		if (reason == AnyEnum.None)
		{
			return TaggedString.Empty;
		}
		throw new InvalidOperationException("Unknown enum type " + reason.enumType.ToStringSafe() + "; did you forget to override `InvalidTargetReason` in a child class?");
	}

	public virtual void CalculateMaxPower(PsychicRitualRoleAssignments assignments, List<QualityFactor> powerFactorsOut, out float power)
	{
		power = 0f;
		int num = 0;
		IReadOnlyDictionary<PsychicRitualRoleDef, List<Pawn>> roleAssignments = assignments.RoleAssignments;
		PsychicRitualRoleDef key;
		List<Pawn> value;
		foreach (KeyValuePair<PsychicRitualRoleDef, List<Pawn>> item in roleAssignments)
		{
			item.Deconstruct(out key, out value);
			PsychicRitualRoleDef psychicRitualRoleDef = key;
			List<Pawn> list = value;
			if (psychicRitualRoleDef.applyPowerOffset)
			{
				num += list.Count;
			}
			if (psychicRitualRoleDef.MaxCount != psychicRitualRoleDef.MinCount)
			{
				float num2 = (float)(list.Count - psychicRitualRoleDef.MinCount) / (float)(psychicRitualRoleDef.MaxCount - psychicRitualRoleDef.MinCount);
				power += rolePowerFactor * num2;
				powerFactorsOut?.Add(new QualityFactor
				{
					count = $"{list.Count} / {psychicRitualRoleDef.MaxCount}",
					label = Find.ActiveLanguageWorker.Pluralize(psychicRitualRoleDef.LabelCap),
					positive = (list.Count >= psychicRitualRoleDef.MinCount),
					quality = rolePowerFactor * num2,
					toolTip = psychicRitualRoleDef.description.CapitalizeFirst().EndWithPeriod()
				});
			}
		}
		if (num > 0)
		{
			float num3 = 0f;
			foreach (KeyValuePair<PsychicRitualRoleDef, List<Pawn>> item2 in roleAssignments)
			{
				item2.Deconstruct(out key, out value);
				PsychicRitualRoleDef psychicRitualRoleDef2 = key;
				List<Pawn> list2 = value;
				if (!psychicRitualRoleDef2.applyPowerOffset)
				{
					continue;
				}
				foreach (Pawn item3 in list2)
				{
					num3 += item3.GetStatValue(StatDefOf.PsychicRitualQualityOffset) / (float)num;
				}
			}
			if (!Mathf.Approximately(num3, 0f))
			{
				power += num3;
				powerFactorsOut?.Add(new QualityFactor
				{
					label = "PsychicRitualDef_InvocationCircle_QualityFactor_Ideoligion".Translate(),
					positive = (num3 > 0f),
					count = num3.ToStringPercent(),
					quality = num3,
					toolTip = "PsychicRitualDef_InvocationCircle_QualityFactor_Ideoligion_Tooltip".Translate()
				});
			}
		}
		power = Mathf.Clamp01(power);
	}

	public virtual void RemoveIncapablePawns(PsychicRitual psychicRitual)
	{
		foreach (var (psychicRitualRoleDef2, collection) in psychicRitual.assignments.RoleAssignments)
		{
			tmpPawnsIterationList.Clear();
			tmpPawnsIterationList.AddRange(collection);
			foreach (Pawn tmpPawnsIteration in tmpPawnsIterationList)
			{
				if (!psychicRitualRoleDef2.PawnCanDo(PsychicRitualRoleDef.Context.Runtime, tmpPawnsIteration, psychicRitual.assignments.Target, out var reason) && psychicRitual.LeaveOrCancelPsychicRitual(psychicRitualRoleDef2, tmpPawnsIteration, reason.ToPlayerReadable()) == PsychicRitual.LeftOrCanceled.Canceled)
				{
					return;
				}
			}
		}
	}

	public virtual void CheckPsychicRitualCancelConditions(PsychicRitual psychicRitual)
	{
		TargetInfo target = psychicRitual.assignments.Target;
		if (target.ThingDestroyed)
		{
			psychicRitual.CancelPsychicRitual("PsychicRitualDef_TargetDestroyed".Translate(target.Thing.Named("TARGET")));
		}
	}

	public virtual TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return TaggedString.Empty;
	}

	public virtual IEnumerable<TaggedString> OutcomeWarnings(PsychicRitualRoleAssignments assignments)
	{
		return Enumerable.Empty<TaggedString>();
	}

	public virtual TaggedString TimeAndOfferingLabel()
	{
		return TaggedString.Empty;
	}

	public virtual void InitializeCast(Map map)
	{
	}

	public virtual IntVec3 GetBestStandableRolePosition(bool playerRitual, IntVec3 origin, IntVec3 ritualPosition, Map map, float radius = 8f)
	{
		if (playerRitual)
		{
			return origin;
		}
		IntVec3 result = CellFinder.StandableCellNear(origin, map, radius, (IntVec3 c) => map.reachability.CanReach(ritualPosition, c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoorsOrWater));
		if (result.IsValid)
		{
			return result;
		}
		return origin;
	}

	public virtual IEnumerable<string> GetPawnTooltipExtras(Pawn pawn)
	{
		yield break;
	}
}
