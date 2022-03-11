using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MyLevelComplete : MonoBehaviour
{
    bool ballIn = false;
    bool shouldPlaySound = true;
    [SerializeField]
    Collider ballCollider;
    [SerializeField]
    Rigidbody ballRB;
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    AudioSource successAudio;

    int level = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ballIn) {
		if (Mathf.Abs(ballRB.velocity.y) < 0.1f)
		{
		    if (Mathf.Abs(ballRB.velocity.y) < 0.001f)
		    {
			  text.text = "Next Level";
			  if (level == 1)
			  {
				SceneManager.LoadScene("Level2");
				level = 2;
			  }
			  else
			  {
				SceneManager.LoadScene("SampleScene");
				level = 1;
			  }
		    }
		    else
		    {
		        text.text = "YOU DID IT!!";
			  if (shouldPlaySound)
			  {
				successAudio.Play();
				shouldPlaySound = false;
			  }
		    }
		}
	  }
    }

    //Upon collision with another GameObject, see if it's the ball. 
    private void OnTriggerEnter(Collider other)
    {
	  if (other == ballCollider)
	  {
		text.text = "Verifying. . .";
		ballIn = true;
	  }
    }

    private void OnTriggerExit(Collider other)
    {
	  if (other == ballCollider)
	  {
		text.text = "Get it in the Bucket!";
		ballIn = false;
		shouldPlaySound = true;
	  }
    }
}
