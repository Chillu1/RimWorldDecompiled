using System;

namespace RimWorld
{
	public static class ThoughtMaker
	{
		public static Thought MakeThought(ThoughtDef def)
		{
			Thought obj = (Thought)Activator.CreateInstance(def.ThoughtClass);
			obj.def = def;
			obj.Init();
			return obj;
		}

		public static Thought_Memory MakeThought(ThoughtDef def, int forcedStage)
		{
			Thought_Memory obj = (Thought_Memory)Activator.CreateInstance(def.ThoughtClass);
			obj.def = def;
			obj.SetForcedStage(forcedStage);
			obj.Init();
			return obj;
		}
	}
}
