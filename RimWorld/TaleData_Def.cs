using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Def : TaleData
	{
		public Def def;

		private string tmpDefName;

		private Type tmpDefType;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				tmpDefName = ((def != null) ? def.defName : null);
				tmpDefType = ((def != null) ? def.GetType() : null);
			}
			Scribe_Values.Look(ref tmpDefName, "defName");
			Scribe_Values.Look(ref tmpDefType, "defType");
			if (Scribe.mode == LoadSaveMode.LoadingVars && tmpDefName != null)
			{
				def = GenDefDatabase.GetDef(tmpDefType, BackCompatibility.BackCompatibleDefName(tmpDefType, tmpDefName));
			}
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			if (def != null)
			{
				yield return new Rule_String(prefix + "_label", def.label);
				yield return new Rule_String(prefix + "_definite", Find.ActiveLanguageWorker.WithDefiniteArticle(def.label));
				yield return new Rule_String(prefix + "_indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label));
			}
		}

		public static TaleData_Def GenerateFrom(Def def)
		{
			return new TaleData_Def
			{
				def = def
			};
		}
	}
}
