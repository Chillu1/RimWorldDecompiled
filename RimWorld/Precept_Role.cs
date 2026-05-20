using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;
using Verse.Sound;

namespace RimWorld;

public abstract class Precept_Role : Precept
{
	protected bool active;

	public bool restrictToSupremeGender;

	protected const string BulletPointString = "  - ";

	public List<PreceptApparelRequirement> apparelRequirements;

	private Dictionary<Gender, List<string>> allApparelRequirementLabelsCached = new Dictionary<Gender, List<string>>();

	public bool Active
	{
		get
		{
			if (!active)
			{
				return DebugSettings.activateAllIdeoRoles;
			}
			return true;
		}
	}

	public override string UIInfoFirstLine => def.LabelCap;

	public override string UIInfoSecondLine => base.LabelCap;

	public override string TipLabel => def.issue.LabelCap + ": " + base.LabelCap;

	public override bool UsesGeneratedName => true;

	public override bool CanRegenerate => true;

	public override bool SortByImpact => false;

	public override List<PreceptApparelRequirement> ApparelRequirements
	{
		get
		{
			return apparelRequirements;
		}
		set
		{
			tipCached = null;
			allApparelRequirementLabelsCached.Clear();
			apparelRequirements = value;
		}
	}

	public IEnumerable<WorkTypeDef> DisabledWorkTypes
	{
		get
		{
			List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				if ((def.roleDisabledWorkTags & list[i].workTags) != WorkTags.None)
				{
					yield return list[i];
				}
			}
		}
	}

	public List<string> AllApparelRequirementLabels(Gender gender, Pawn forPawn = null)
	{
		if (!allApparelRequirementLabelsCached.TryGetValue(gender, out var value))
		{
			value = new List<string>();
			if (apparelRequirements != null)
			{
				for (int i = 0; i < apparelRequirements.Count; i++)
				{
					ApparelRequirement requirement = apparelRequirements[i].requirement;
					value.Add((!requirement.groupLabel.NullOrEmpty()) ? requirement.groupLabel : requirement.AllRequiredApparel(Gender.Male).First().LabelCap.Resolve());
				}
			}
			allApparelRequirementLabelsCached[gender] = value;
		}
		if (apparelRequirements != null)
		{
			bool flag = false;
			for (int j = 0; j < apparelRequirements.Count; j++)
			{
				ApparelRequirement requirement2 = apparelRequirements[j].requirement;
				if (forPawn != null && !ApparelUtility.IsRequirementActive(requirement2, ApparelRequirementSource.Role, forPawn, out var disabledByLabel))
				{
					if (!flag)
					{
						value = new List<string>(value);
						flag = true;
					}
					string text = value[j];
					text += " [" + "ApparelRequirementDisabledLabel".Translate() + ": " + disabledByLabel + "]";
					value[j] = text;
				}
			}
		}
		return value;
	}

	public abstract IEnumerable<Pawn> ChosenPawns();

	public abstract Pawn ChosenPawnSingle();

	public abstract bool IsAssigned(Pawn p);

	public abstract void Unassign(Pawn p, bool generateThoughts);

	public string LabelForPawn(Pawn p)
	{
		if (def.leaderRole)
		{
			return ((p.gender == Gender.Female && !ideo.leaderTitleFemale.NullOrEmpty()) ? ideo.leaderTitleFemale : ideo.leaderTitleMale).CapitalizeFirst();
		}
		return name;
	}

	public static List<PreceptApparelRequirement> AllPossibleRequirements(Ideo ideo, PreceptDef def, bool desperate = false)
	{
		List<PreceptApparelRequirement> apparelRequirementPool = new List<PreceptApparelRequirement>();
		foreach (MemeDef meme in ideo.memes)
		{
			if (!meme.apparelRequirements.NullOrEmpty())
			{
				AddDistinct(meme.apparelRequirements);
			}
		}
		AddDistinct(from apparelPrecept in ideo.PreceptsListForReading.OfType<Precept_Apparel>()
			where apparelPrecept.TargetGender == Gender.None
			select new PreceptApparelRequirement
			{
				requirement = new ApparelRequirement
				{
					bodyPartGroupsMatchAny = apparelPrecept.apparelDef.apparel.bodyPartGroups,
					requiredDefs = new List<ThingDef> { apparelPrecept.apparelDef }
				}
			});
		if (def.roleApparelRequirements != null)
		{
			AddDistinct(def.roleApparelRequirements);
		}
		if (!desperate)
		{
			for (int num = apparelRequirementPool.Count - 1; num >= 0; num--)
			{
				foreach (Precept item in ideo.PreceptsListForReading)
				{
					if (item is Precept_Apparel precept_Apparel && !apparelRequirementPool[num].CanWearTogetherWith(precept_Apparel.apparelDef))
					{
						apparelRequirementPool.RemoveAt(num);
						break;
					}
				}
			}
		}
		return apparelRequirementPool;
		void AddDistinct(IEnumerable<PreceptApparelRequirement> requirements)
		{
			foreach (PreceptApparelRequirement requirement in requirements)
			{
				bool flag = false;
				foreach (PreceptApparelRequirement item2 in apparelRequirementPool)
				{
					if (item2.requirement.AllRequiredApparel().All((ThingDef a) => requirement.requirement.AllRequiredApparel().Contains(a)))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					apparelRequirementPool.Add(requirement);
				}
			}
		}
	}

	public override List<PreceptApparelRequirement> GenerateNewApparelRequirements(FactionDef generatingFor = null)
	{
		int apparelRequirementCount = ((def.roleApparelRequirementCountCurve != null) ? Mathf.CeilToInt(def.roleApparelRequirementCountCurve.Evaluate(Rand.Value)) : 0);
		if (apparelRequirementCount == 0)
		{
			return null;
		}
		List<PreceptApparelRequirement> apparelRequirements = new List<PreceptApparelRequirement>();
		foreach (MemeDef meme in ideo.memes)
		{
			if (meme.preventApparelRequirements)
			{
				return apparelRequirements;
			}
		}
		Choose(desperate: false);
		if (apparelRequirements.Count < apparelRequirementCount)
		{
			Choose(desperate: true);
		}
		return apparelRequirements;
		void Choose(bool desperate)
		{
			List<PreceptApparelRequirement> list = AllPossibleRequirements(ideo, def, desperate);
			List<PreceptApparelRequirement> list2 = new List<PreceptApparelRequirement>(list);
			foreach (PreceptApparelRequirement req in list)
			{
				foreach (Precept_Role item in ideo.RolesListForReading)
				{
					if (item.apparelRequirements != null && item.apparelRequirements.Any((PreceptApparelRequirement o) => o.requirement.SameApparelAs(req.requirement)))
					{
						list2.Remove(req);
					}
				}
			}
			ChooseApparelRequirements(list2);
			if (apparelRequirements.Count < apparelRequirementCount)
			{
				ChooseApparelRequirements(list);
			}
		}
		void ChooseApparelRequirements(List<PreceptApparelRequirement> source)
		{
			if (apparelRequirements.Count < apparelRequirementCount)
			{
				List<PreceptApparelRequirement> list = new List<PreceptApparelRequirement>(source);
				list.Shuffle();
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].CanAddRequirement(this, apparelRequirements, out var cannotAddReason, generatingFor) && !list[i].RequirementOverlapsOther(apparelRequirements, out cannotAddReason))
					{
						apparelRequirements.Add(list[i]);
						if (apparelRequirements.Count >= apparelRequirementCount)
						{
							break;
						}
					}
				}
			}
		}
	}

	public override void Init(Ideo ideo, FactionDef generatingFor = null)
	{
		base.Init(ideo);
		if (ModLister.CheckIdeology("Ideology role"))
		{
			RegenerateName();
			if (!ideo.classicExtraMode && !ideo.classicMode)
			{
				apparelRequirements = GenerateNewApparelRequirements(generatingFor);
			}
			allApparelRequirementLabelsCached.Clear();
			restrictToSupremeGender = Rand.Value < def.restrictToSupremeGenderChance;
			FillOrUpdateAbilities();
		}
	}

	public override string GenerateNameRaw()
	{
		if (!def.leaderRole)
		{
			GrammarRequest request = new GrammarRequest
			{
				Includes = { def.nameMaker }
			};
			AddIdeoRulesTo(ref request);
			return GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_roleName", request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false));
		}
		ideo.foundation.GenerateLeaderTitle();
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			item.ClearTipCache();
		}
		return ideo.leaderTitleMale;
	}

	public override string GetTip()
	{
		if (tipCached.NullOrEmpty())
		{
			Precept.tmpCompsDesc.Clear();
			if (!def.description.NullOrEmpty())
			{
				Precept.tmpCompsDesc.Append(def.description);
			}
			if (!def.leaderRole && def.activationBelieverCount != -1)
			{
				Precept.tmpCompsDesc.AppendLine();
				string text = ((def.activationBelieverCount > 1) ? Find.ActiveLanguageWorker.Pluralize(ideo.memberName) : ideo.memberName);
				string text2 = ((def.deactivationBelieverCount > 1) ? Find.ActiveLanguageWorker.Pluralize(ideo.memberName) : ideo.memberName);
				Precept.tmpCompsDesc.AppendInNewLine("RoleBelieverCountDesc".Translate(text, def.activationBelieverCount, text2, def.deactivationBelieverCount).Resolve());
			}
			if (!def.grantedAbilities.NullOrEmpty())
			{
				List<string> list = new List<string>();
				for (int i = 0; i < def.grantedAbilities.Count; i++)
				{
					if (def.grantedAbilities[i].comps.FirstOrDefault((AbilityCompProperties comp) => comp is CompProperties_AbilityStartRitual) is CompProperties_AbilityStartRitual compProperties_AbilityStartRitual)
					{
						foreach (Precept item in ideo.PreceptsListForReading)
						{
							if (item is Precept_Ritual precept_Ritual && precept_Ritual.def == compProperties_AbilityStartRitual.ritualDef)
							{
								list.Add(precept_Ritual.LabelCap);
								break;
							}
						}
					}
					else
					{
						list.Add(def.grantedAbilities[i].LabelCap.Resolve());
					}
				}
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("RoleGrantedAbilitiesLabel".Translate().Resolve() + ":"));
				Precept.tmpCompsDesc.AppendInNewLine(list.ToLineList("  - "));
			}
			List<string> list2 = new List<string>();
			foreach (Precept item2 in ideo.PreceptsListForReading)
			{
				if (item2 is Precept_Ritual precept_Ritual2 && item2.def.listedForRoles && precept_Ritual2?.behavior?.def.stages != null && precept_Ritual2.behavior.def.roles != null && !list2.Contains(precept_Ritual2.LabelCap) && precept_Ritual2.behavior.def.roles.Any((RitualRole r) => r.precept == def))
				{
					list2.Add(precept_Ritual2.LabelCap);
				}
			}
			if (list2.Count > 0)
			{
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("RoleRitualsLabel".Translate().Resolve() + ":"));
				Precept.tmpCompsDesc.AppendInNewLine(list2.ToLineList("  - "));
			}
			if (!def.roleRequirements.NullOrEmpty())
			{
				List<string> list3 = new List<string>();
				for (int num = 0; num < def.roleRequirements.Count; num++)
				{
					string text3 = def.roleRequirements[num].GetLabelCap(this).ResolveTags();
					if (!text3.NullOrEmpty())
					{
						list3.Add(text3);
					}
				}
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("RoleRequirementsLabel".Translate().Resolve() + ":"));
				Precept.tmpCompsDesc.AppendInNewLine(list3.ToLineList("  - "));
			}
			if (!def.roleEffects.NullOrEmpty())
			{
				List<string> list4 = new List<string>();
				foreach (RoleEffect item3 in def.roleEffects.OrderBy((RoleEffect r) => r.IsBad ? 1 : 0))
				{
					list4.Add(item3.Label(null, this));
				}
				if (list4.Count > 0)
				{
					Precept.tmpCompsDesc.AppendLine();
					Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("RoleEffectsLabel".Translate().Resolve() + ":"));
					Precept.tmpCompsDesc.AppendInNewLine(list4.ToLineList("  - "));
				}
			}
			List<string> list5 = AllApparelRequirementLabels((ChosenPawnSingle() == null) ? Gender.Male : ChosenPawnSingle().gender, ChosenPawnSingle());
			if (list5.Count > 0)
			{
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("RoleRequiredApparelLabel".Translate().Resolve() + ":"));
				Precept.tmpCompsDesc.AppendInNewLine(list5.ToLineList("  - "));
			}
			if (def.expectationsOffset != 0)
			{
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("Expectations".Translate().CapitalizeFirst().Resolve() + ":"));
				Precept.tmpCompsDesc.AppendInNewLine("  - " + "RoleExpectationOffset".Translate(def.expectationsOffset).Resolve());
			}
			if (def.roleDisabledWorkTags != WorkTags.None)
			{
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("DisabledWorkLabel".Translate().Resolve() + ":"));
				Precept.tmpCompsDesc.AppendInNewLine("  - " + DisabledWorkTypes.Select((WorkTypeDef w) => w.labelShort).ToCommaList().CapitalizeFirst());
			}
			if (!def.requiredMemes.NullOrEmpty())
			{
				IEnumerable<MemeDef> source = def.requiredMemes?.Where((MemeDef m) => ideo.memes.Contains(m));
				if (source.Any())
				{
					Precept.tmpCompsDesc.AppendLine();
					Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("UnlockedByMeme".Translate().Resolve() + ":"));
					Precept.tmpCompsDesc.AppendInNewLine(source.Select((MemeDef m) => m.LabelCap.Resolve()).ToLineList("  - "));
				}
			}
			Precept.tmpCompsDesc.AppendLine();
			Precept.tmpCompsDesc.AppendInNewLine("RoleAssignmentTip".Translate().Colorize(Color.gray).ResolveTags());
			tipCached = Precept.tmpCompsDesc.ToString();
		}
		return tipCached;
	}

	public override void Tick()
	{
		base.Tick();
		RecacheActivity();
	}

	public abstract void RecacheActivity();

	public abstract void Assign(Pawn p, bool addThoughts);

	public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
	{
		yield return EditFloatMenuOption();
	}

	public abstract void FillOrUpdateAbilities();

	protected List<Ability> FillOrUpdateAbilityList(Pawn forPawn, List<Ability> abilities)
	{
		if (def.grantedAbilities == null)
		{
			return null;
		}
		if (abilities != null)
		{
			abilities.RemoveAll((Ability a) => a == null || a.def == null);
			int num = 0;
			for (int num2 = 0; num2 < abilities.Count; num2++)
			{
				if (def.grantedAbilities.Contains(abilities[num2].def))
				{
					num++;
				}
			}
			if (num != def.grantedAbilities.Count)
			{
				abilities = null;
			}
		}
		if (abilities == null)
		{
			abilities = new List<Ability>();
			foreach (AbilityDef grantedAbility in def.grantedAbilities)
			{
				abilities.Add(AbilityUtility.MakeAbility(grantedAbility, forPawn, this));
			}
		}
		else
		{
			foreach (Ability ability in abilities)
			{
				ability.pawn = forPawn;
				ability.verb.caster = forPawn;
				ability.sourcePrecept = this;
			}
		}
		return abilities;
	}

	public abstract List<Ability> AbilitiesFor(Pawn p);

	public void Notify_PawnUnassigned(Pawn oldPawn)
	{
		oldPawn?.abilities?.Notify_TemporaryAbilitiesChanged();
		oldPawn?.Notify_DisabledWorkTypesChanged();
		oldPawn?.apparel?.Notify_RoleChanged();
		if (oldPawn != null && oldPawn.IsPrisoner)
		{
			Messages.Message("MessageRoleUnassignedPrisoner".Translate(oldPawn, LabelForPawn(oldPawn)), oldPawn, MessageTypeDefOf.SilentInput, historical: false);
			SoundDefOf.Quest_Succeded.PlayOneShotOnCamera();
		}
	}

	public void Notify_PawnAssigned(Pawn newPawn)
	{
		newPawn?.abilities?.Notify_TemporaryAbilitiesChanged();
		newPawn?.Notify_DisabledWorkTypesChanged();
		newPawn?.apparel?.Notify_RoleChanged();
		if (newPawn != null)
		{
			Messages.Message("MessageRoleAssigned".Translate(newPawn, LabelForPawn(newPawn)), newPawn, MessageTypeDefOf.SilentInput, historical: false);
			SoundDefOf.Quest_Succeded.PlayOneShotOnCamera();
		}
		FillOrUpdateAbilities();
	}

	public RoleRequirement GetFirstUnmetRequirement(Pawn p)
	{
		if (!def.roleCanBeChild && !RoleRequirement_NotChild.Requirement.Met(p, this))
		{
			return RoleRequirement_NotChild.Requirement;
		}
		foreach (RoleRequirement roleRequirement in def.roleRequirements)
		{
			if (!roleRequirement.Met(p, this))
			{
				return roleRequirement;
			}
		}
		return null;
	}

	public bool RequirementsMet(Pawn p)
	{
		if (!def.roleCanBeChild && !RoleRequirement_NotChild.Requirement.Met(p, this))
		{
			return false;
		}
		foreach (RoleRequirement roleRequirement in def.roleRequirements)
		{
			if (!roleRequirement.Met(p, this))
			{
				return false;
			}
		}
		return true;
	}

	public override void DrawIcon(Rect rect)
	{
		GUI.color = ideo.Color;
		GUI.DrawTexture(rect, Icon);
		GUI.color = Color.white;
	}

	protected override void PostDrawBox(Rect rect, out bool anyTooltipActive)
	{
		bool localAnyTooltipActive = false;
		if (!apparelRequirements.NullOrEmpty())
		{
			List<ThingDef> list = apparelRequirements.SelectMany((PreceptApparelRequirement ar) => ar.requirement.AllRequiredApparel()).Distinct().ToList();
			int num = list.Count * 24;
			num += (list.Count - 1) * 2;
			GenUI.DrawElementStack(new Rect(rect.xMax - (float)num, rect.yMin, num, 24f), 24f, list, delegate(Rect r, ThingDef apparel)
			{
				if (Mouse.IsOver(r))
				{
					localAnyTooltipActive = true;
					TooltipHandler.TipRegion(r, "RoleRequiredApparelLabel".Translate() + ": " + apparel.LabelCap + "\n\n" + apparel.DescriptionDetailed);
				}
				GUI.DrawTexture(r, apparel.uiIcon, ScaleMode.ScaleToFit, alphaBlend: true, 0f, apparel.uiIconColor, 0f, 0f);
			}, (ThingDef apparel) => 24f, 0f, 2f);
		}
		anyTooltipActive = localAnyTooltipActive;
	}

	public override List<Thought_Situational> SituationThoughtsToAdd(Pawn pawn, List<Thought_Situational> activeThoughts)
	{
		tmpSituationalThoughts.Clear();
		if (!Active || !pawn.IsFreeNonSlaveColonist || pawn.IsQuestLodger())
		{
			return tmpSituationalThoughts;
		}
		if (!def.leaderRole && def.createsRoleEmptyThought && !ChosenPawns().Any() && !activeThoughts.Any((Thought_Situational t) => t is Thought_IdeoRoleEmpty thought_IdeoRoleEmpty2 && thought_IdeoRoleEmpty2.Role == this))
		{
			Thought_IdeoRoleEmpty thought_IdeoRoleEmpty = (Thought_IdeoRoleEmpty)ThoughtMaker.MakeThought(ThoughtDefOf.IdeoRoleEmpty);
			if (thought_IdeoRoleEmpty != null)
			{
				thought_IdeoRoleEmpty.pawn = pawn;
				thought_IdeoRoleEmpty.sourcePrecept = this;
				tmpSituationalThoughts.Add(thought_IdeoRoleEmpty);
			}
		}
		if (IsAssigned(pawn) && apparelRequirements != null && apparelRequirements.Count > 0 && !pawn.IsQuestLodger() && !activeThoughts.Any((Thought_Situational t) => t is Thought_IdeoRoleApparelRequirementNotMet thought_IdeoRoleApparelRequirementNotMet2 && thought_IdeoRoleApparelRequirementNotMet2.Role == this))
		{
			Thought_IdeoRoleApparelRequirementNotMet thought_IdeoRoleApparelRequirementNotMet = (Thought_IdeoRoleApparelRequirementNotMet)ThoughtMaker.MakeThought(ThoughtDefOf.IdeoRoleApparelRequirementNotMet);
			if (thought_IdeoRoleApparelRequirementNotMet != null)
			{
				thought_IdeoRoleApparelRequirementNotMet.pawn = pawn;
				thought_IdeoRoleApparelRequirementNotMet.sourcePrecept = this;
				tmpSituationalThoughts.Add(thought_IdeoRoleApparelRequirementNotMet);
			}
		}
		return tmpSituationalThoughts;
	}

	protected bool ValidatePawn(Pawn p)
	{
		if (p.Faction == null || (p.Faction.IsPlayer && !p.IsFreeNonSlaveColonist))
		{
			return false;
		}
		if (p.Destroyed || p.Dead)
		{
			return false;
		}
		if (!RequirementsMet(p))
		{
			return false;
		}
		return true;
	}

	public override void Notify_RemovedByReforming()
	{
		foreach (Pawn item in ChosenPawns())
		{
			item?.Notify_DisabledWorkTypesChanged();
		}
	}

	public bool CanEquip(Pawn pawn, Thing thing, out string reason)
	{
		if (def.roleEffects != null)
		{
			foreach (RoleEffect roleEffect in def.roleEffects)
			{
				if (!roleEffect.CanEquip(pawn, thing))
				{
					reason = roleEffect.Label(pawn, this);
					return false;
				}
			}
		}
		reason = null;
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref active, "active", defaultValue: false);
		Scribe_Collections.Look(ref apparelRequirements, "apparelRequirements", LookMode.Deep);
		Scribe_Values.Look(ref restrictToSupremeGender, "restrictToSupremeGender", defaultValue: false);
	}

	public override void CopyTo(Precept precept)
	{
		base.CopyTo(precept);
		Precept_Role precept_Role = (Precept_Role)precept;
		precept_Role.active = active;
		precept_Role.restrictToSupremeGender = restrictToSupremeGender;
		if (apparelRequirements != null)
		{
			List<PreceptApparelRequirement> list = new List<PreceptApparelRequirement>();
			for (int i = 0; i < apparelRequirements.Count; i++)
			{
				PreceptApparelRequirement preceptApparelRequirement = new PreceptApparelRequirement();
				apparelRequirements[i].CopyTo(preceptApparelRequirement);
				list.Add(preceptApparelRequirement);
			}
			precept_Role.ApparelRequirements = list;
		}
		else
		{
			precept_Role.ApparelRequirements = null;
		}
	}
}
