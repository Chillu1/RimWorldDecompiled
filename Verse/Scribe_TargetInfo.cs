using RimWorld.Planet;

namespace Verse
{
	public static class Scribe_TargetInfo
	{
		public static void Look(ref LocalTargetInfo value, string label)
		{
			Look(ref value, saveDestroyedThings: false, label, LocalTargetInfo.Invalid);
		}

		public static void Look(ref LocalTargetInfo value, bool saveDestroyedThings, string label)
		{
			Look(ref value, saveDestroyedThings, label, LocalTargetInfo.Invalid);
		}

		public static void Look(ref LocalTargetInfo value, string label, LocalTargetInfo defaultValue)
		{
			Look(ref value, saveDestroyedThings: false, label, defaultValue);
		}

		public static void Look(ref LocalTargetInfo value, bool saveDestroyedThings, string label, LocalTargetInfo defaultValue)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!value.Equals(defaultValue) && (value.Thing == null || !Scribe_References.CheckSaveReferenceToDestroyedThing(value.Thing, label, saveDestroyedThings)))
				{
					Scribe.saver.WriteElement(label, value.ToString());
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.LocalTargetInfoFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveLocalTargetInfo(value, label);
			}
		}

		public static void Look(ref TargetInfo value, string label)
		{
			Look(ref value, saveDestroyedThings: false, label, TargetInfo.Invalid);
		}

		public static void Look(ref TargetInfo value, bool saveDestroyedThings, string label)
		{
			Look(ref value, saveDestroyedThings, label, TargetInfo.Invalid);
		}

		public static void Look(ref TargetInfo value, string label, TargetInfo defaultValue)
		{
			Look(ref value, saveDestroyedThings: false, label, defaultValue);
		}

		public static void Look(ref TargetInfo value, bool saveDestroyedThings, string label, TargetInfo defaultValue)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!value.Equals(defaultValue) && (value.Thing == null || !Scribe_References.CheckSaveReferenceToDestroyedThing(value.Thing, label, saveDestroyedThings)))
				{
					if (!value.HasThing && value.Cell.IsValid && (value.Map == null || !Find.Maps.Contains(value.Map)))
					{
						Scribe.saver.WriteElement(label, "null");
					}
					else
					{
						Scribe.saver.WriteElement(label, value.ToString());
					}
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.TargetInfoFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveTargetInfo(value, label);
			}
		}

		public static void Look(ref GlobalTargetInfo value, string label)
		{
			Look(ref value, saveDestroyedThings: false, label, GlobalTargetInfo.Invalid);
		}

		public static void Look(ref GlobalTargetInfo value, bool saveDestroyedThings, string label)
		{
			Look(ref value, saveDestroyedThings, label, GlobalTargetInfo.Invalid);
		}

		public static void Look(ref GlobalTargetInfo value, string label, GlobalTargetInfo defaultValue)
		{
			Look(ref value, saveDestroyedThings: false, label, defaultValue);
		}

		public static void Look(ref GlobalTargetInfo value, bool saveDestroyedThings, string label, GlobalTargetInfo defaultValue)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!value.Equals(defaultValue) && (value.Thing == null || !Scribe_References.CheckSaveReferenceToDestroyedThing(value.Thing, label, saveDestroyedThings)) && (value.WorldObject == null || !Scribe_References.CheckSaveReferenceToDestroyedWorldObject(value.WorldObject, label, saveDestroyedThings)))
				{
					if (!value.HasThing && !value.HasWorldObject && value.Cell.IsValid && (value.Map == null || !Find.Maps.Contains(value.Map)))
					{
						Scribe.saver.WriteElement(label, "null");
					}
					else
					{
						Scribe.saver.WriteElement(label, value.ToString());
					}
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				value = ScribeExtractor.GlobalTargetInfoFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				value = ScribeExtractor.ResolveGlobalTargetInfo(value, label);
			}
		}
	}
}
