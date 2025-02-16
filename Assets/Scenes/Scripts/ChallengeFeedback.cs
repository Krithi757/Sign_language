using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;  // Required for SceneManager
using TMPro;

public class ChallengeFeedback : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI scoreText;
    public Animator characterAnimator;
    public float jumpHeight = 1f; // Adjust jump height as needed
    public float jumpSpeed = 2f;  // Adjust jump speed as needed
    public float coinAnimationSpeed = 0.0001f; // Adjust speed of the coin animation

    private bool hasCelebrated = false;

    void Start()
    {
        int finalCoins = PlayerPrefs.GetInt("Coins", 0);
        int finalScore = PlayerPrefs.GetInt("Score", 0);
        int isCompleted = PlayerPrefs.GetInt("ChallengeIsCompleted", 0);

        // Start the coin and score increment animations
        StartCoroutine(AnimateCoinsAndScore(0, finalCoins, 0, finalScore));

        if (isCompleted == 1)
        {
            StartCoroutine(HandleJumpAndAnimation());
        }
    }

    // Coroutine to handle the coin and score increment animation
    private IEnumerator AnimateCoinsAndScore(int startCoins, int targetCoins, int startScore, int targetScore)
    {
        float elapsedTime = 0f;

        // Continuously update the coins and score text until we reach the target numbers
        while (elapsedTime < 1f)
        {
            int currentCoins = Mathf.FloorToInt(Mathf.Lerp(startCoins, targetCoins, elapsedTime));
            int currentScore = Mathf.FloorToInt(Mathf.Lerp(startScore, targetScore, elapsedTime));

            coinsText.text = currentCoins.ToString();
            scoreText.text = "Score: " + currentScore.ToString();

            elapsedTime += Time.deltaTime / coinAnimationSpeed; // Control the speed of the animation
            yield return null;
        }

        // Ensure we reach the target coin and score count exactly at the end
        coinsText.text = targetCoins.ToString();
        scoreText.text = "Score: " + targetScore.ToString();
    }

    // Coroutine to handle the jump and animation transitions
    private IEnumerator HandleJumpAndAnimation()
    {
        // Check if the celebration has already been done
        if (!hasCelebrated)
        {
            // Start Idle animation or make sure it's idle initially
            characterAnimator.SetBool("isIdle", true);

            // Wait for idle animation to finish (if necessary)
            yield return new WaitForSeconds(1f);  // Adjust delay as needed for idle animation duration

            // Trigger the celebratory pose and start the jump
            characterAnimator.SetBool("isPass", true);

            // Perform Jump (without animation)
            yield return StartCoroutine(Jump());

            // After jump, stay in the celebratory pose for a short time
            yield return new WaitForSeconds(1f);  // Adjust this to allow celebratory pose animation to play

            // After the celebration, transition to the dance cover
            characterAnimator.SetBool("isPass", false);
            characterAnimator.SetBool("isDanceCover", true);  // Transition to dance cover

            // Reduce the speed of the dance cover animation
            characterAnimator.SetFloat("AnimationSpeed", 0.001f);  // Extremely slow speed

            hasCelebrated = true;  // Mark the celebration as done
        }
    }

    // Coroutine to handle the manual jump
    private IEnumerator Jump()
    {
        float jumpDuration = 0.5f;  // Time for the jump arc
        float elapsedTime = 0f;
        Vector3 originalPosition = characterAnimator.transform.position;
        Vector3 jumpTarget = new Vector3(originalPosition.x, originalPosition.y + jumpHeight, originalPosition.z);

        // Move the character up to simulate a jump
        while (elapsedTime < jumpDuration)
        {
            characterAnimator.transform.position = Vector3.Lerp(originalPosition, jumpTarget, (elapsedTime / jumpDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position after the jump
        characterAnimator.transform.position = jumpTarget;

        // Now bring the character down to idle position
        elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            characterAnimator.transform.position = Vector3.Lerp(jumpTarget, originalPosition, (elapsedTime / jumpDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure back to original position (in case of floating point issues)
        characterAnimator.transform.position = originalPosition;
    }

    // Method to be called when the "Retry" button is clicked
    public void OnRetryButtonClicked()
    {
        // Load the current challenge scene (assuming ChallengeTracker.CurrentChallenge stores the current challenge index/scene)
        int currentChallenge = ChallengeTracker.currentChallenge;
        SceneManager.LoadScene(currentChallenge);
    }

    // Method to be called when the "Main Menu" button is clicked
    public void OnMainMenuButtonClicked()
    {
        Debug.Log("Main menu");
        // Load the main menu scene (assuming scene 0 is the main menu)
        SceneManager.LoadScene(0);
    }

    public void OnPlayGameClicked()
    {
        // Load the main menu scene (assuming scene 0 is the main menu)
        SceneManager.LoadScene(4);
    }
}
