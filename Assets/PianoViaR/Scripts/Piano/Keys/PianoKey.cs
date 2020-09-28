using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum KeyMode { Physical, ForShow }

public class PianoKey : MonoBehaviour
{
    public List<AudioSource> AudioSources { get; set; }
    public AudioSource CurrentAudioSource { get; set; }
    public PianoKeyController PianoKeyController { get; set; }

    public bool Sustain { get; set; }
    public float SustainSeconds { get; set; }

    // public Material changeMaterial;

    // public Material currentMaterial;

    private bool _play = false;
    private bool _played = false;
    private float _velocity;
    private float _length;
    private float _speed;
    private float _timer;
    private float _keyAngle = 360f;

    private Vector3 _position;
    private Vector3 _rotation;

    private Rigidbody _rigidbody;
    private HingeJoint _springJoint;
    private ConstantForce _constantForce;
    private IEnumerator _playCoro;
    private IEnumerator _volumeCoro;

    private List<AudioSource> _toFade = new List<AudioSource>();

    private bool _depression;
    private float _startAngle;

    // Debug
    public bool TestPlay = false;

    void Awake()
    {
        AudioSources = new List<AudioSource>();
        AudioSources.Add(GetComponent<AudioSource>());
        CurrentAudioSource = AudioSources[0];

        _rigidbody = GetComponent<Rigidbody>();
        _springJoint = GetComponent<HingeJoint>();
        _constantForce = GetComponent<ConstantForce>();

        _position = transform.position;
        _rotation = transform.eulerAngles;

        // currentMaterial = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // Constrain();

        if (_play)
        {
            KeyPlayMechanics();
        }

        if (PianoKeyController.KeyMode == KeyMode.Physical)
        {
            if (transform.eulerAngles.x > 350 && transform.eulerAngles.x < 359.5f && !_played)
            {
                if (CurrentAudioSource.clip)
                    StartCoroutine(PlayPressedAudio());

                _played = true;

                if (_toFade.Count > 0)
                {
                    FadeList();
                }
            }
            else if (transform.eulerAngles.x > 359.9 || transform.eulerAngles.x < 350)
            {
                FadeAll();

                _played = false;
            }
        }
        else if (PianoKeyController.KeyMode == KeyMode.ForShow)
        {
            if (_timer >= 1)
            {
                FadeAll();
            }

            if (_toFade.Count > 0)
            {
                FadeList();
            }
        }

        // Debug
        if (TestPlay)
        {
            Play();
            TestPlay = false;
        }
    }
    // public void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.tag == tagToIgnore)
    //     {
    //         Debug.Log("Two keys have collided:");

    //         Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());

    //         return;
    //     }

    //     Debug.Log($"{gameObject.name} with type {gameObject.GetType().Name} and tag {gameObject.tag} collided with {collision.gameObject.name} with type {collision.gameObject.GetType().Name} and tag {collision.gameObject.tag}");

    //     // if (GetComponent<Collider>().bounds.max.x > collision.contacts[0].point.x && GetComponent<Collider>().bounds.min.x < collision.contacts[0].point.x
    //     //     && GetComponent<Collider>().bounds.min.y < collision.contacts[0].point.y)
    //     // {
    //     //     Play();
    //     // }
    // }

    void Constrain()
    {
        transform.position = _position;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, _rotation.y, _rotation.z);

