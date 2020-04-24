using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class StaticConstructorOnStartup : Attribute
	{
	}
}
