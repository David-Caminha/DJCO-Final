using UnityEngine;
using UnityEngine.Networking;

public class Frenzy : Skill {

    public AudioSource skillSoundSource;
    public AudioClip frenzySound;

    private bool growing;
    private bool unGrowing;
    private float time;
    private Vector3 normalScale;
    private Vector3 bigScale;
    private float frenzyDuration;

    public override void Activate()
    {
        CmdStartGrowing();
        var playerStats = transform.GetComponent<PlayerStats>();
        playerStats.CmdActivateFrenzy(frenzyDuration);
    }

    // Use this for initialization
    void Start () {
        time = 0;
        frenzyDuration = 10f;
        growing = false;
        unGrowing = false;
        normalScale = new Vector3(1, 1, 1);
        bigScale = new Vector3(2, 2, 2);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (growing)
        {
            transform.localScale = Vector3.Lerp(normalScale, bigScale, time / 1.5f);
            time += Time.deltaTime;
            if (time >= 1.7f)
            {
                Invoke("Ungrow", frenzyDuration);
                growing = false;
                time = 0;
            }
        }
        if (unGrowing)
        {
            transform.localScale = Vector3.Lerp(bigScale, normalScale, time / 1.5f);
            time += Time.deltaTime;
            if (time >= 1.7)
            {
                unGrowing = false;
                time = 0;
            }
        }
    }

    void Ungrow()
    {
        unGrowing = true;
        //RESET STATS
    }

    [Command]
    void CmdStartGrowing()
    {
        RpcStartGrowing();
    }

    [ClientRpc]
    void RpcStartGrowing()
    {
        skillSoundSource.clip = frenzySound;
        skillSoundSource.Play();
        growing = true;
    }
}
