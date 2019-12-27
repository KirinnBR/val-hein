using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static bool ChanceOf(float rate)
	{
		if (rate > 100 || rate < 0) throw new System.ArgumentOutOfRangeException("rate", rate, "The argument is greater than 100 or less than 0");

		float random = Random.Range(0f, 100f);

		return rate >= random;
	}
}
