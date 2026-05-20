using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Grammar;

namespace Verse;

public class BattleLogEntry_ExplosionImpact : LogEntry_DamageResult
{
	private Pawn initiatorPawn;

	private ThingDef initiatorThing;

	private Pawn recipientPawn;

	private ThingDef recipientThing;

	private ThingDef weaponDef;

	private ThingDef projectileDef;

	private DamageDef damageDef;

	private string InitiatorName
	{
		get
		{
			if (initiatorPawn != null)
			{
				return initiatorPawn.LabelShort;
			}
			if (initiatorThing != null)
			{
				return initiatorThing.defName;
			}
			return "null";
		}
	}

	private string RecipientName
	{
		get
		{
			if (recipientPawn != null)
			{
				return recipientPawn.LabelShort;
			}
			if (recipientThing != null)
			{
				return recipientThing.defName;
			}
			return "null";
		}
	}

	public BattleLogEntry_ExplosionImpact()
	{
	}

	public BattleLogEntry_ExplosionImpact(Thing initiator, Thing recipient, ThingDef weaponDef, ThingDef projectileDef, DamageDef damageDef)
	{
		if (initiator is Pawn)
		{
			initiatorPawn = initiator as Pawn;
		}
		else if (initiator != null)
		{
			initiatorThing = initiator.def;
		}
		if (recipient is Pawn)
		{
			recipientPawn = recipient as Pawn;
		}
		else if (recipient != null)
		{
			recipientThing = recipient.def;
		}
		this.weaponDef = weaponDef;
		this.projectileDef = projectileDef;
		this.damageDef = damageDef;
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
		if (pov != initiatorPawn || recipientPawn == null || !CameraJumper.CanJump(recipientPawn))
		{
			if (pov == recipientPawn)
			{
				return CameraJumper.CanJump(initiatorPawn);
			}
			return false;
		}
		return true;
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

	public override Texture2D IconFromPOV(Thing pov)
	{
		if (damagedParts.NullOrEmpty())
		{
			return null;
		}
		if (pov == null || pov == recipientPawn)
		{
			return LogEntry.Blood;
		}
		if (pov == initiatorPawn)
		{
			return LogEntry.BloodTarget;
		}
		return null;
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
		result.Includes.Add(RulePackDefOf.Combat_ExplosionImpact);
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
		if (projectileDef != null)
		{
			result.Rules.AddRange(GrammarUtility.RulesForDef("PROJECTILE", projectileDef));
		}
		if (damageDef != null && damageDef.combatLogRules != null)
		{
			result.Includes.Add(damageDef.combatLogRules);
		}
		return result;
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
		Scribe_Defs.Look(ref damageDef, "damageDef");
	}

	public override string ToString()
	{
		return "BattleLogEntry_ExplosionImpact: " + InitiatorName + "->" + RecipientName;
	}
}
