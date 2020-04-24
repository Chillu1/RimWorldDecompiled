using RimWorld;
using System.Linq;

namespace Verse
{
	public class HediffComp_Link : HediffComp
	{
		public Pawn other;

		private MoteDualAttached mote;

		public bool drawConnection;

		public HediffCompProperties_Link Props => (HediffCompProperties_Link)props;

		public override bool CompShouldRemove
		{
			get
			{
				if (base.CompShouldRemove)
				{
					return true;
				}
				if (other == null || !parent.pawn.Spawned || !other.Spawned)
				{
					return true;
				}
				if (Props.maxDistance > 0f && !parent.pawn.Position.InHorDistOf(other.Position, Props.maxDistance))
				{
					return true;
				}
				foreach (Hediff hediff in other.health.hediffSet.hediffs)
				{
					HediffWithComps hediffWithComps = hediff as HediffWithComps;
					if (hediffWithComps != null && hediffWithComps.comps.FirstOrDefault(delegate(HediffComp c)
					{
						HediffComp_Link hediffComp_Link = c as HediffComp_Link;
						return hediffComp_Link != null && hediffComp_Link.other == parent.pawn && hediffComp_Link.parent.def == parent.def;
					}) != null)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (!Props.showName || other == null)
				{
					return null;
				}
				return other.LabelShort;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			if (drawConnection)
			{
				if (mote == null)
				{
					mote = MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_PsychicLinkLine, parent.pawn, other);
				}
				mote.Maintain();
			}
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_References.Look(ref other, "other");
		}
	}
}
