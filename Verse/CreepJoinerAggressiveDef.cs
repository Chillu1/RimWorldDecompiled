using System;

namespace Verse;

public class CreepJoinerAggressiveDef : CreepJoinerBaseDef
{
	public bool hasMessage;

	[MustTranslate]
	public string message;

	public bool hasLetter;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterDesc;

	public LetterDef letterDef;

	public Type workerType;
}
