using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			Pawn myPawn2 = myPawn;
			CompUsable compUsable = this;
			if (!CanBeUsedBy(myPawn2, out string failReason))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn2) + ((failReason != null) ? (" (" + failReason + ")") : ""), null);
				yield break;
			}
			if (!myPawn2.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn2) + " (" + "NoPath".Translate() + ")", null);
				yield break;
			}
			if (!myPawn2.CanReserve(parent))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn2) + " (" + "Reserved".Translate() + ")", null);
				yield break;
			}
			if (!myPawn2.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn2) + " (" + "Incapable".Translate() + ")", null);
				yield break;
			}
			yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn2), delegate
			{
				if (myPawn2.CanReserveAndReach(compUsable.parent, PathEndMode.Touch, Danger.Deadly))
				{
					foreach (CompUseEffect comp in compUsable.parent.GetComps<CompUseEffect>())
					{
						if (comp.SelectedUseOption(myPawn2))
						{
							return;
						}
					}
					compUsable.TryStartUseJob(myPawn2, LocalTargetInfo.Invalid);
				}
			});
		}

		public virtual void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget)
		{
			if (!pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly) || !CanBeUsedBy(pawn, out string _))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
			{
				TaggedString taggedString = comp.ConfirmMessage(pawn);
				if (!taggedString.NullOrEmpty())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendTagged(taggedString);
				}
			}
			string str = stringBuilder.ToString();
			if (str.NullOrEmpty())
			{
				StartJob();
			}
			else
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(str, StartJob));
			}
			void StartJob()
			{
				Job job = extraTarget.IsValid ? JobMaker.MakeJob(Props.useJob, parent, extraTarget) : JobMaker.MakeJob(Props.useJob, parent);
				pawn.jobs.TryTakeOrderedJob(job);
			}
		}

		public void UsedBy(Pawn p)
		{
			if (!CanBeUsedBy(p, out string _))
			{
				return;
			}
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
