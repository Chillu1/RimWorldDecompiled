namespace Verse;

public static class Scribe_Defs
{
	public static void Look<T>(ref T value, string label) where T : Def, new()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			string value2 = ((value != null) ? value.defName : "null");
			Scribe_Values.Look(ref value2, label, "null");
		}
		else if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			value = ScribeExtractor.DefFromNode<T>(Scribe.loader.curXmlParent[label]);
		}
	}
}
