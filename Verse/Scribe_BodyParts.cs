namespace Verse;

public static class Scribe_BodyParts
{
	public static void Look(ref BodyPartRecord part, string label, BodyPartRecord defaultValue = null)
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			if (part == defaultValue || !Scribe.EnterNode(label))
			{
				return;
			}
			try
			{
				if (part == null)
				{
					Scribe.saver.WriteAttribute("IsNull", "True");
					return;
				}
				string value = part.body.defName;
				Scribe_Values.Look(ref value, "body");
				int value2 = part.Index;
				Scribe_Values.Look(ref value2, "index", 0, forceSave: true);
				return;
			}
			finally
			{
				Scribe.ExitNode();
			}
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			part = ScribeExtractor.BodyPartFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
		}
	}
}
