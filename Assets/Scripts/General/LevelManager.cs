using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    #region Variables
    public static LevelManager Instance;

    [SerializeField] private Text coinText;                     // Currency indicator.
    [SerializeField] private Image healthImage;                 // Health bar.
    [SerializeField] private HealthController healthController; // Reference to the health of the character.

    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioClip musicSoundtrack;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip coinPickupClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip dieClip;

    private int coins = 0; // Amount of coins collected.

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeSingleton();
        PlayBackgroundMusic();
    }

    private void Update()
    {
        HandleSceneRestart();
    }

    private void OnEnable()
    {
        SubscribeToHealthEvent();
    }

    private void OnDisable()
    {
        UnsubscribeFromHealthEvent();
    }

    #endregion

    #region Public Methods

    public void AddHealth(int health)
    {
        UpdateHealthBar(health);
    }

    public void AddCoins(int amount)
    {
        // Prevent negative coin amounts
        if (amount > 0)
        {
            PlayCoinPickupSound();
            Debug.Log($"Adding coins: {amount}");
            UpdateCoinCount(amount);
        }
    }

    public void PlayWalkSound()
    {
        PlaySoundEffect(walkClip);
    }

    public void PlayJumpSound()
    {
        PlaySoundEffect(jumpClip);
    }

    public void PlayIdleSound()
    {
        PlaySoundEffect(idleClip);
    }

    public void PlayRunSound()
    {
        PlaySoundEffect(runClip);
    }

    public void PlayAttackSound()
    {
        PlaySoundEffect(attackClip);
    }

    public void PlayDieSound()
    {
        PlaySoundEffect(dieClip);
    }

    #endregion

    #region Private Methods

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HandleSceneRestart()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateHealthBar(int health)
    {
        float healthPercentage = (float)health / healthController.maxLife;
        healthImage.fillAmount = healthPercentage;
    }

    private void UpdateCoinCount(int amount)
    {
        coins += amount;
        coinText.text = coins.ToString();
        Debug.Log($"Total coins: {coins}");
    }

    private void SubscribeToHealthEvent()
    {
        healthController.HealthEvent += AddHealth;
    }

    private void UnsubscribeFromHealthEvent()
    {
        healthController.HealthEvent -= AddHealth;
    }

    private void PlayBackgroundMusic()
    {
        if (loopSource != null && musicSoundtrack != null)
        {
            loopSource.clip = musicSoundtrack;
            loopSource.loop = true;
            loopSource.Play();
        }
        else
        {
            Debug.LogWarning("Loop Source or Music Soundtrack not assigned in LevelManager.");
        }
    }

    private void PlayCoinPickupSound()
    {
        if (sfxSource != null && coinPickupClip != null)
        {
            sfxSource.PlayOneShot(coinPickupClip);
        }
        else
        {
            Debug.LogWarning("SFX Source or Coin Pickup Clip not assigned in LevelManager.");
        }
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("SFX Source or Clip not assigned in LevelManager.");
        }
    }

    #endregion
}
