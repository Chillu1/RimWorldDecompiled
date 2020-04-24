using System;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldObjectMaker
	{
		public static WorldObject MakeWorldObject(WorldObjectDef def)
		{
			WorldObject obj = (WorldObject)Activator.CreateInstance(def.worldObjectClass);
			obj.def = def;
			obj.ID = Find.UniqueIDsManager.GetNextWorldObjectID();
			obj.creationGameTicks = Find.TickManager.TicksGame;
			obj.PostMake();
			return obj;
		}
	}
}
