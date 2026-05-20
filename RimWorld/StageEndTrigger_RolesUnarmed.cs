using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class StageEndTrigger_RolesUnarmed : StageEndTrigger
	{
		[NoTranslate]
		public List<string> roleIds;

		public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
		{
			return new Trigger_Custom(delegate
			{
				foreach (string roleId in roleIds)
				{
					foreach (ThingWithComps item in ritual.PawnWithRole(roleId).equipment.AllEquipmentListForReading)
					{
						if (item.def.IsWeapon)
						{
							return false;
						}
					}
				}
				return true;
			});
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref roleIds, "roleIds", LookMode.Value);
		}
	}
}
