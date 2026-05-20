using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse.Grammar;

namespace Verse;

public class BattleLogEntry_MeleeCombat : LogEntry_DamageResult
{
	private RulePackDef ruleDef;

	private Pawn initiator;

	private Pawn recipientPawn;

	private ThingDef recipientThing;

	private ImplementOwnerTypeDef implementType;

	private ThingDef ownerEquipmentDef;

	private HediffDef ownerHediffDef;

	private string toolLabel;

	public bool alwaysShowInCompact;

	[TweakValue("LogFilter", 0f, 1f)]
	private static float DisplayChanceOnMiss = 0.5f;

	private string InitiatorName
	{
		get
		{
			if (initiator == null)
			{
				return "null";
			}
			return initiator.LabelShort;
		}
	}

	private string RecipientName
	{
		get
		{
			if (recipientPawn == null)
			{
				return "null";
			}
			return recipientPawn.LabelShort;
		}
	}

	public RulePackDef RuleDef
	{
		get
		{
			return ruleDef;
		}
		set
		{
			ruleDef = value;
			ResetCache();
		}
	}

	public BattleLogEntry_MeleeCombat()
	{
	}

	public BattleLogEntry_MeleeCombat(RulePackDef ruleDef, bool alwaysShowInCompact, Pawn initiator, Thing recipient, ImplementOwnerTypeDef implementType, string toolLabel, ThingDef ownerEquipmentDef = null, HediffDef ownerHediffDef = null, LogEntryDef def = null)
		: base(def)
	{
		this.ruleDef = ruleDef;
		this.alwaysShowInCompact = alwaysShowInCompact;
		this.initiator = initiator;
		this.implementType = implementType;
		this.ownerEquipmentDef = ownerEquipmentDef;
		this.ownerHediffDef = ownerHediffDef;
		this.toolLabel = toolLabel;
		if (recipient is Pawn)
		{
			recipientPawn = recipient as Pawn;
		}
		else if (recipient != null)
		{
			recipientThing = recipient.def;
		}
		if (ownerEquipmentDef != null && ownerHediffDef != null)
		{
			Log.ErrorOnce($"Combat log owned by both equipment {ownerEquipmentDef.label} and hediff {ownerHediffDef.label}, may produce unexpected results", 96474669);
		}
	}

	public override bool Concerns(Thing t)
	{
		if (t != initiator)
		{
			return t == recipientPawn;
		}
		return true;
	}

	public override IEnumerable<Thing> GetConcerns()
	{
		if (initiator != null)
		{
			yield return initiator;
		}
		if (recipientPawn != null)
		{
			yield return recipientPawn;
		}
	}

	public override bool CanBeClickedFromPOV(Thing pov)
	{
		if (pov != initiator || recipientPawn == null || !CameraJumper.CanJump(recipientPawn))
		{
			if (pov == recipientPawn)
			{
				return CameraJumper.CanJump(initiator);
			}
			return false;
		}
		return true;
	}

	public override void ClickedFromPOV(Thing pov)
	{
		if (pov == initiator && recipientPawn != null)
		{
			CameraJumper.TryJumpAndSelect(recipientPawn);
		}
		else if (pov == recipientPawn)
		{
			CameraJumper.TryJumpAndSelect(initiator);
		}
		else if (recipientPawn != null)
		{
			throw new NotImplementedException();
		}
	}

	public override Texture2D IconFromPOV(Thing pov)
	{
		if (damagedParts.NullOrEmpty())
		{
			return def.iconMissTex;
		}
		if (deflected)
		{
			return def.iconMissTex;
		}
		if (pov == null || pov == recipientPawn)
		{
			return def.iconDamagedTex;
		}
		if (pov == initiator)
		{
			return def.iconDamagedFromInstigatorTex;
		}
		return def.iconDamagedTex;
	}

	protected override BodyDef DamagedBody()
	{
		if (recipientPawn == null)
		{
			return null;
		}
		return recipientPawn.RaceProps.body;
	}

	protected override GrammarRequest GenerateGrammarRequest()
	{
		GrammarRequest result = base.GenerateGrammarRequest();
		result.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, result.Constants));
		if (recipientPawn != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipientPawn, result.Constants));
		}
		else if (recipientThing != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForDef("RECIPIENT", recipientThing));
		}
		result.Includes.Add(ruleDef);
		if (!toolLabel.NullOrEmpty())
		{
			result.Rules.Add(new Rule_String("TOOL_label", toolLabel));
			result.Rules.Add(new Rule_String("TOOL_definite", Find.ActiveLanguageWorker.WithDefiniteArticle(toolLabel)));
			result.Rules.Add(new Rule_String("TOOL_indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(toolLabel)));
			result.Constants["TOOL_gender"] = LanguageDatabase.activeLanguage.ResolveGender(toolLabel).ToString();
		}
		if (implementType != null && !implementType.implementOwnerRuleName.NullOrEmpty())
		{
			if (ownerEquipmentDef != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForDef(implementType.implementOwnerRuleName, ownerEquipmentDef));
			}
			else if (ownerHediffDef != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForDef(implementType.implementOwnerRuleName, ownerHediffDef));
			}
		}
		if (initiator != null && initiator.skills != null)
		{
			result.Constants["INITIATOR_skill"] = initiator.skills.GetSkill(SkillDefOf.Melee).Level.ToStringCached();
		}
		if (recipientPawn != null && recipientPawn.skills != null)
		{
			result.Constants["RECIPIENT_skill"] = recipientPawn.skills.GetSkill(SkillDefOf.Melee).Level.ToStringCached();
		}
		if (implementType != null && !implementType.implementOwnerTypeValue.NullOrEmpty())
		{
			result.Constants["IMPLEMENTOWNER_type"] = implementType.implementOwnerTypeValue;
		}
		return result;
	}

	public override bool ShowInCompactView()
	{
		if (alwaysShowInCompact)
		{
			return true;
		}
		return Rand.ChanceSeeded(DisplayChanceOnMiss, logID);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref ruleDef, "ruleDef");
		Scribe_Values.Look(ref alwaysShowInCompact, "alwaysShowInCompact", defaultValue: false);
		Scribe_References.Look(ref initiator, "initiator", saveDestroyedThings: true);
		Scribe_References.Look(ref recipientPawn, "recipientPawn", saveDestroyedThings: true);
		Scribe_Defs.Look(ref recipientThing, "recipientThing");
		Scribe_Defs.Look(ref implementType, "implementType");
		Scribe_Defs.Look(ref ownerEquipmentDef, "ownerDef");
		Scribe_Values.Look(ref toolLabel, "toolLabel");
		BackCompatibility.PostExposeData(this);
	}

	public override string ToString()
	{
		return ruleDef.defName + ": " + InitiatorName + "->" + RecipientName;
	}
}
