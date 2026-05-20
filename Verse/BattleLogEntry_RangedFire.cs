using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse.Grammar;

namespace Verse;

public class BattleLogEntry_RangedFire : LogEntry
{
	private Pawn initiatorPawn;

	private ThingDef initiatorThing;

	private Pawn recipientPawn;

	private ThingDef recipientThing;

	private ThingDef weaponDef;

	private ThingDef projectileDef;

	private bool burst;

	[TweakValue("LogFilter", 0f, 1f)]
	private static float DisplayChance = 0.25f;

	private string InitiatorName
	{
		get
		{
			if (initiatorPawn == null)
			{
				return "null";
			}
			return initiatorPawn.LabelShort;
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

	public BattleLogEntry_RangedFire()
	{
	}

	public BattleLogEntry_RangedFire(Thing initiator, Thing target, ThingDef weaponDef, ThingDef projectileDef, bool burst)
	{
		if (initiator is Pawn)
		{
			initiatorPawn = initiator as Pawn;
		}
		else if (initiator != null)
		{
			initiatorThing = initiator.def;
		}
		if (target is Pawn)
		{
			recipientPawn = target as Pawn;
		}
		else if (target != null)
		{
			recipientThing = target.def;
		}
		this.weaponDef = weaponDef;
		this.projectileDef = projectileDef;
		this.burst = burst;
	}

	public override bool Concerns(Thing t)
	{
		if (t != initiatorPawn)
		{
			return t == recipientPawn;
		}
		return true;
	}

	public override IEnumerable<Thing> GetConcerns()
	{
		if (initiatorPawn != null)
		{
			yield return initiatorPawn;
		}
		if (recipientPawn != null)
		{
			yield return recipientPawn;
		}
	}

	public override bool CanBeClickedFromPOV(Thing pov)
	{
		if (recipientPawn != null)
		{
			if (pov != initiatorPawn || !CameraJumper.CanJump(recipientPawn))
			{
				if (pov == recipientPawn)
				{
					return CameraJumper.CanJump(initiatorPawn);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override void ClickedFromPOV(Thing pov)
	{
		if (recipientPawn == null)
		{
			return;
		}
		if (pov == initiatorPawn)
		{
			CameraJumper.TryJumpAndSelect(recipientPawn);
			return;
		}
		if (pov == recipientPawn)
		{
			CameraJumper.TryJumpAndSelect(initiatorPawn);
			return;
		}
		throw new NotImplementedException();
	}

	protected override GrammarRequest GenerateGrammarRequest()
	{
		GrammarRequest result = base.GenerateGrammarRequest();
		if (initiatorPawn == null && initiatorThing == null)
		{
			Log.ErrorOnce("BattleLogEntry_RangedFire has a null initiator.", 60465709);
		}
		if (weaponDef != null && weaponDef.Verbs[0].rangedFireRulepack != null)
		{
			result.Includes.Add(weaponDef.Verbs[0].rangedFireRulepack);
		}
		else
		{
			result.Includes.Add(RulePackDefOf.Combat_RangedFire);
		}
		if (initiatorPawn != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiatorPawn, result.Constants));
		}
		else if (initiatorThing != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForDef("INITIATOR", initiatorThing));
		}
		else
		{
			result.Constants["INITIATOR_missing"] = "True";
		}
		if (recipientPawn != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipientPawn, result.Constants));
		}
		else if (recipientThing != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForDef("RECIPIENT", recipientThing));
		}
		else
		{
			result.Constants["RECIPIENT_missing"] = "True";
		}
		result.Rules.AddRange(PlayLogEntryUtility.RulesForOptionalWeapon("WEAPON", weaponDef, projectileDef));
		if (initiatorPawn != null && initiatorPawn.skills != null)
		{
			result.Constants["INITIATOR_skill"] = initiatorPawn.skills.GetSkill(SkillDefOf.Shooting).Level.ToStringCached();
		}
		if (recipientPawn != null && recipientPawn.skills != null)
		{
			result.Constants["RECIPIENT_skill"] = recipientPawn.skills.GetSkill(SkillDefOf.Shooting).Level.ToStringCached();
		}
		result.Constants["BURST"] = burst.ToString();
		return result;
	}

	public override bool ShowInCompactView()
	{
		return Rand.ChanceSeeded(DisplayChance, logID);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref initiatorPawn, "initiatorPawn", saveDestroyedThings: true);
		Scribe_Defs.Look(ref initiatorThing, "initiatorThing");
		Scribe_References.Look(ref recipientPawn, "recipientPawn", saveDestroyedThings: true);
		Scribe_Defs.Look(ref recipientThing, "recipientThing");
		Scribe_Defs.Look(ref weaponDef, "weaponDef");
		Scribe_Defs.Look(ref projectileDef, "projectileDef");
		Scribe_Values.Look(ref burst, "burst", defaultValue: false);
	}

	public override string ToString()
	{
		return "BattleLogEntry_RangedFire: " + InitiatorName + "->" + RecipientName;
	}
}
