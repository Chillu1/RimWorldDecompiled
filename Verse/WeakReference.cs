using System;

namespace Verse
{
	public class WeakReference<T> : WeakReference where T : class
	{
		public new T Target
		{
			get
			{
				return (T)base.Target;
			}
			set
			{
				base.Target = value;
			}
		}

		public WeakReference(T target)
			: base(target)
		{
		}
	}
}
