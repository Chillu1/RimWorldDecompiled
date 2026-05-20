using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_SanguophageMeeting : LordToil
	{
		private IntVec3 meetingSpot;

		private int meetingDurationTicks;

		private Effecter progressBar;

		private Effecter meetingEffect;

		private Thing torchCached;

		private LordToilData_SanguophageMeeting Data => (LordToilData_SanguophageMeeting)data;

		public LordToil_SanguophageMeeting(IntVec3 meetingSpot, int meetingDurationTicks)
		{
			this.meetingSpot = meetingSpot;
			this.meetingDurationTicks = meetingDurationTicks;
			data = new LordToilData_SanguophageMeeting();
		}

		public override void Init()
		{
			base.Init();
			Messages.Message("SanguophagesBegunMeeting".Translate(), lord.ownedPawns, MessageTypeDefOf.NeutralEvent);
		}

		public override void LordToilTick()
		{
			Data.ticksInMeeting++;
			TargetInfo targetInfo = new TargetInfo(meetingSpot + new IntVec3(0, 0, -2), base.Map);
			if (progressBar == null)
			{
				progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
			}
			progressBar.EffectTick(targetInfo, TargetInfo.Invalid);
			MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
			if (mote != null)
			{
				mote.progress = Mathf.Clamp01((float)Data.ticksInMeeting / (float)meetingDurationTicks);
			}
			if (torchCached == null)
			{
				torchCached = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(targetInfo.Cell, base.Map, ThingRequest.ForDef(ThingDefOf.SanguphageMeetingTorch), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), 3f);
			}
			TargetInfo a = ((torchCached != null) ? new TargetInfo(torchCached.Position, base.Map) : targetInfo);
			if (meetingEffect == null)
			{
				meetingEffect = EffecterDefOf.SanguophageMeeting.Spawn(a.Cell, base.Map);
			}
			meetingEffect.EffectTick(a, TargetInfo.Invalid);
		}

		public override void Cleanup()
		{
			progressBar?.Cleanup();
			meetingEffect?.Cleanup();
			progressBar = null;
			meetingEffect = null;
		}

		public override void UpdateAllDuties()
		{
			if (ModsConfig.BiotechActive)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					PawnDuty pawnDuty = new PawnDuty(DutyDefOf.SocialMeeting, meetingSpot);
					pawnDuty.spectateRect = CellRect.SingleCell(meetingSpot);
					pawnDuty.spectateDistance = new IntRange(1, 2);
					pawnDuty.locomotion = LocomotionUrgency.Walk;
					lord.ownedPawns[i].mindState.duty = pawnDuty;
				}
			}
		}
	}
}
