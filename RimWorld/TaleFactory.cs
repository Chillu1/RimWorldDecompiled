using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TaleFactory
	{
		public static Tale MakeRawTale(TaleDef def, params object[] args)
		{
			try
			{
				Tale obj = (Tale)Activator.CreateInstance(def.taleClass, args);
				obj.def = def;
				obj.id = Find.UniqueIDsManager.GetNextTaleID();
				obj.date = Find.TickManager.TicksAbs;
				return obj;
			}
			catch (Exception arg2)
			{
				Log.Error($"Failed to create tale object {def} with parameters {args.Select((object arg) => arg.ToStringSafe()).ToCommaList()}: {arg2}");
				return null;
			}
		}

		public static Tale MakeRandomTestTale(TaleDef def = null)
		{
			if (def == null)
			{
				def = DefDatabase<TaleDef>.AllDefs.Where((TaleDef d) => d.usableForArt).RandomElement();
			}
			Tale tale = MakeRawTale(def);
			tale.GenerateTestData();
			return tale;
		}
	}
}
