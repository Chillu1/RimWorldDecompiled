namespace Verse
{
	public static class Scribe_Values
	{
		public static void Look<T>(ref T value, string label, T defaultValue = default(T), bool forceSave = false)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (typeof(T) == typeof(TargetInfo))
				{
					Log.Error("Saving a TargetInfo " + label + " with Scribe_Values. TargetInfos must be saved with Scribe_TargetInfo.");
				}
				else if (typeof(Thing).IsAssignableFrom(typeof(T)))
				{
					Log.Error("Using Scribe_Values with a Thing reference " + label + ". Use Scribe_References or Scribe_Deep instead.");
				}
				else if (typeof(IExposable).IsAssignableFrom(typeof(T)))
				{
					Log.Error("Using Scribe_Values with a IExposable reference " + label + ". Use Scribe_References or Scribe_Deep instead.");
				}
				else if (typeof(Def).IsAssignableFrom(typeof(T)))
				{
					Log.Error("Using Scribe_Values with a Def " + label + ". Use Scribe_Defs instead.");
				}
				else
				{
					if (!forceSave && (value != null || defaultValue == null) && (value == null || value.Equals(defaultValue)))
					{
						return;
					}
					if (value == null)
					{
						if (Scribe.EnterNode(label))
						{
							try
							{
								Scribe.saver.WriteAttribute("IsNull", "True");
							}
							finally
							{
								Scribe.ExitNode();
							}
						}
					}
					else
					{
						Scribe.saver.WriteElement(label, value.ToString());
					}
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.ValueFromNode(Scribe.loader.curXmlParent[label], defaultValue);
			}
		}
	}
}
