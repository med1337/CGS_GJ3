﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class dynamite : MonoBehaviour {

    public bool lit = false;
    private float fuse_timer = 0.0f;
    private float fuse_spark_speed = 0.0f;

    [SerializeField] float fuse_length = 5.0f;

    [SerializeField] ParticleSystem explosion_prefab;
    [SerializeField] ParticleSystem fire_prefab;
    [SerializeField] ParticleSystem sparks;
    [SerializeField] VRTK_InteractableObject linkedObject;
    [SerializeField] Transform fuse_start_pos;
    [SerializeField] Transform fuse_end_pos;
    private AudioSource audioSource;

    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (linkedObject.IsGrabbed() && !lit)
        {
            LightFuse();
        }

		if (lit)
        {
            fuse_timer += Time.deltaTime;

            fuse_spark_speed += Time.deltaTime / fuse_length;

            sparks.gameObject.transform.position = Vector3.Lerp(fuse_start_pos.position, fuse_end_pos.position, fuse_spark_speed);

            if (fuse_timer > fuse_length)
            {
                Explode();
            }
        }
	}

    public void LightFuse()
    {
        if (!lit)
        {
            lit = true;
            sparks.Play();
        }
    }

    private void Explode()
    {
        lit = false;
        Instantiate(explosion_prefab, transform.position, transform.rotation);
        RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position, new Vector3(1, 1, 1)), 25);
        foreach (var item in hits)
        {
            var so = item.collider.GetComponent<SpawnedObject>();
            if(so!=null)
            {
                so.OnDeath();
            }

            var dy = item.collider.GetComponent<dynamite>();
            if (dy != null)
            {
                dy.LightFuse();
            }
        }
        SoundManager.instance.PlaySingleAtSource(audioSource);
        Destroy(this.gameObject,2);
    }
}
