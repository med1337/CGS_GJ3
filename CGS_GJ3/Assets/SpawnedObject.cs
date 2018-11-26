﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour {

    [SerializeField] private float breakForce;
    private AudioSource audioSource;
    [SerializeField] private AudioClip spawnAudioClip;
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField] private ParticleSystem spawnParticleSystem;
    [SerializeField] private ParticleSystem deathParticleSystem;
    [SerializeField] private LayerMask dieOnContactWith;
    [SerializeField] private Animator animator;
    [SerializeField] private string boolName= "GrowTriggered";
    public Vector3 desiredScale = Vector3.one;
    public bool AnimationFinished = false;
    public bool RandomRotation = false;

    // Use this for initialization
    void Start ()
    {
        audioSource = GetComponent<AudioSource>();
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

    public virtual void OnSpawn()
    {
        //animator.SetBool(boolName, true);
        StartCoroutine(WaitForFinish());
        //if(audioSource)
        //{
        //    SoundManager.instance.PlaySingleAtSource(audioSource,spawnAudioClip);
        //}
        //spawnParticleSystem.Play();
    }

    public virtual void OnDeath()
    {
        SoundManager.instance.PlaySingle(deathAudioClip);
        deathParticleSystem.Play();
    }

    public virtual void OnUpdate()
    {

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
