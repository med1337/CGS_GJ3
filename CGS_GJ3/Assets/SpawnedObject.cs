﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour {

    [SerializeField] private float breakForce;
    public AudioSource audioSource;
    [SerializeField] private AudioClip spawnAudioClip;
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField] private ParticleSystem spawnParticleSystem;
    [SerializeField] private ParticleSystem deathParticleSystem;
    [SerializeField] private LayerMask dieOnContactWith;
    [SerializeField] private Animator animator;
    [SerializeField] public Collider myCollider;
    [SerializeField] private string boolName= "GrowTriggered";
    private MeshRenderer mr;
    public Vector3 desiredScale = Vector3.one;
    public bool AnimationFinished = false;
    public bool RandomRotation = false;

    // Use this for initialization
    void Start ()
    {
        mr = GetComponentInChildren<MeshRenderer>();
        //audioSource = GetComponent<AudioSource>();
        if (RandomRotation)
            transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360),0);
        OnSpawn();
	}
	
    private IEnumerator WaitForFinish()
    {
        while(!AnimationFinished)
        {
            yield return new WaitForEndOfFrame();
        }
        animator.enabled = false;
        transform.localScale = desiredScale;
    }
  

    public void PlaySound()
    {
        if(audioSource)
         SoundManager.instance.PlaySingleAtSource(audioSource, spawnAudioClip);
    }

    public virtual void OnSpawn()
    {
        //animator.SetBool(boolName, true);
        StartCoroutine(WaitForFinish());
        if (audioSource )
        {
            PlaySound();
            
        }
        else
        {
            //SoundManager.instance.PlaySingle(spawnAudioClip);
        }
        if (spawnParticleSystem!=null)
            spawnParticleSystem.Play();
    }

    public virtual void OnDeath()
    {
        if (audioSource!=null && deathAudioClip !=null)
        {
            SoundManager.instance.PlaySingleAtSource(audioSource, deathAudioClip);
        }
        if (deathParticleSystem != null)
            deathParticleSystem.Play();
        Destroy(gameObject, 1f);
    }

    public virtual void OnUpdate()
    {
        if(spawnParticleSystem!=null && mr!=null)
        {
            if (!mr.isVisible && spawnParticleSystem.isPlaying)
            {
                spawnParticleSystem.Stop();
            }
        }
        
    }

    public virtual void OnFixedUpdate()
    {

    }


    // Update is called once per frame
    void Update () {
        if(AnimationFinished)
            OnUpdate();
	}

    void FixedUpdate()
    {
        OnFixedUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (((1 << collision.gameObject.layer) & dieOnContactWith) != 0)
        {
            //It matched layer
            OnDeath();
        }
        
    }
}
