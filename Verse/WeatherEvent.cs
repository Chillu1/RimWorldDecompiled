using System;
using UnityEngine;

namespace Verse
{
	public abstract class WeatherEvent
	{
		protected Map map;

		public abstract bool Expired
		{
			get;
		}

		public bool CurrentlyAffectsSky => SkyTargetLerpFactor > 0f;

		public virtual SkyTarget SkyTarget
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual float SkyTargetLerpFactor => -1f;

		public virtual Vector2? OverrideShadowVector => null;

		public WeatherEvent(Map map)
		{
			this.map = map;
		}

		public abstract void FireEvent();

		public abstract void WeatherEventTick();

		public virtual void WeatherEventDraw()
		{
		}
	}
}
