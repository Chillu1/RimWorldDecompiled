using System;
using System.Collections.Generic;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestTextRequest
	{
		public string keyword;

		public Action<string> setter;

		public List<Rule> extraRules;
	}
}
