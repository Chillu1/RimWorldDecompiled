using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InstructionDef : Def
	{
		public Type instructionClass = typeof(Instruction_Basic);

		[MustTranslate]
		public string text;

		public bool startCentered;

		public bool tutorialModeOnly = true;

		[NoTranslate]
		public string eventTagInitiate;

		public InstructionDef eventTagInitiateSource;

		[NoTranslate]
		public List<string> eventTagsEnd;

		[NoTranslate]
		public List<string> actionTagsAllowed;

		[MustTranslate]
		public string rejectInputMessage;

		public ConceptDef concept;

		[NoTranslate]
		public List<string> highlightTags;

		[MustTranslate]
		public string onMapInstruction;

		public int targetCount;

		public ThingDef thingDef;

		public RecipeDef recipeDef;

		public int recipeTargetCount = 1;

		public ThingDef giveOnActivateDef;

		public int giveOnActivateCount;

		public bool endTutorial;

		public bool resetBuildDesignatorStuffs;

		private static List<string> tmpParseErrors = new List<string>();

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (instructionClass == null)
			{
				yield return "no instruction class";
			}
			if (text.NullOrEmpty())
			{
				yield return "no text";
			}
			if (eventTagInitiate.NullOrEmpty())
			{
				yield return "no eventTagInitiate";
			}
			tmpParseErrors.Clear();
			text.AdjustedForKeys(tmpParseErrors, resolveKeys: false);
			for (int i = 0; i < tmpParseErrors.Count; i++)
			{
				yield return "text error: " + tmpParseErrors[i];
			}
		}
	}
}
