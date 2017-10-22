//-------------------------
// (c) 2017, Jonas Reich
//-------------------------

using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {
	static int _score;
	static Score _instance;
	static bool _victory;

	Text _textComponent;

	void Awake ()
	{
		_instance = this;
		_score = 0;
		_victory = false;

		_textComponent = GetComponent<Text>();
	}

	void Update ()
	{
		if (!_victory)
			_textComponent.text = _score.ToString();
		else
			_textComponent.text = "VICTORY! - " + _score;

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
