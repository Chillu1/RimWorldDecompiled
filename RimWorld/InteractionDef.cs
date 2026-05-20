using System;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class InteractionDef : Def
{
	private Type workerClass = typeof(InteractionWorker);

	public ThingDef interactionMote;

	public float socialFightBaseChance;

	public ThoughtDef initiatorThought;

	public SkillDef initiatorXpGainSkill;

	public int initiatorXpGainAmount;

	public ThoughtDef recipientThought;

	public SkillDef recipientXpGainSkill;

	public int recipientXpGainAmount;

	public bool ignoreTimeSinceLastInteraction;

	[NoTranslate]
	private string symbol;

	public InteractionSymbolSource symbolSource;

	public RulePack logRulesInitiator;

	public RulePack logRulesRecipient;

	[Unsaved(false)]
	private InteractionWorker workerInt;

	[Unsaved(false)]
	private Texture2D symbolTex;

	public InteractionWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (InteractionWorker)Activator.CreateInstance(workerClass);
				workerInt.interaction = this;
			}
			return workerInt;
		}
	}

	private Texture2D Symbol
	{
		get
		{
			if (symbolTex == null)
			{
				symbolTex = ContentFinder<Texture2D>.Get(symbol);
			}
			return symbolTex;
		}
	}

	public Texture2D GetSymbol(Faction initiatorFaction = null, Ideo initatorIdeo = null)
	{
		switch (symbolSource)
		{
		case InteractionSymbolSource.InitiatorIdeo:
			if (Find.IdeoManager.classicMode)
			{
				return Symbol;
			}
			return initatorIdeo?.Icon;
		case InteractionSymbolSource.InitiatorFaction:
			return initiatorFaction?.def.FactionIcon;
		default:
			return Symbol;
		}
	}

	public Color? GetSymbolColor(Faction initiatorFaction = null)
	{
		if (initiatorFaction != null && symbolSource == InteractionSymbolSource.InitiatorFaction)
		{
			return initiatorFaction.Color;
		}
		return null;
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (interactionMote == null)
		{
			interactionMote = ThingDefOf.Mote_Speech;
		}
	}
}
