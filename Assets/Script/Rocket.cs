using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rotateThrust = 100f;
    [SerializeField] private float thrustThisFrame = 2000f;
    [SerializeField] private float successLoadDelay = 1f;
    [SerializeField] private float deathLoadDelay = 2f;

    [SerializeField] private AudioClip mainEngineSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip levelCompleteSound;

    [SerializeField] private ParticleSystem leftJetParticle;
    [SerializeField] private ParticleSystem rightJetParticle;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem successParticles;

    private int _levelIndex;
    private Rigidbody _rigidbody;
    private AudioSource _audioSource;
    private readonly float _myVolumeLevel = 1.0f;
    private bool _isCollisionEnabled = true;

    private enum State
    {
        Alive,
        Died,
        Transcending
    }

    private State _state = State.Alive;

    // Start is called before the first frame update
    void Start()
    {
        _levelIndex = SceneManager.GetActiveScene().buildIndex;
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (State.Alive != _state || !_isCollisionEnabled)
        {
            return; // ignore collisions when dead
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                print("Ok"); //todo remove this line
                break;
            case "Finish":
                StartFinishSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartFinishSequence()
    {
        _state = State.Transcending;
        _audioSource.Stop();
        _audioSource.PlayOneShot(levelCompleteSound, _myVolumeLevel);
        successParticles.Play();
        Invoke(nameof(LoadNextLevel), successLoadDelay);
    }

    private void StartDeathSequence()
    {
        _state = State.Died;
        _audioSource.Stop();
        _audioSource.PlayOneShot(deathSound, _myVolumeLevel);
        deathParticles.Play();
        Invoke(nameof(ReLoadLevel), deathLoadDelay);
    }

    /**
     * Go to the next level.
     * If level max --> go to he first level
     */
    private void LoadNextLevel()
    {
        var currentSceneIdx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIdx == SceneManager.sceneCountInBuildSettings - 1 ? 0 : ++currentSceneIdx);
    }

    /**
     * Go to the level before till zero level
     */
    private void ReLoadLevel()
    {
        SceneManager.LoadScene(_levelIndex == 0 ? _levelIndex : --_levelIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (State.Alive == _state)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            _isCollisionEnabled = !_isCollisionEnabled; // toggle
        }
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) // can thrust while rotating
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void ApplyThrust()
    {
        _rigidbody.AddRelativeForce(Vector3.up * (thrustThisFrame * Time.deltaTime));
        if (!_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(mainEngineSound);
        }

        leftJetParticle.Play();
        rightJetParticle.Play();
    }

    private void StopApplyingThrust()
    {
        _audioSource.Stop();
        leftJetParticle.Stop();
        rightJetParticle.Stop();
    }

    private void RespondToRotateInput()
    {
        float rotationThisFrame = Time.deltaTime * rotateThrust;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            RotateManually(rotationThisFrame);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            RotateManually(-rotationThisFrame);
        }
    }

    private void RotateManually(float rotationThisFrame)
    {
        _rigidbody.freezeRotation = true; // take manual control of rotation
        transform.Rotate(Vector3.forward * rotationThisFrame);
        rightJetParticle.Play();
        _rigidbody.freezeRotation = false; // resume physics control of rotation 
    }
}