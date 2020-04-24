using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoyalTitlePermitDef : Def
	{
		public Type workerClass = typeof(RoyalTitlePermitWorker);

		public RoyalAid royalAid;

		public float cooldownDays;

		private RoyalTitlePermitWorker worker;

		public int CooldownTicks => (int)(cooldownDays * 60000f);

		public RoyalTitlePermitWorker Worker
		{
			get
			{
				if (worker == null)
				{
					worker = (RoyalTitlePermitWorker)Activator.CreateInstance(workerClass);
					worker.def = this;
				}
				return worker;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (!typeof(RoyalTitlePermitWorker).IsAssignableFrom(workerClass))
			{
				yield return $"RoyalTitlePermitDef {defName} has worker class {workerClass}, which is not deriving from {typeof(RoyalTitlePermitWorker).FullName}";
			}
			if (royalAid != null && royalAid.pawnKindDef != null && royalAid.pawnCount <= 0)
			{
				yield return "pawnCount should be greater than 0, if you specify pawnKindDef";
			}
		}
	}
}
