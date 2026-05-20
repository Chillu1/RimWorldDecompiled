using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SignalAction_OpenCasket : SignalAction_Delay
	{
		public List<Thing> caskets = new List<Thing>();

		private Alert_ActionDelay cachedAlert;

		public override Alert_ActionDelay Alert
		{
			get
			{
				if (cachedAlert == null)
				{
					cachedAlert = new Alert_CasketOpening(this);
				}
				return cachedAlert;
			}
		}

		public override bool ShouldRemoveNow
		{
			get
			{
				for (int i = 0; i < caskets.Count; i++)
				{
					if (caskets[i] != null && !caskets[i].Destroyed && ((Building_Casket)caskets[i]).HasAnyContents)
					{
						return false;
					}
				}
				return true;
			}
		}

		protected override void Complete()
		{
			base.Complete();
			for (int i = 0; i < caskets.Count; i++)
			{
				Building_Casket building_Casket = (Building_Casket)caskets[i];
				if (building_Casket.CanOpen)
				{
					building_Casket.Open();
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref caskets, "caskets", LookMode.Reference);
		}
	}
}
