using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI.Group
{
	public abstract class LordToil
	{
		public Lord lord;

		public LordToilData data;

		private List<Func<bool>> failConditions = new List<Func<bool>>();

		public bool useAvoidGrid;

		public Map Map => lord.lordManager.map;

		public virtual IntVec3 FlagLoc => IntVec3.Invalid;

		public virtual bool AllowSatisfyLongNeeds => true;

		public virtual float? CustomWakeThreshold => null;

		public virtual bool AllowRestingInBed => true;

		public virtual bool AllowSelfTend => true;

		public virtual bool ShouldFail
		{
			get
			{
				for (int i = 0; i < failConditions.Count; i++)
				{
					if (failConditions[i]())
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual bool ForceHighStoryDanger => false;

		public virtual void Init()
		{
		}

		public abstract void UpdateAllDuties();

		public virtual void LordToilTick()
		{
		}

		public virtual void Cleanup()
		{
		}

		public virtual ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			return ThinkTreeDutyHook.None;
		}

		public virtual void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
		{
		}

		public virtual void Notify_BuildingLost(Building b)
		{
		}

		public virtual void Notify_ReachedDutyLocation(Pawn pawn)
		{
		}

		public virtual void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
		}

		public void AddFailCondition(Func<bool> failCondition)
		{
			failConditions.Add(failCondition);
		}

		public override string ToString()
		{
			string text = GetType().ToString();
			if (text.Contains('.'))
			{
				text = text.Substring(text.LastIndexOf('.') + 1);
			}
			if (text.Contains('_'))
			{
				text = text.Substring(text.LastIndexOf('_') + 1);
			}
			return text;
		}
	}
}
