using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class LetterMaker
	{
		public static Letter MakeLetter(LetterDef def)
		{
			Letter obj = (Letter)Activator.CreateInstance(def.letterClass);
			obj.def = def;
			obj.ID = Find.UniqueIDsManager.GetNextLetterID();
			return obj;
		}

		public static ChoiceLetter MakeLetter(TaggedString label, TaggedString text, LetterDef def, Faction relatedFaction = null, Quest quest = null)
		{
			if (!typeof(ChoiceLetter).IsAssignableFrom(def.letterClass))
			{
				Log.Error(string.Concat(def, " is not a choice letter."));
				return null;
			}
			ChoiceLetter obj = (ChoiceLetter)MakeLetter(def);
			obj.label = label;
			obj.text = text;
			obj.relatedFaction = relatedFaction;
			obj.quest = quest;
			return obj;
		}

		public static ChoiceLetter MakeLetter(TaggedString label, TaggedString text, LetterDef def, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null)
		{
			ChoiceLetter choiceLetter = MakeLetter(label, text, def);
			choiceLetter.lookTargets = lookTargets;
			choiceLetter.relatedFaction = relatedFaction;
			choiceLetter.quest = quest;
			choiceLetter.hyperlinkThingDefs = hyperlinkThingDefs;
			return choiceLetter;
		}
	}
}
