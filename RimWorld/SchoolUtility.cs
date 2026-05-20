using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class SchoolUtility
	{
		public const TargetIndex DeskIndex = TargetIndex.A;

		public const TargetIndex TeacherIndex = TargetIndex.B;

		public const TargetIndex StudentIndex = TargetIndex.B;

		private const int DeskStudentInteractionIndex = 0;

		private const int DeskTeacherInteractionIndex = 1;

		public static Pawn FindTeacher(Pawn child)
		{
			foreach (Pawn item in ChildcareUtility.SpawnedColonistChildcarers(child.Map))
			{
				if (CanTeachNow(item) && item.CanReach(child, PathEndMode.Touch, Danger.Deadly))
				{
					return item;
				}
			}
			return null;
		}

		public static bool CanTeachNow(Pawn teacher)
		{
			if (teacher.DevelopmentalStage.Juvenile() || teacher.Downed || teacher.Drafted || teacher.WorkTypeIsDisabled(WorkTypeDefOf.Childcare) || !teacher.workSettings.WorkIsActive(WorkTypeDefOf.Childcare) || !teacher.health.capacities.CapableOf(PawnCapacityDefOf.Talking) || !teacher.Awake() || teacher.IsBurning() || PawnUtility.WillSoonHaveBasicNeed(teacher, 0.1f) || teacher.InMentalState || teacher.GetLord() != null)
			{
				return false;
			}
			return true;
		}

		public static bool NeedsTeacher(Pawn student)
		{
			if (student.CurJobDef != JobDefOf.Lessontaking)
			{
				return false;
			}
			Pawn pawn = student.CurJob.GetTarget(TargetIndex.B).Pawn;
			if (pawn != null && pawn.CurJobDef == JobDefOf.Lessongiving && pawn.CurJob.GetTarget(TargetIndex.B) == student)
			{
				return false;
			}
			return true;
		}

		public static IntVec3 DeskSpotTeacher(Thing desk)
		{
			if (desk.def != ThingDefOf.SchoolDesk)
			{
				return IntVec3.Invalid;
			}
			return desk.InteractionCells[1];
		}

		public static IntVec3 DeskSpotStudent(Thing desk)
		{
			if (desk.def != ThingDefOf.SchoolDesk)
			{
				return IntVec3.Invalid;
			}
			return desk.InteractionCells[0];
		}

		public static Thing ClosestSchoolDesk(Pawn child, Pawn teacher)
		{
			if (teacher == null)
			{
				return null;
			}
			return GenClosest.ClosestThingReachable(child.Position, child.Map, ThingRequest.ForDef(ThingDefOf.SchoolDesk), PathEndMode.InteractionCell, TraverseParms.For(child), 9999f, (Thing d) => child.CanReserveSittableOrSpot(DeskSpotStudent(d)) && teacher.CanReserveSittableOrSpot(DeskSpotTeacher(d)) && !d.IsForbidden(child) && !d.IsForbidden(teacher));
		}
	}
}
