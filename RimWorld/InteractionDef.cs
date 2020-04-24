using System;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
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

		[NoTranslate]
		private string symbol;

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

		public Texture2D Symbol
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

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (interactionMote == null)
			{
				interactionMote = ThingDefOf.Mote_Speech;
			}
		}
	}
}
