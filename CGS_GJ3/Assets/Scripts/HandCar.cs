﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using UnityEngine.SceneManagement;

public class HandCar : MonoBehaviour
{
    enum PumpState
    {
        IDLE = 0,
        UP = 1,
        DOWN = 2,
        GRABBED = 3
    }
    public float slowdownSpeed = 2.0f;
    private Track currentTrack;
    [SerializeField] private Transform chassis;
    [SerializeField] private float speedModifier = 5;
    [SerializeField] private Transform movementLever;
    [SerializeField] private Transform brakeLever;
    [SerializeField] private List<Transform> wheelTransforms;
    private bool coroutineStarted = false;
    private float modifier = 1; private PumpState pumpState = PumpState.IDLE;
    private float timer = 0.0f;
    public Rigidbody myRigidbody;
    public bool WillHit = false;
    private float ClosestPoint;
    public float input = 0.0f;
    public float vrinput = 0.0f;
    public float speed;
    [SerializeField] private AudioClip track;
    private int trackCount = 0;
    private float soundtimer = 0f;
    private bool live = true;
    private bool coroutineStarted2 = false;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceAmbient;
    [SerializeField] private List<ParticleSystem> brakingParticles;

    // Use this for initialization
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.centerOfMass = Vector3.down;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") && other.impulse.magnitude > 0)
        {
            Debug.Log(other.gameObject.name + ", V: " + other.relativeVelocity.magnitude + ", I: " + other.impulse.magnitude);

            live = false;
        }
        if (other.gameObject.CompareTag("Tile"))
        {
            live = false;
            Debug.Log("DED");
        }
    }

    void GetCurrentTrack()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Tracks");

        RaycastHit hit;

        if (Physics.Raycast(transform.position + transform.up * 5, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {

            var prevTrack = currentTrack;

            currentTrack = hit.collider.gameObject.GetComponentInParent<Track>();

            if (currentTrack != prevTrack)
            {

                if (soundtimer > 0.5f)
                {
                    if (currentTrack.playSound)
                    {
                        currentTrack.playSound = false;
                        //SoundManager.instance.PlaySingleAtSource(audioSource, track);
                        soundtimer= 0.0f;
                    }
                }
                ResetPosition();
            }
            else
            {
                currentTrack = prevTrack;
            }
        }
    }

    private IEnumerator Shake()
    {
        //chassis.localPosition -= Vector3.up * 0.01f * UnityEngine.Random.Range(1.0f,5.0f);
        //while (chassis.localPosition.y<0)
        //{
        //    chassis.localPosition += Time.fixedDeltaTime * Vector3.up;
        //    yield return null;
        //}
        //chassis.localPosition = Vector3.zero;
        yield return null;
    }


    private IEnumerator Ded()
    {
        coroutineStarted2 = true;
           yield return new WaitForSeconds(3.0f);
        Destroy(TileManager.instance.gameObject);
        SceneManager.LoadScene(0);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!live)
        {
            if(!coroutineStarted2)
            {
                StartCoroutine(Ded());
            }
            return;

        }
        soundtimer += Time.fixedDeltaTime;
            GetCurrentTrack();
        BrakeLever();
        MoveLever();

        #region debug
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            speedModifier++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            speedModifier--;
        }


        if (Input.GetKeyDown(KeyCode.U))
        {
            AddForce(1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Brake(1);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            Brake(0);

        }
        #endregion


        
        ClosestPoint = currentTrack.spline.ClosestPoint(transform.position);

        if (ClosestPoint > 1)
        {
            ClosestPoint = 1 - ClosestPoint;
            currentTrack = currentTrack.forwardTrack;
        }

        var position = transform.position + transform.forward * Time.fixedDeltaTime * input * speedModifier;
        myRigidbody.MovePosition(position);


        var newforward = currentTrack.spline.Forward(ClosestPoint);
        if (newforward != Vector3.zero)
        {
            var transformtemp = transform;
            if (currentTrack.spline.direction == Pixelplacement.SplineDirection.Backwards)
            {
                newforward = -newforward;
            }

            transformtemp.forward = newforward;
            myRigidbody.MoveRotation(transformtemp.rotation);
        }



        if (pumpState == PumpState.IDLE)
            input += (0 - input) * (slowdownSpeed / 100f);
        if (input < 0.0f)
            input = 0.0f;


        speed = input * 2;
        if (speed < 3 && !WillHit)
        {
            myRigidbody.isKinematic = true;
        }
        else
        {
            myRigidbody.isKinematic = false;
        }
    }

    private void Update()
    {
        foreach (var item in wheelTransforms)
        {
            item.Rotate(Vector3.right, speed);
        }
        var s2 = speed;
        if(s2>5)
        {
            s2 = 5;
        }
        audioSourceAmbient.volume = TileManager.instance.Remap(s2, 0.5f, 5, 0, 0.2f);
        audioSourceAmbient.pitch = TileManager.instance.Remap(s2, 0, 5, 0, 1);
        //axle.Rotate(Vector3.right, spd);
        //axle2.Rotate(Vector3.right, spd);
    }

    internal void SetPosition()
    {
        var newPos = currentTrack.spline.GetPosition(ClosestPoint, true, 100);
        newPos.y = transform.position.y;
        myRigidbody.MovePosition(newPos);
        myRigidbody.MoveRotation(Quaternion.Euler(0, 0, 0));
    }

    internal void ResetPosition()
    {
        ClosestPoint = currentTrack.spline.ClosestPoint(transform.position);
        var newPos = transform.position;
        newPos.x = currentTrack.spline.GetPosition(ClosestPoint, true, 1000).x;
        newPos.z = currentTrack.spline.GetPosition(ClosestPoint, true, 1000).z;
        transform.position = newPos;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(myRigidbody.worldCenterOfMass, 0.01f);
    //}

    private IEnumerator WaitForInput(bool positive)
    {
        if (positive)
        {

            pumpState = PumpState.DOWN;
        }
        else
        {
            pumpState = PumpState.UP;
        }
        coroutineStarted = true;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            if (positive)
            {
                if (vrinput > 0)
                {
                    SoundManager.instance.PlaySingle("choose");
                    modifier++;
                    coroutineStarted = false;
                    if (modifier > 100)
                    {
                        modifier = 100;
                    }

                    yield break;
                }
            }
            else
            {
                if (vrinput < 0)
                {
                    SoundManager.instance.PlaySingle("choose");
                    modifier++;
                    if (modifier > 100)
                    {
                        modifier = 100;
                    }
                    coroutineStarted = false;
                    yield break;
                }

            }
            timer += Time.unscaledDeltaTime;
            yield return null;

        }

        modifier = 20;
        coroutineStarted = false;
    }

    private void Brake(float force)
    {
        //foreach (var wheel in motorWheels)
        //{
        //    wheel.brakeTorque = force * speedModifier;
        //}

        float dampeningForce = 0.001f;

        input -= (force * dampeningForce);

        if (input < 0)
        {
            input = 0;
        }
    }

    private void AddForce(float force)
    {
        ClosestPoint += Time.unscaledDeltaTime;
        if (ClosestPoint > 1)
        {
            ClosestPoint = 1 - ClosestPoint;
            currentTrack = currentTrack.forwardTrack;
        }

    }

    private void BrakeLever()
    {
        bool disable_sparks = false;

        float spd = input * 3.14f;
        float spark_amplifier = 50.0f;

        float force = brakeLever.localEulerAngles.x;
        //check if brake lever is pulled
        if ((360 - force) < 30)
        {
            force = (360 - force) / 10;
            force = force * force;

            if ((360 - force) > 10)
            {
                if (spd > 7.0f)
                {
                    foreach (var item in brakingParticles)
                    {
                        var emitter = item.emission;
                        float em = spark_amplifier * input * force;

                        emitter.rateOverTime = em;
                    }
                }
            }

            Brake(force);
        }

        else
        {
            disable_sparks = true;
        }

        if (spd < 7.0f)
        {
            disable_sparks = true;
        }

        if (disable_sparks)
        {
            foreach (var item in brakingParticles)
            {
                var emitter = item.emission;
                emitter.rateOverTime = 0;
            }
        }
    }

    private void MoveLever()
    {

        //within movement range
        if (movementLever.parent.localEulerAngles.x >= 330 || movementLever.parent.localEulerAngles.x <= 30)
        {
            if (vrinput == 0)
            {
                timer += Time.unscaledDeltaTime;
            }
            else if (!coroutineStarted && vrinput != 0)
            {
                if (pumpState == PumpState.IDLE)
                {
                    pumpState = PumpState.GRABBED;
                }
                var force = vrinput * Time.fixedDeltaTime *speedModifier ;
                modifier += Mathf.Abs(force);
                //movementLever.Rotate(Vector3.right, force);
                timer = 0.0f;
                input += Mathf.Abs(force) * modifier/100;
                if(input>19f)
                {
                    input = 19f;
                }
            }
            if (timer >= 1.0f)
            {
                pumpState = PumpState.IDLE;
            }
        }


        if (movementLever.parent.localEulerAngles.x > 15 && movementLever.parent.localEulerAngles.x < 180)
        {
            if (!coroutineStarted && pumpState != PumpState.UP)
                StartCoroutine(WaitForInput(false));

            //if (movementLever.localEulerAngles.x > 15)
            //    movementLever.localRotation = Quaternion.Euler(14.99f, 0, 0);
            //TODO: UI
            //Debug.Log("GO DOWN");
        }
        if (movementLever.parent.localEulerAngles.x < 345 && movementLever.parent.localEulerAngles.x > 180)
        {
            //if (movementLever.localEulerAngles.x < 345)
            //    movementLever.localRotation = Quaternion.Euler(345.01f, 0, 0);

            if (!coroutineStarted && pumpState != PumpState.DOWN)
                StartCoroutine(WaitForInput(true));

            //TODO: UI
            //Debug.Log("GO UP");
            //movementLever.localRotation.SetEulerAngles(14.99f, 0, 0);
        }

    }
}
