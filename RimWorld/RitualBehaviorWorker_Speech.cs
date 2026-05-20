using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RitualBehaviorWorker_Speech : RitualBehaviorWorker
	{
		public RitualBehaviorWorker_Speech()
		{
		}

		public RitualBehaviorWorker_Speech(RitualBehaviorDef def)
			: base(def)
		{
		}

		protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
		{
			Pawn organizer2 = assignments.AssignedPawns("speaker").First();
			return new LordJob_Joinable_Speech(target, organizer2, ritual, def.stages, assignments, titleSpeech: false);
		}

		protected override void PostExecute(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
		{
			Pawn arg = assignments.AssignedPawns("speaker").First();
			Find.LetterStack.ReceiveLetter(def.letterTitle.Formatted(ritual.Named("RITUAL")), def.letterText.Formatted(arg.Named("SPEAKER"), ritual.Named("RITUAL"), ritual.ideo.MemberNamePlural.Named("IDEOMEMBERS")) + "\n\n" + ritual.outcomeEffect.ExtraAlertParagraph(ritual), LetterDefOf.PositiveEvent, target);
		}

		public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
		{
			Precept_Role precept_Role = ritual.ideo.RolesListForReading.FirstOrDefault((Precept_Role r) => r.def == PreceptDefOf.IdeoRole_Leader);
			if (precept_Role == null)
			{
				return null;
			}
			if (precept_Role.ChosenPawnSingle() == null)
			{
				return "CantStartRitualRoleNotAssigned".Translate(precept_Role.LabelCap);
			}
			return base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
		}
	}
}
