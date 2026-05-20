using System;

namespace Verse;

public static class HediffMaker
{
	public static Hediff MakeHediff(HediffDef def, Pawn pawn, BodyPartRecord partRecord = null)
	{
		if (pawn == null)
		{
			Log.Error("Cannot make hediff " + def?.ToString() + " for null pawn.");
			return null;
		}
		Hediff obj = (Hediff)Activator.CreateInstance(def.hediffClass);
		obj.def = def;
		obj.pawn = pawn;
		obj.Part = partRecord;
		obj.loadID = Find.UniqueIDsManager.GetNextHediffID();
		obj.PostMake();
		return obj;
	}

	public static Hediff Debug_MakeConcreteExampleHediff(HediffDef def)
	{
		Hediff obj = (Hediff)Activator.CreateInstance(def.hediffClass);
		obj.def = def;
		obj.loadID = Find.UniqueIDsManager.GetNextHediffID();
		obj.PostMake();
		return obj;
	}
}
