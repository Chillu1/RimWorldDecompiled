using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Grammar;

namespace Verse;

public class PlayLogEntry_Interaction : LogEntry
{
	protected InteractionDef intDef;

	protected Pawn initiator;

	protected Pawn recipient;

	protected List<RulePackDef> extraSentencePacks;

	public Faction initiatorFaction;

	public Ideo initiatorIdeo;

	protected string InitiatorName
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
			if (recipient == null)
			{
				return "null";
			}
			return recipient.LabelShort;
		}
	}

	public PlayLogEntry_Interaction()
	{
	}

	public PlayLogEntry_Interaction(InteractionDef intDef, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
	{
		this.intDef = intDef;
		this.initiator = initiator;
		this.recipient = recipient;
		this.extraSentencePacks = extraSentencePacks;
		initiatorFaction = initiator.Faction;
		initiatorIdeo = initiator.Ideo;
	}

	public override bool Concerns(Thing t)
	{
		if (t != initiator)
		{
			return t == recipient;
		}
		return true;
	}

	public override IEnumerable<Thing> GetConcerns()
	{
		if (initiator != null)
		{
			yield return initiator;
		}
		if (recipient != null)
		{
			yield return recipient;
		}
	}

	public override bool CanBeClickedFromPOV(Thing pov)
	{
		if (pov != recipient || !CameraJumper.CanJump(initiator))
		{
			if (pov == initiator)
			{
				return CameraJumper.CanJump(recipient);
			}
			return false;
		}
		return true;
	}

	public override void ClickedFromPOV(Thing pov)
	{
		if (pov == initiator)
		{
			CameraJumper.TryJumpAndSelect(recipient);
			return;
		}
		if (pov == recipient)
		{
			CameraJumper.TryJumpAndSelect(initiator);
			return;
		}
		throw new NotImplementedException();
	}

	public override Texture2D IconFromPOV(Thing pov)
	{
		return intDef.GetSymbol(initiatorFaction, initiatorIdeo);
	}

	public override Color? IconColorFromPOV(Thing pov)
	{
		return intDef.GetSymbolColor(initiatorFaction);
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (initiatorFaction == faction)
		{
			initiatorFaction = null;
		}
	}

	public override void Notify_IdeoRemoved(Ideo ideo)
	{
		if (initiatorIdeo == ideo)
		{
			initiatorIdeo = null;
		}
	}

	public override string GetTipString()
	{
		return intDef.LabelCap + "\n" + base.GetTipString();
	}

	protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
	{
		if (initiator == null || recipient == null)
		{
			Log.ErrorOnce("PlayLogEntry_Interaction has a null pawn reference.", 34422);
			return "[" + intDef.label + " error: null pawn reference]";
		}
		Rand.PushState();
		Rand.Seed = logID;
		GrammarRequest request = base.GenerateGrammarRequest();
		string text;
		if (pov == initiator)
		{
			request.IncludesBare.Add(intDef.logRulesInitiator);
			request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));
			request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipient, request.Constants));
			text = GrammarResolver.Resolve("r_logentry", request, "interaction from initiator", forceLog);
		}
		else if (pov == recipient)
		{
			if (intDef.logRulesRecipient != null)
			{
				request.IncludesBare.Add(intDef.logRulesRecipient);
			}
			else
			{
				request.IncludesBare.Add(intDef.logRulesInitiator);
			}
			request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));
			request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipient, request.Constants));
			text = GrammarResolver.Resolve("r_logentry", request, "interaction from recipient", forceLog);
		}
		else
		{
			Log.ErrorOnce("Cannot display PlayLogEntry_Interaction from POV who isn't initiator or recipient.", 51251);
			text = ToString();
		}
		if (extraSentencePacks != null)
		{
			for (int i = 0; i < extraSentencePacks.Count; i++)
			{
				request.Clear();
				request.Includes.Add(extraSentencePacks[i]);
				request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));
				request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipient, request.Constants));
				text = text + " " + GrammarResolver.Resolve(extraSentencePacks[i].FirstRuleKeyword, request, "extraSentencePack", forceLog, extraSentencePacks[i].FirstUntranslatedRuleKeyword);
			}
		}
		Rand.PopState();
		return text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref intDef, "intDef");
		Scribe_References.Look(ref initiator, "initiator", saveDestroyedThings: true);
		Scribe_References.Look(ref recipient, "recipient", saveDestroyedThings: true);
		Scribe_Collections.Look(ref extraSentencePacks, "extras", LookMode.Undefined);
		Scribe_References.Look(ref initiatorFaction, "initiatorFaction");
		Scribe_References.Look(ref initiatorIdeo, "initiatorIdeo");
	}

	public override string ToString()
	{
		return intDef.label + ": " + InitiatorName + "->" + RecipientName;
	}
}
