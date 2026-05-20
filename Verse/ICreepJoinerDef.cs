using System.Collections.Generic;

namespace Verse;

public interface ICreepJoinerDef
{
	float Weight { get; }

	float MinCombatPoints { get; }

	bool CanOccurRandomly { get; }

	List<CreepJoinerBaseDef> Excludes { get; }

	List<CreepJoinerBaseDef> Requires { get; }
}
