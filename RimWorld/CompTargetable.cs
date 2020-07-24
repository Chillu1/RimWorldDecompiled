using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompTargetable : CompUseEffect
	{
		private Thing target;

		public CompProperties_Targetable Props => (CompProperties_Targetable)props;

		protected abstract bool PlayerChoosesTarget
		{
			get;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref target, "target");
		}

		public override bool SelectedUseOption(Pawn p)
		{
			if (PlayerChoosesTarget)
			{
				Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate(LocalTargetInfo t)
				{
					target = t.Thing;
					parent.GetComp<CompUsable>().TryStartUseJob(p, target);
				}, p);
				return true;
			}
			target = null;
			return false;
		}

		public override void DoEffect(Pawn usedBy)
		{
			if ((PlayerChoosesTarget && target == null) || (target != null && !GetTargetingParameters().CanTarget(target)))
			{
				return;
			}
			base.DoEffect(usedBy);
			foreach (Thing target2 in GetTargets(target))
			{
				foreach (CompTargetEffect comp in parent.GetComps<CompTargetEffect>())
				{
					comp.DoEffectOn(usedBy, target2);
				}
				if (Props.moteOnTarget != null)
				{
					MoteMaker.MakeAttachedOverlay(target2, Props.moteOnTarget, Vector3.zero);
				}
				if (Props.moteConnecting != null)
				{
					MoteMaker.MakeConnectingLine(usedBy.DrawPos, target2.DrawPos, Props.moteConnecting, usedBy.Map);
				}
			}
			target = null;
		}

		protected abstract TargetingParameters GetTargetingParameters();

		public abstract IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null);

		public bool BaseTargetValidator(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (Props.psychicSensitiveTargetsOnly && pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
				{
					return false;
				}
				if (Props.ignoreQuestLodgerPawns && pawn.IsQuestLodger())
				{
					return false;
				}
				if (Props.ignorePlayerFactionPawns && pawn.Faction == Faction.OfPlayer)
				{
					return false;
				}
			}
			if (Props.fleshCorpsesOnly)
			{
				Corpse corpse = t as Corpse;
				if (corpse != null && !corpse.InnerPawn.RaceProps.IsFlesh)
				{
					return false;
				}
			}
			if (Props.nonDessicatedCorpsesOnly)
			{
				Corpse corpse2 = t as Corpse;
				if (corpse2 != null && corpse2.GetRotStage() == RotStage.Dessicated)
				{
					return false;
				}
			}
			return true;
		}
	}
}
