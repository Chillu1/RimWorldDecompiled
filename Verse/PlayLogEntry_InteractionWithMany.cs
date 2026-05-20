using System;
using System.Collections.Generic;
using RimWorld;
using Verse.Grammar;

namespace Verse;

public class PlayLogEntry_InteractionWithMany : PlayLogEntry_Interaction
{
	private List<Pawn> recipients;

	public PlayLogEntry_InteractionWithMany()
	{
	}

	public PlayLogEntry_InteractionWithMany(InteractionDef intDef, Pawn initiator, List<Pawn> recipients, List<RulePackDef> extraSentencePacks)
	{
		base.intDef = intDef;
		base.initiator = initiator;
		this.recipients = recipients;
		base.extraSentencePacks = extraSentencePacks;
		initiatorFaction = initiator.Faction;
		initiatorIdeo = initiator.Ideo;
	}

	public override bool Concerns(Thing t)
	{
		if (t is Pawn item)
		{
			if (t != initiator)
			{
				return recipients.Contains(item);
			}
			return true;
		}
		return false;
	}

	public override IEnumerable<Thing> GetConcerns()
	{
		yield return initiator;
		foreach (Pawn recipient in recipients)
		{
			yield return recipient;
		}
	}

	protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
	{
		if (initiator == null || recipients == null || recipients.Contains(null))
		{
			Log.ErrorOnce("PlayLogEntry_Interaction has a null pawn reference.", 34422);
			return "[" + intDef.label + " error: null pawn reference]";
		}
		Rand.PushState();
		Rand.Seed = logID;
		GrammarRequest request = base.GenerateGrammarRequest();
		string result;
		if (pov == initiator)
		{
			request.IncludesBare.Add(intDef.logRulesInitiator);
			request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));
			result = GrammarResolver.Resolve("r_logentry", request, "interaction from initiator", forceLog);
		}
		else if (pov is Pawn pawn && recipients.Contains(pawn))
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
			request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", pawn, request.Constants));
			result = GrammarResolver.Resolve("r_logentry", request, "interaction from recipient", forceLog);
		}
		else
		{
			Log.ErrorOnce("Cannot display PlayLogEntry_Interaction from POV who isn't initiator or recipient.", 51251);
			result = ToString();
		}
		Rand.PopState();
		return result;
	}

	public override bool CanBeClickedFromPOV(Thing pov)
	{
		if (recipients.Contains(pov as Pawn))
		{
			return CameraJumper.CanJump(initiator);
		}
		return false;
	}

	public override void ClickedFromPOV(Thing pov)
	{
		if (pov == initiator)
		{
			CameraJumper.TryJumpAndSelect(recipients.RandomElement());
			return;
		}
		if (recipients.Contains(pov as Pawn))
		{
			CameraJumper.TryJumpAndSelect(initiator);
			return;
		}
		throw new NotImplementedException();
	}

	public override string ToString()
	{
		return intDef.label + ": " + base.InitiatorName + "-> Many";
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref recipients, "recipients", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			recipients.RemoveAll((Pawn x) => x == null);
		}
	}
}
