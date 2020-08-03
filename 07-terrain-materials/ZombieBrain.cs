using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBrain : MonoBehaviour {

    public bool isFeral = false;

    float speed = 0;
    float targetSpeed = 0;
    public float animTransitionSpeed = 10f;

    CharacterController cc;
    Animator anim;
    public Transform target;
    Vector2 destination { get { return new Vector2(target.position.x, target.position.z); } } 


    // Quick way to get our position as a Vector2, ignoring the Y value.
    Vector2 posAsV2 { get { return new Vector2(transform.position.x, transform.position.z); } }

    private void Awake() {

        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

    }

    private void Update() {

        // Make the zombie transform look towards our destination, but ignore the Y position because
        // we don't want the whole zombie tilting up and down when the destination is higher or lower.
        transform.LookAt(new Vector3(destination.x, transform.position.y, destination.y));

        // Check if we're at our destination. We can't use == because the movements will never be that
        // that accurate and it will never evaluate to true. The result being our zombie will get to the
        // to the location and do a bit of a funny dance.
        if (Vector2.Distance(posAsV2, destination) > 0.6f) {

            // Set running speed based on the isFeral bool.
            targetSpeed = (isFeral) ? 2f : 1f;

        } else {

            targetSpeed = 0;
            anim.SetTrigger("Attack");

        }

        speed = Mathf.SmoothStep(speed, targetSpeed, Time.deltaTime * animTransitionSpeed);
        anim.SetFloat("Speed", speed);

    }

}
