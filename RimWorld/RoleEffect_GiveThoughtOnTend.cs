using Verse;

namespace RimWorld
{
	public class RoleEffect_GiveThoughtOnTend : RoleEffect
	{
		public ThoughtDef thoughtDef;

		public RoleEffect_GiveThoughtOnTend()
		{
			labelKey = "RoleEffectGiveThoughtOnTended";
		}

		public override string Label(Pawn pawn, Precept_Role role)
		{
			return labelKey.Translate() + ": " + thoughtDef.LabelCap.Formatted(role.LabelCap.ResolveTags());
		}

		public override void Notify_Tended(Pawn doctor, Pawn patient)
		{
			base.Notify_Tended(doctor, patient);
			if (doctor != patient && doctor.Ideo == patient.Ideo)
			{
				patient.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, doctor);
			}
		}
	}
}
