using UnityEngine;

public class Score : MonoBehaviour {
	private static int score;
	private static Score instance;
	private static bool victory;

	void Awake () {
		instance = this;
		score = 0;
		victory = false;
	}

	void Update () {
		if (!victory) {
			GetComponent<TextMesh>().text = score.ToString();
		} else {
			GetComponent<TextMesh>().text = "VICTORY! - " + score.ToString();
		}

		if (transform.localScale.x > 1) {
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), 5 * Time.deltaTime);
		}
	}

	public static void Raise (int amount) {
		score += amount;
		instance.transform.localScale = new Vector3(1.3f, 1.3f);
	}

	public static void Clear () {
		score = 0;
	}

	public static void Victory () {
		victory = true;
	}
}
