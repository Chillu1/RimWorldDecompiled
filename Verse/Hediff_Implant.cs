namespace Verse
{
	public class Hediff_Implant : HediffWithComps
	{
		public override bool ShouldRemove => false;

		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			if (base.Part == null)
			{
				Log.Error(def.defName + " has null Part. It should be set before PostAdd.");
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
			{
				Log.Error(GetType().Name + " has null part after loading.");
				pawn.health.hediffSet.hediffs.Remove(this);
			}
		}
	}
}
