using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 


public class GameManager : MonoBehaviour
{
    public AudioSource backgroundMusic; 
    public AudioSource powerUpMusic;    


    public static GameManager Instance;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    [HideInInspector] public bool gameStarted = false;
    [HideInInspector] public bool gameEnded = false;

    [Header("Power-up")]
    public Slider powerUpSlider;
    public int powerUpPointsThreshold = 100;
    private float currentPowerUpPoints = 0f; 
    private bool canUsePowerUp = false;
    private bool isPowerUpActive = false;
    public float powerUpDuration = 8f;
    private float powerUpTimer = 0f;

    [Header("Speed Setting")]
    public float startingSpeed = 0.2f;
    public float speedIncreasePerSecond = 0.01f;
    public float currentScoreIncreaseSpeedMultiplier = 5f;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public GameObject gameEndedScreen;

    [Header("Obstacle Spawn")]
    public float minTimeDelayBetweenSpawn = 0.8f;
    public float maxTimeDelayBetweenSpawn = 2f;
    public float obstacleSpeedMultiplier = 25f;

    [Space]
    public GameObject[] allGroundObstacles;
    public GameObject[] allFlyingObstacles;

    [Space]
    public Transform groundObstacleSpawnPoint;
    public Transform flyingObstacleSpawnPoint;

    private List<GameObject> allCurrentObstacles = new List<GameObject>();
    private int highScore = 0;
    private float currentScore = 0;
    private float currentSpeed;
    private float timeUntilNextObstacle;

    private void Awake()
    {
        Instance = this;
        currentSpeed = startingSpeed;

        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScore = PlayerPrefs.GetInt("HighScore");
        }

        UpdateScoreUI();

    }

    private void Start()
    {
        timeUntilNextObstacle = UnityEngine.Random.Range(minTimeDelayBetweenSpawn, maxTimeDelayBetweenSpawn);
        powerUpSlider.maxValue = powerUpPointsThreshold;
        powerUpSlider.value = 0;
        backgroundMusic.Play();
    }
    public void ShowGameEndScreen()
    {
        gameEndedScreen.SetActive(true);
    }
    private void Update()
    {
        if (gameStarted && !gameEnded)
        {
            HandleObstacleSpawning();
            MoveObstacles();
            UpdateSpeedAndScore();
            UpdateScoreUI();
            UpdatePowerUp();
        }
    }

    private void HandleObstacleSpawning()
    {
        timeUntilNextObstacle -= Time.deltaTime *currentSpeed;

        if (timeUntilNextObstacle <= 0)
        {
            timeUntilNextObstacle = UnityEngine.Random.Range(minTimeDelayBetweenSpawn, maxTimeDelayBetweenSpawn); 

            if (currentScore >= 50 && UnityEngine.Random.value > 0.8f) 
            {
                SpawnObstacle(allFlyingObstacles, flyingObstacleSpawnPoint);
            }
            else
            {
                SpawnObstacle(allGroundObstacles, groundObstacleSpawnPoint);
            }
        }
    }

    private void SpawnObstacle(GameObject[] obstacleArray, Transform spawnPoint)
    {
        if (obstacleArray.Length > 0)
        {
            GameObject newObstacle = Instantiate(obstacleArray[UnityEngine.Random.Range(0, obstacleArray.Length)], spawnPoint.position, Quaternion.identity);
            allCurrentObstacles.Add(newObstacle);
        }
    }

    private void MoveObstacles()
    {
        for (int i = allCurrentObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = allCurrentObstacles[i];

            if (obstacle != null)
            {
                obstacle.transform.Translate(new Vector3(-currentSpeed * Time.deltaTime * obstacleSpeedMultiplier, 0, 0));

                if (obstacle.transform.position.x < -10f) 
                {
                    Destroy(obstacle);
                    allCurrentObstacles.RemoveAt(i);
                }
            }
        }
    }

    private void UpdateSpeedAndScore()
    {
        // Increase speed and score over time
        currentSpeed += Time.deltaTime * speedIncreasePerSecond;
        floorMeshRenderer.material.mainTextureOffset += new Vector2(currentSpeed * Time.deltaTime, 0);
        currentScore += currentSpeed * Time.deltaTime * currentScoreIncreaseSpeedMultiplier;


        if (Mathf.RoundToInt(currentScore) >= highScore)
        {
            highScore = Mathf.RoundToInt(currentScore);
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    private void UpdatePowerUp()
    {
        if (!isPowerUpActive && gameStarted)
        {
            currentPowerUpPoints += currentSpeed * Time.deltaTime * currentScoreIncreaseSpeedMultiplier; 
            currentPowerUpPoints = Mathf.Min(currentPowerUpPoints, powerUpPointsThreshold);

            powerUpSlider.value = currentPowerUpPoints;

            if (currentPowerUpPoints >= powerUpPointsThreshold)
            {
                canUsePowerUp = true;
            }

            Debug.Log("Current power-up points: " + currentPowerUpPoints + ", Slider value: " + powerUpSlider.value);
        }

        if (canUsePowerUp && Input.GetKeyDown(KeyCode.LeftShift))
        {
            ActivatePowerUp();
        }

        if (isPowerUpActive)
        {
            powerUpTimer += Time.deltaTime;
            if (powerUpTimer >= powerUpDuration)
            {
                DeactivatePowerUp();
            }
        }
    }


    private void ActivatePowerUp()
    {
        if (canUsePowerUp)
        {
            isPowerUpActive = true;
            powerUpTimer = 0;
            canUsePowerUp = false;
            currentPowerUpPoints = 0;
            powerUpSlider.value = 0;
            Debug.Log("Power-Up Activated!");
            backgroundMusic.Stop();
            powerUpMusic.Play();

            DinoMovement.Instance.SetInvincible(true);
        }
    }


  private void DeactivatePowerUp()
    {
        isPowerUpActive = false;
        DinoMovement.Instance.SetInvincible(false);
        Debug.Log("Power-Up Deactivated!");
        powerUpMusic.Stop();
        backgroundMusic.Play();

    }







    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void UpdateScoreUI()
    {
        scoreText.SetText($"HI {highScore.ToString("D5")}   {Mathf.RoundToInt(currentScore).ToString("D5")}");
    }
}
