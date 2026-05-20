using System;
using Verse;

namespace RimWorld
{
	public static class ShipJobMaker
	{
		public static ShipJob MakeShipJob(ShipJobDef def)
		{
			ShipJob obj = (ShipJob)Activator.CreateInstance(def.jobClass);
			obj.def = def;
			obj.loadID = Find.UniqueIDsManager.GetNextShipJobID();
			return obj;
		}
	}
}
