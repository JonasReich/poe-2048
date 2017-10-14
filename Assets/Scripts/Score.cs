//-------------------------
// (c) 2017, Jonas Reich
//-------------------------

using UnityEngine;

public class Score : MonoBehaviour {
	static int _score;
	static Score _instance;
	static bool _victory;

	void Awake ()
	{
		_instance = this;
		_score = 0;
		_victory = false;
	}

	void Update ()
	{
		if (!_victory)
			GetComponent<TextMesh>().text = _score.ToString();
		else
			GetComponent<TextMesh>().text = "VICTORY! - " + _score;

		if (transform.localScale.x > 1)
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), 5 * Time.deltaTime);
	}

	public static void Raise (int amount)
	{
		_score += amount;
		_instance.transform.localScale = new Vector3(1.3f, 1.3f);
	}

	public static void Clear ()
	{
		_score = 0;
	}

	public static void Victory ()
	{
		_victory = true;
	}
}
