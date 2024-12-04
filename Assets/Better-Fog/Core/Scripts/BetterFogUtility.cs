using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.BetterFog.Core
{
	public enum HeightFogType
	{
		Exponential = 1,
		ExponentialSquared = 2
	}

	public enum NoiseAffect
	{
		HeightOnly,
		DistanceOnly,
		Both
	}

	public class BetterFogUtility
    {
		public static GUIStyle centeredBoldLabel = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Bold,
			fontSize = 12,
		};
	}

	
}