using System;

namespace Verse
{
	public static class ThingRequestGroupUtility
	{
		public static bool StoreInRegion(this ThingRequestGroup group)
		{
			return group switch
			{
				ThingRequestGroup.Undefined => false, 
				ThingRequestGroup.Nothing => false, 
				ThingRequestGroup.Everything => true, 
				ThingRequestGroup.HaulableEver => true, 
				ThingRequestGroup.HaulableAlways => true, 
				ThingRequestGroup.FoodSource => true, 
				ThingRequestGroup.FoodSourceNotPlantOrTree => true, 
				ThingRequestGroup.Corpse => true, 
				ThingRequestGroup.Blueprint => true, 
				ThingRequestGroup.BuildingArtificial => true, 
				ThingRequestGroup.BuildingFrame => true, 
				ThingRequestGroup.Pawn => true, 
				ThingRequestGroup.PotentialBillGiver => true, 
				ThingRequestGroup.Medicine => true, 
				ThingRequestGroup.Filth => true, 
				ThingRequestGroup.AttackTarget => true, 
				ThingRequestGroup.Weapon => true, 
				ThingRequestGroup.Refuelable => true, 
				ThingRequestGroup.HaulableEverOrMinifiable => true, 
				ThingRequestGroup.Drug => true, 
				ThingRequestGroup.Shell => true, 
				ThingRequestGroup.HarvestablePlant => true, 
				ThingRequestGroup.Fire => true, 
				ThingRequestGroup.Plant => true, 
				ThingRequestGroup.Bed => true, 
				ThingRequestGroup.Chunk => true, 
				ThingRequestGroup.Construction => false, 
				ThingRequestGroup.HasGUIOverlay => false, 
				ThingRequestGroup.Apparel => false, 
				ThingRequestGroup.MinifiedThing => false, 
				ThingRequestGroup.Grave => false, 
				ThingRequestGroup.Art => false, 
				ThingRequestGroup.ThingHolder => false, 
				ThingRequestGroup.ActiveDropPod => false, 
				ThingRequestGroup.Transporter => false, 
				ThingRequestGroup.LongRangeMineralScanner => false, 
				ThingRequestGroup.AffectsSky => false, 
				ThingRequestGroup.WindSource => false, 
				ThingRequestGroup.AlwaysFlee => false, 
				ThingRequestGroup.ResearchBench => false, 
				ThingRequestGroup.Facility => false, 
				ThingRequestGroup.AffectedByFacilities => false, 
				ThingRequestGroup.CreatesInfestations => false, 
				ThingRequestGroup.WithCustomRectForSelector => false, 
				ThingRequestGroup.ProjectileInterceptor => false, 
				ThingRequestGroup.ConditionCauser => false, 
				ThingRequestGroup.MusicalInstrument => false, 
				ThingRequestGroup.Throne => false, 
				ThingRequestGroup.FoodDispenser => false, 
				ThingRequestGroup.Projectile => false, 
				ThingRequestGroup.MeditationFocus => false, 
				_ => throw new ArgumentException("group"), 
			};
		}
	}
}
