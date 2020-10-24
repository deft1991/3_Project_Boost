using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] private float thrustThisFrame = 10f;
    
    [SerializeField] private AudioClip mainEngineSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip levelCompleteSound;    
    
    [SerializeField] private ParticleSystem leftJetParticle;
    [SerializeField] private ParticleSystem rightJetParticle;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem successParticles;
    
    private static int _sceneIndex;
    private Rigidbody _rigidbody;
    private AudioSource _audioSource;
    private readonly float _myVolumeLevel = 1.0f;

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
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (State.Alive != _state)
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
        Invoke(nameof(LoadNextLevel), 1f); // parametrize time
    }

    private void StartDeathSequence()
    {
        _state = State.Died;
        _audioSource.Stop();
        _audioSource.PlayOneShot(deathSound, _myVolumeLevel);
        deathParticles.Play();
        Invoke(nameof(LoadFirstLevel), 2f); // parametrize time
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(++_sceneIndex);
    }

    private void LoadFirstLevel()
    {
        _sceneIndex = 0;
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (State.Alive == _state)
        {
            RespondToThrustInput();
            RespondToRotateInput();
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
            _audioSource.Stop();
            leftJetParticle.Stop();
            rightJetParticle.Stop();
        }
    }

    private void ApplyThrust()
    {
        _rigidbody.AddRelativeForce(Vector3.up * thrustThisFrame);
        if (!_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(mainEngineSound);
        }
        leftJetParticle.Play();
        rightJetParticle.Play();
    }

    private void RespondToRotateInput()
    {
        _rigidbody.freezeRotation = true; // take manual control of rotation
        float rotationThisFrame = Time.deltaTime * rcsThrust;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
            rightJetParticle.Play();
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
            leftJetParticle.Play();
        }

        _rigidbody.freezeRotation = false; // resume physics control of rotation 
    }
}