        if (transform.eulerAngles.x > 0 && transform.eulerAngles.x < 90)
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        if (transform.eulerAngles.x > 90 && transform.eulerAngles.x < 351)
        {
            transform.rotation = Quaternion.Euler(352, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    void KeyPlayMechanics()
    {
        if (_timer < 1)
        {
            _springJoint.useSpring = false;
            _constantForce.enabled = false;

            if (transform.eulerAngles.x < 1 || transform.eulerAngles.x > 359.5f)
            {
                _rigidbody.AddTorque(-Vector3.right * _velocity / 1024f);
            }

            if (transform.eulerAngles.x > 1)
            {
                if (PianoKeyController.KeyPressAngleDecay && _depression && transform.eulerAngles.x > PianoKeyController.PressAngleThreshold
                    || !PianoKeyController.KeyPressAngleDecay && transform.eulerAngles.x < _keyAngle)
                {
                    _keyAngle = transform.eulerAngles.x;
                }
                else
                {
                    if (transform.eulerAngles.x <= PianoKeyController.PressAngleThreshold)
                        _depression = false;

                    transform.rotation = Quaternion.Euler(_keyAngle, transform.eulerAngles.y, transform.eulerAngles.z);

                    if (PianoKeyController.KeyPressAngleDecay && !_depression && transform.eulerAngles.x < 359.5f)
                        _keyAngle += Time.deltaTime * PianoKeyController.PressAngleDecay;
                }
            }

            _timer += Time.deltaTime / _length * _speed;
        }
        else
        {
            _constantForce.enabled = true;
            _springJoint.useSpring = true;
            _play = false;
        }
    }

    void FadeAll()
    {
        if (_toFade.Count > 0)
            _toFade.RemoveRange(0, _toFade.Count);

        foreach (var audioSource in AudioSources)
        {
            if (audioSource.isPlaying)
            {
                audioSource.volume -= Time.deltaTime / (PianoKeyController.SustainPedalPressed ? PianoKeyController.SustainSeconds : 1f);

                if (audioSource.volume <= 0)
                    audioSource.Stop();
            }
        }

        // MeshRenderer currentRenderer = GetComponent<MeshRenderer>();
        // currentRenderer.material = currentMaterial;
        // Debug.Log($"{this} - New Material: {changeMaterial.name}");
    }

    void FadeList()
    {
        for (int i = 0; i < _toFade.Count; i++)
        {
            if (_toFade[i].isPlaying)
            {
                _toFade[i].volume -= Time.deltaTime * 0.5f;

                if (_toFade[i].volume <= 0)
                {
                    _toFade[i].volume = 0;
                    _toFade[i].Stop();
                    _toFade.Remove(_toFade[i]);
                    break;
                }
            }
        }
    }

    public void Play(float velocity = 10, float length = 1, float speed = 1)
    {
        _keyAngle = 360f;

        if (_play)
        {
            if (PianoKeyController.RepeatedKeyTeleport)
                transform.rotation = Quaternion.Euler(_keyAngle, transform.eulerAngles.y, transform.eulerAngles.z);
            else
                _rigidbody.AddTorque(Vector3.right * 127);
        }

        // StartCoroutine(ChangeMaterial(length * 2));

        _velocity = velocity;
        _length = length;
        _speed = speed;
        _timer = 0;
        _play = true;
        _depression = true;


        if (PianoKeyController.KeyMode == KeyMode.ForShow)
            PlayVirtualAudio();
    }

    // IEnumerator ChangeMaterial(float noteDuration)
    // {
    //     MeshRenderer currentRenderer = GetComponent<MeshRenderer>();
    //     currentRenderer.material = changeMaterial;
    //     Debug.Log($"{this} - New Material: {changeMaterial.name}");

    //     yield return new WaitForSeconds(noteDuration);
    //     yield return new WaitForFixedUpdate();
    //     yield return new WaitForFixedUpdate();

    //     currentRenderer.material = currentMaterial;
    //     Debug.Log($"{this} - New Material: {changeMaterial.name}");
    // }

    IEnumerator PlayPressedAudio()
    {
        if (!PianoKeyController.NoMultiAudioSource && CurrentAudioSource.isPlaying)
        {
            bool foundReplacement = false;
            int index = AudioSources.IndexOf(CurrentAudioSource);

            for (int i = 0; i < AudioSources.Count; i++)
            {
                if (i != index && (!AudioSources[i].isPlaying || AudioSources[i].volume <= 0))
                {
                    foundReplacement = true;
                    CurrentAudioSource = AudioSources[i];
                    _toFade.Remove(AudioSources[i]);
                    break;
                }
            }

            if (!foundReplacement)
            {
                AudioSource newAudioSource = CloneAudioSource();
                AudioSources.Add(newAudioSource);
                CurrentAudioSource = newAudioSource;
            }

            _toFade.Add(AudioSources[index]);
        }

        _startAngle = transform.eulerAngles.x;

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (Mathf.Abs(_startAngle - transform.eulerAngles.x) > 0)
        {
            CurrentAudioSource.volume = Mathf.Lerp(0, 1, Mathf.Clamp((Mathf.Abs(_startAngle - transform.eulerAngles.x) / 2f), 0, 1));
        }

        CurrentAudioSource.Play();
    }

    void PlayVirtualAudio()
    {
        if (!PianoKeyController.NoMultiAudioSource && CurrentAudioSource.isPlaying)
        {
            bool foundReplacement = false;
            int index = AudioSources.IndexOf(CurrentAudioSource);

            for (int i = 0; i < AudioSources.Count; i++)
            {
                if (i != index && (!AudioSources[i].isPlaying || AudioSources[i].volume <= 0))
                {
                    foundReplacement = true;
                    CurrentAudioSource = AudioSources[i];
                    _toFade.Remove(AudioSources[i]);
                    break;
                }
            }

            if (!foundReplacement)
            {
                AudioSource newAudioSource = CloneAudioSource();
                AudioSources.Add(newAudioSource);
                CurrentAudioSource = newAudioSource;
            }

            _toFade.Add(AudioSources[index]);
        }

        CurrentAudioSource.volume = _velocity / 127f;

        CurrentAudioSource.Play();
    }

    AudioSource CloneAudioSource()
    {
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        newAudioSource.volume = CurrentAudioSource.volume;
        newAudioSource.playOnAwake = CurrentAudioSource.playOnAwake;
        newAudioSource.spatialBlend = CurrentAudioSource.spatialBlend;
        newAudioSource.clip = CurrentAudioSource.clip;
        newAudioSource.outputAudioMixerGroup = CurrentAudioSource.outputAudioMixerGroup;

        return newAudioSource;
    }
}
