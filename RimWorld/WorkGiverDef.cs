using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiverDef : Def
	{
		public Type giverClass;

		public WorkTypeDef workType;

		public WorkTags workTags;

		public int priorityInType;

		[MustTranslate]
		public string verb;

		[MustTranslate]
		public string gerund;

		public bool scanThings = true;

		public bool scanCells;

		public bool emergency;

		public List<PawnCapacityDef> requiredCapacities = new List<PawnCapacityDef>();

		public bool directOrderable = true;

		public bool prioritizeSustains;

		public bool nonColonistsCanDo;

		public JobTag tagToGive = JobTag.MiscWork;

		public WorkGiverEquivalenceGroupDef equivalenceGroup;

		public bool canBeDoneWhileDrafted;

		public int autoTakeablePriorityDrafted = -1;

		public ThingDef forceMote;

		public List<ThingDef> fixedBillGiverDefs;

		public bool billGiversAllHumanlikes;

		public bool billGiversAllHumanlikesCorpses;

		public bool billGiversAllMechanoids;

		public bool billGiversAllMechanoidsCorpses;

		public bool billGiversAllAnimals;

		public bool billGiversAllAnimalsCorpses;

		public bool tendToHumanlikesOnly;

		public bool tendToAnimalsOnly;

		public bool feedHumanlikesOnly;

		public bool feedAnimalsOnly;

		public ThingDef scannerDef;

		[Unsaved(false)]
		private WorkGiver workerInt;

		public WorkGiver Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (WorkGiver)Activator.CreateInstance(giverClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (verb.NullOrEmpty())
			{
				yield return defName + " lacks a verb.";
			}
			if (gerund.NullOrEmpty())
			{
				yield return defName + " lacks a gerund.";
			}
		}
	}
}
