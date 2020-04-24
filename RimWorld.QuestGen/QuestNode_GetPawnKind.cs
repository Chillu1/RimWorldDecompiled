using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetPawnKind : QuestNode
	{
		public class Option
		{
			public PawnKindDef kindDef;

			public float weight;

			public bool anyAnimal;

			public FleshTypeDef onlyAllowedFleshType;
		}

		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<List<Option>> options;

		protected override bool TestRunInt(Slate slate)
		{
			SetVars(slate);
			return true;
		}

		protected override void RunInt()
		{
			SetVars(QuestGen.slate);
		}

		private void SetVars(Slate slate)
		{
			Option option = options.GetValue(slate).RandomElementByWeight((Option x) => x.weight);
			slate.Set<PawnKindDef>(var: (option.kindDef != null) ? option.kindDef : ((!option.anyAnimal) ? null : DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.RaceProps.Animal && (option.onlyAllowedFleshType == null || x.RaceProps.FleshType == option.onlyAllowedFleshType)).RandomElement()), name: storeAs.GetValue(slate), isAbsoluteName: false);
		}
	}
}
