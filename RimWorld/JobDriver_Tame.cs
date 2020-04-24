using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Tame : JobDriver_InteractAnimal
	{
		protected override bool CanInteractNow => !TameUtility.TriedToTameTooRecently(base.Animal);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
			this.FailOn(() => base.Map.designationManager.DesignationOn(base.Animal, DesignationDefOf.Tame) == null && !base.OnLastToil);
		}

		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryRecruit(TargetIndex.A);
		}
	}
}
