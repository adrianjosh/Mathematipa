using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class onCollisionEnter : MonoBehaviour
{
	public int gameOver = 0;
	void OnCollisionEnter2D(Collision2D exampleCol)
	{
		if (exampleCol.collider.tag == "Wall")
		{
			//Debug.Log("Game Over");
			//SceneManager.LoadScene(0);
			GameOverMenu.GameIsOver = true;
            WordManager.i.CleanChildren();
			AudioManager.i.PlaySfx(AudioId.EndlessGameOver);

        }
	}
}
