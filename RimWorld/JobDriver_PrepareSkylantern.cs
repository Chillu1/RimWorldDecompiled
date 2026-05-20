using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PrepareSkylantern : JobDriver_GotoAndStandSociallyActive
	{
		public override Toil StandToil
		{
			get
			{
				if (!ModLister.CheckIdeology("Skylantern job"))
				{
					return null;
				}
				Toil toil = base.StandToil.WithEffect(EffecterDefOf.MakingSkylantern, () => pawn.Position + pawn.Rotation.FacingCell);
				toil.AddPreInitAction(delegate
				{
					Thing thing = pawn.inventory.innerContainer.FirstOrDefault((Thing t) => t.def == job.thingDefToCarry && t.stackCount >= job.count);
					if (thing != null)
					{
						pawn.carryTracker.TryStartCarry(thing, job.count);
					}
				});
				return toil;
			}
		}
	}
}
