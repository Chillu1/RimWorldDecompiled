using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompUsable : ThingComp
	{
		public CompProperties_Usable Props => (CompProperties_Usable)props;

		protected virtual string FloatMenuOptionLabel(Pawn pawn)
		{
			return Props.useLabel;
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
		{
			if (!CanBeUsedBy(myPawn, out string failReason))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + ((failReason != null) ? (" (" + failReason + ")") : ""), null);
			}
			else if (!myPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + " (" + "NoPath".Translate() + ")", null);
			}
			else if (!myPawn.CanReserve(parent))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + " (" + "Reserved".Translate() + ")", null);
			}
			else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + " (" + "Incapable".Translate() + ")", null);
			}
			else
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn), delegate
				{
					if (myPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly))
					{
						foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
						{
							if (comp.SelectedUseOption(myPawn))
							{
								return;
							}
						}
						TryStartUseJob(myPawn, LocalTargetInfo.Invalid);
					}
				});
			}
		}

		public virtual void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget)
		{
			if (pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly) && CanBeUsedBy(pawn, out string _))
			{
				Job job = extraTarget.IsValid ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent);
				pawn.jobs.TryTakeOrderedJob(job);
			}
		}

		public void UsedBy(Pawn p)
		{
			if (CanBeUsedBy(p, out string _))
			{
				foreach (CompUseEffect item in from x in parent.GetComps<CompUseEffect>()
					orderby x.OrderPriority descending
					select x)
				{
					try
					{
						item.DoEffect(p);
					}
					catch (Exception arg)
					{
						Log.Error("Error in CompUseEffect: " + arg);
					}
				}
			}
		}

		private bool CanBeUsedBy(Pawn p, out string failReason)
		{
			List<ThingComp> allComps = parent.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				CompUseEffect compUseEffect = allComps[i] as CompUseEffect;
				if (compUseEffect != null && !compUseEffect.CanBeUsedBy(p, out failReason))
				{
					return false;
				}
			}
			failReason = null;
			return true;
		}
	}
}
