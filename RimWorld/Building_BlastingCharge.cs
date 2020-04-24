using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_BlastingCharge : Building
	{
		public override IEnumerable<Gizmo> GetGizmos()
		{
			Command_Action command_Action = new Command_Action();
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate");
			command_Action.defaultDesc = "CommandDetonateDesc".Translate();
			command_Action.action = Command_Detonate;
			if (GetComp<CompExplosive>().wickStarted)
			{
				command_Action.Disable();
			}
			command_Action.defaultLabel = "CommandDetonateLabel".Translate();
			yield return command_Action;
		}

		private void Command_Detonate()
		{
			GetComp<CompExplosive>().StartWick();
		}
	}
}
