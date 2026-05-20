using System;

namespace Verse;

public class CreepJoinerRejectionDef : CreepJoinerBaseDef
{
	public bool hasLetter;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterDesc;

	public LetterDef letterDef;

	public Type workerType;
}
