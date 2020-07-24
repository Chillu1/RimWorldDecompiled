using System;

namespace Verse
{
	public class Scribe_Deep
	{
		public static void Look<T>(ref T target, string label, params object[] ctorArgs)
		{
			Look(ref target, saveDestroyedThings: false, label, ctorArgs);
		}

		public static void Look<T>(ref T target, bool saveDestroyedThings, string label, params object[] ctorArgs)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Thing thing = target as Thing;
				if (thing != null && thing.Destroyed)
				{
					if (!saveDestroyedThings)
					{
						Log.Warning(string.Concat("Deep-saving destroyed thing ", thing, " with saveDestroyedThings==false. label=", label));
					}
					else if (thing.Discarded)
					{
						Log.Warning(string.Concat("Deep-saving discarded thing ", thing, ". This mode means that the thing is no longer managed by anything in the code and should not be deep-saved anywhere. (even with saveDestroyedThings==true) , label=", label));
					}
				}
				IExposable exposable = target as IExposable;
				if (target != null && exposable == null)
				{
					Log.Error("Cannot use LookDeep to save non-IExposable non-null " + label + " of type " + typeof(T));
					return;
				}
				if (target == null)
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
				else if (Scribe.EnterNode(label))
				{
					try
					{
						if (target.GetType() != typeof(T) || typeof(T).IsGenericTypeDefinition)
						{
							Scribe.saver.WriteAttribute("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(target.GetType()));
						}
						exposable.ExposeData();
					}
					catch (OutOfMemoryException)
					{
						throw;
					}
					catch (Exception ex2)
					{
						Log.Error("Exception while saving " + exposable.ToStringSafe() + ": " + ex2);
					}
					finally
					{
						Scribe.ExitNode();
					}
				}
				Scribe.saver.loadIDsErrorsChecker.RegisterDeepSaved(target, label);
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				try
				{
					target = ScribeExtractor.SaveableFromNode<T>(Scribe.loader.curXmlParent[label], ctorArgs);
				}
				catch (Exception ex3)
				{
					Log.Error("Exception while loading " + Scribe.loader.curXmlParent[label].ToStringSafe() + ": " + ex3);
					target = default(T);
				}
			}
		}
	}
}
