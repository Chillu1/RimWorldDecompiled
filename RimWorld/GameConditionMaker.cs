using System;
using Verse;

namespace RimWorld
{
	public static class GameConditionMaker
	{
		public static GameCondition MakeConditionPermanent(GameConditionDef def)
		{
			GameCondition gameCondition = MakeCondition(def);
			gameCondition.Permanent = true;
			gameCondition.startTick -= 180000;
			return gameCondition;
		}

		public static GameCondition MakeCondition(GameConditionDef def, int duration = -1)
		{
			GameCondition obj = (GameCondition)Activator.CreateInstance(def.conditionClass);
			obj.startTick = Find.TickManager.TicksGame;
			obj.def = def;
			obj.Duration = duration;
			obj.uniqueID = Find.UniqueIDsManager.GetNextGameConditionID();
			obj.PostMake();
			return obj;
		}
	}
}
