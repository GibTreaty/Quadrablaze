using UnityEngine;

public class OmniJetController : MonoBehaviour {

    [SerializeField]
    float _speed;

    public Animator shipAnimator;

    public ParticleSystem jet1;
    public ParticleSystem jet2;
    public ParticleSystem jet3;
    public ParticleSystem jet4;

    public AudioSource jetAudio1;
    public AudioSource jetAudio2;
    public AudioSource jetAudio3;
    public AudioSource jetAudio4;

    public Light light1;
    public Light light2;
    public Light light3;
    public Light light4;

    Vector3 _moveDirection;

    float lightIntensity1;
    float lightIntensity2;
    float lightIntensity3;
    float lightIntensity4;

    public Vector3 MoveDirection {
        get { return _moveDirection; }
        set { _moveDirection = value; }
    }

    public float Speed {
        get { return _speed; }
        set { _speed = value; }
    }

    void Awake() {
        lightIntensity1 = light1.intensity;
        lightIntensity2 = light2.intensity;
        lightIntensity3 = light3.intensity;
        lightIntensity4 = light4.intensity;

        light1.intensity = 0;
        light2.intensity = 0;
        light3.intensity = 0;
        light4.intensity = 0;
    }

    void Update() {
        UpdateJet(jet1, "Shield1Opened", jetAudio1, light1, lightIntensity1);
        UpdateJet(jet2, "Shield2Opened", jetAudio2, light2, lightIntensity2);
        UpdateJet(jet3, "Shield3Opened", jetAudio3, light3, lightIntensity3);
        UpdateJet(jet4, "Shield4Opened", jetAudio4, light4, lightIntensity4);
    }

    public void UpdateJet(ParticleSystem jet, string animatorBool, AudioSource audio, Light light, float defaultLightIntensity) {
        float dot = Mathf.Clamp01(Vector3.Dot(MoveDirection, transform.position - jet.transform.position));
        //jet.startSpeed = Speed * dot;
        var info = jet.main;

        info.startSpeed = Speed * dot;

        light.intensity = dot > .1f ? dot * defaultLightIntensity : 0;

        if(jet.isPlaying) {
            if(dot == 0) {
                shipAnimator.SetBool(animatorBool, false);
                jet.Stop();
                audio.Stop();
            }
        }
        else {
            if(dot > .1f) {
                shipAnimator.SetBool(animatorBool, true);
                jet.Play();
                audio.Play();
            }
        }
    }
}