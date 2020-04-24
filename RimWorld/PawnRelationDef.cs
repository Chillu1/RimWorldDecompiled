using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnRelationDef : Def
	{
		public Type workerClass = typeof(PawnRelationWorker);

		[MustTranslate]
		public string labelFemale;

		public float importance;

		public bool implied;

		public bool reflexive;

		public int opinionOffset;

		public float generationChanceFactor;

		public float romanceChanceFactor = 1f;

		public float incestOpinionOffset;

		public bool familyByBloodRelation;

		public ThoughtDef diedThought;

		public ThoughtDef diedThoughtFemale;

		public ThoughtDef lostThought;

		public ThoughtDef lostThoughtFemale;

		public List<ThoughtDef> soldThoughts;

		public ThoughtDef killedThought;

		public ThoughtDef killedThoughtFemale;

		[Unsaved(false)]
		private PawnRelationWorker workerInt;

		public PawnRelationWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnRelationWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public string GetGenderSpecificLabel(Pawn pawn)
		{
			if (pawn.gender == Gender.Female && !labelFemale.NullOrEmpty())
			{
				return labelFemale;
			}
			return label;
		}

		public string GetGenderSpecificLabelCap(Pawn pawn)
		{
			return GetGenderSpecificLabel(pawn).CapitalizeFirst();
		}

		public ThoughtDef GetGenderSpecificDiedThought(Pawn killed)
		{
			if (killed.gender == Gender.Female && diedThoughtFemale != null)
			{
				return diedThoughtFemale;
			}
			return diedThought;
		}

		public ThoughtDef GetGenderSpecificLostThought(Pawn killed)
		{
			if (killed.gender == Gender.Female && diedThoughtFemale != null)
			{
				return lostThoughtFemale;
			}
			return lostThought;
		}

		public ThoughtDef GetGenderSpecificKilledThought(Pawn killed)
		{
			if (killed.gender == Gender.Female && killedThoughtFemale != null)
			{
				return killedThoughtFemale;
			}
			return killedThought;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (implied && reflexive)
			{
				yield return defName + ": implied relations can't use the \"reflexive\" option.";
				reflexive = false;
			}
		}
	}
}
