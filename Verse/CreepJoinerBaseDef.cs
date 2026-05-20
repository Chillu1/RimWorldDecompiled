using System.Collections.Generic;

namespace Verse;

public class CreepJoinerBaseDef : Def, ICreepJoinerDef
{
	private float weight = 1f;

	private float minCombatPoints;

	private bool canOccurRandomly = true;

	private List<CreepJoinerBaseDef> excludes = new List<CreepJoinerBaseDef>();

	private List<CreepJoinerBaseDef> requires = new List<CreepJoinerBaseDef>();

	[MustTranslate]
	public string surgicalInspectionLetterExtra;

	public float Weight => weight;

	public List<CreepJoinerBaseDef> Excludes => excludes;

	public List<CreepJoinerBaseDef> Requires => requires;

	public float MinCombatPoints => minCombatPoints;

	public bool CanOccurRandomly => canOccurRandomly;
}
