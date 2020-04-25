using UnityEngine;

public static class Util
{
	public static bool ChanceOf(float rate)
	{
		if (rate > 1f || rate < 0f) throw new System.ArgumentOutOfRangeException(nameof(rate), rate, "The argument is greater than 1 or less than 0");

		float random = Random.value;

		return rate >= random;
	}

	public static Vector3 DirFromTo(Vector3 from, Vector3 to)
	{
		return (from - to).normalized;
	}

	public static Vector3 Lerp(Vector3 from, Vector3 to, float time, ref float curTime)
	{
		if (curTime < 0)
			throw new System.ArgumentOutOfRangeException(nameof(curTime), curTime, "curTime must be equals or greater than 0.");
		else if (curTime == 0)
		{
			curTime += Time.deltaTime;
			return from;
		}
		else if (curTime >= time)
			return to;

		curTime += Time.deltaTime;

		return Vector3.Lerp(from, to, curTime / time);
	}

	public static Vector3 Slerp(Vector3 from, Vector3 to, float time, ref float curTime)
	{
		if (curTime < 0 || curTime == 0)
		{
			curTime = 0;
			curTime += Time.deltaTime;
			return from;
		}
		else if (curTime >= time)
			return to;

		curTime += Time.deltaTime;

		return Vector3.Slerp(from, to, curTime / time);
	}

	public static float Lerp(float from, float to, float time, ref float curTime)
	{
		if (curTime < 0)
			throw new System.ArgumentOutOfRangeException("curTime", curTime, "curTime must be equals or greater than 0.");
		else if (curTime == 0)
		{
			curTime += Time.deltaTime;
			return from;
		}
		else if (curTime >= time)
			return to;

		float trajectory = curTime / time;

		float result = Mathf.Lerp(from, to, trajectory);

		curTime += Time.deltaTime;

		return result;
	}

	public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default)
	{
		if (_color != default)
			UnityEditor.Handles.color = _color;
		Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale);
		using (new UnityEditor.Handles.DrawingScope(angleMatrix))
		{
			var pointOffset = (_height - (_radius * 2)) / 2;

			//draw sideways
			UnityEditor.Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
			UnityEditor.Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
			UnityEditor.Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
			UnityEditor.Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
			//draw frontways
			UnityEditor.Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
			UnityEditor.Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
			UnityEditor.Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
			UnityEditor.Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
			//draw center
			UnityEditor.Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
			UnityEditor.Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);
		}
	}

}
