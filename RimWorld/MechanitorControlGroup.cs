using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class MechanitorControlGroup : IExposable
	{
		public static readonly FloatRange DefaultMechRechargeThresholds = new FloatRange(0.05f, 1f);

		private List<AssignedMech> assignedMechs = new List<AssignedMech>();

		private Pawn_MechanitorTracker tracker;

		private MechWorkModeDef workMode;

		private GlobalTargetInfo target = GlobalTargetInfo.Invalid;

		private Dictionary<Pawn, string> tags;

		public FloatRange mechRechargeThresholds = DefaultMechRechargeThresholds;

		private List<Pawn> tmpTagPawns;

		private List<string> tmpTags;

		private List<Pawn> mechsForReading;

		public List<Pawn> MechsForReading
		{
			get
			{
				if (mechsForReading == null)
				{
					mechsForReading = new List<Pawn>();
				}
				mechsForReading.Clear();
				for (int i = 0; i < assignedMechs.Count; i++)
				{
					if (!assignedMechs[i].pawn.IsGestating())
					{
						mechsForReading.Add(assignedMechs[i].pawn);
					}
				}
				return mechsForReading;
			}
		}

		public List<AssignedMech> AssignedMechs => assignedMechs;

		public Pawn_MechanitorTracker Tracker => tracker;

		public MechWorkModeDef WorkMode => workMode;

		public GlobalTargetInfo Target => target;

		public int Index => Tracker.controlGroups.IndexOf(this) + 1;

		public string LabelIndexWithWorkMode => Index + " (" + WorkMode.LabelCap.ToString() + ")";

		public MechanitorControlGroup(Pawn_MechanitorTracker tracker)
		{
			this.tracker = tracker;
			workMode = MechWorkModeDefOf.Work;
		}

		public bool TryUnassign(Pawn pawn)
		{
			return assignedMechs.RemoveAll((AssignedMech x) => x.pawn == pawn) > 0;
		}

		public void Assign(Pawn pawn)
		{
			foreach (MechanitorControlGroup controlGroup in tracker.controlGroups)
			{
				controlGroup.TryUnassign(pawn);
			}
			assignedMechs.Add(new AssignedMech(pawn));
			SetWorkModeForPawn(pawn, workMode);
		}

		public void SetWorkMode(MechWorkModeDef workMode)
		{
			SetWorkMode(workMode, GlobalTargetInfo.Invalid);
		}

		public void SetWorkMode(MechWorkModeDef workMode, GlobalTargetInfo target)
		{
			if (this.workMode == workMode && this.target == target)
			{
				return;
			}
			this.workMode = workMode;
			this.target = target;
			foreach (AssignedMech assignedMech in assignedMechs)
			{
				SetWorkModeForPawn(assignedMech.pawn, workMode);
			}
			tags?.Clear();
			if (workMode != MechWorkModeDefOf.Escort)
			{
				return;
			}
			Pawn pawn = Tracker.Pawn;
			if (!pawn.IsFormingCaravan())
			{
				return;
			}
			Lord formAndSendCaravanLord = CaravanFormingUtility.GetFormAndSendCaravanLord(pawn);
			List<Pawn> list = Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false);
			for (int i = 0; i < assignedMechs.Count; i++)
			{
				Pawn pawn2 = assignedMechs[i].pawn;
				if (list.Contains(pawn2) && CaravanFormingUtility.GetFormAndSendCaravanLord(pawn2) != formAndSendCaravanLord)
				{
					Messages.Message("MessageCaravanAddingEscortingMech".Translate(pawn2.Named("MECH"), pawn.Named("OVERSEER")), pawn2, MessageTypeDefOf.RejectInput, historical: false);
					CaravanFormingUtility.LateJoinFormingCaravan(pawn2, formAndSendCaravanLord);
				}
			}
		}

		private void SetWorkModeForPawn(Pawn pawn, MechWorkModeDef workMode)
		{
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
			if (workMode != MechWorkModeDefOf.Recharge && pawn.CurJobDef == JobDefOf.MechCharge && pawn.IsCharging())
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			pawn.TryGetComp<CompCanBeDormant>()?.WakeUp();
			pawn.jobs?.CheckForJobOverride();
		}

		public void SetTag(Pawn pawn, string tag)
		{
			if (tags == null)
			{
				tags = new Dictionary<Pawn, string>();
			}
			tags.Add(pawn, tag);
		}

		public string GetTag(Pawn pawn)
		{
			if (tags == null || !tags.TryGetValue(pawn, out var value))
			{
				return null;
			}
			return value;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref assignedMechs, "assignedMechs", LookMode.Deep);
			Scribe_Defs.Look(ref workMode, "workMode");
			Scribe_TargetInfo.Look(ref target, "target");
			Scribe_Collections.Look(ref tags, "tags", LookMode.Reference, LookMode.Value, ref tmpTagPawns, ref tmpTags);
			Scribe_Values.Look(ref mechRechargeThresholds, "mechRechargeThresholds", DefaultMechRechargeThresholds);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				assignedMechs.RemoveAll((AssignedMech x) => x.pawn == null);
			}
		}
	}
}
