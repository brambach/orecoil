using UnityEngine;

public class Sfx : MonoBehaviour
{
    public AudioSource oneShot;
    public AudioClip eat;
    public AudioClip shieldOn;
    public AudioClip shieldExpire;
    public AudioClip shieldPop;
    public AudioClip death;

    public void PlayEat() { if (eat && oneShot) oneShot.PlayOneShot(eat); }
    public void PlayShieldOn() { if (shieldOn && oneShot) oneShot.PlayOneShot(shieldOn); }
    public void PlayShieldExpire() { if (shieldExpire && oneShot) oneShot.PlayOneShot(shieldExpire); }
    public void PlayShieldPop() { if (shieldPop && oneShot) oneShot.PlayOneShot(shieldPop); }
    public void PlayDeath() { if (death && oneShot) oneShot.PlayOneShot(death); }
}