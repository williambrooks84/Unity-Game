using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarAI : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public Transform waypointContainer;
    public bool loop = true;

    [Header("Movement")]
    public float maxSpeed = 8f;  
    public float acceleration = 10f;
    public float steeringSpeed = 180f; 
    public float stoppingDistance = 1.0f; 
    public float slopeLimitDegrees = 50f;
    public float wallCheckDistance = 1.2f;
    [Header("Grounding")]
    public bool stickToGround = true;
    public float groundCheckDistance = 3f;
    public float groundProbeRadius = 0.5f;
    public float groundOffset = 0.5f;
    public LayerMask groundLayers = ~0;

    [Header("Wheels / Visuals")]
    public Transform wheelFL; 
    public Transform wheelFR; 
    public Transform wheelRL; 
    public Transform wheelRR;

    public float wheelRadius = 0.33f;
    public float maxSteerAngle = 30f;

    int _currentIndex = 0;
    Rigidbody _rb;
    List<Transform> _points = new List<Transform>();
    Vector3 _startPosition;
    
    [Header("AI")]
    public bool randomDrive = false;
    public float roamRadius = 12f;
    public float randomChangeInterval = 3f;
    float _randomTimer = 0f;
    Vector3 _randomTarget;

    [Header("Combat")]
    public int collisionDamage = 25;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        _startPosition = transform.position;

        CollectWaypoints();
        if (_points.Count == 0)
        {
            Debug.LogWarning("CarAI: No waypoints assigned or found. Add waypoints or assign a waypointContainer.");
        }

        if (randomDrive)
        {
            ChooseRandomTarget();
        }

        IgnoreOtherCars();
    }

    void CollectWaypoints()
    {
        _points.Clear();
        if (waypoints != null && waypoints.Length > 0)
        {
            foreach (var t in waypoints) if (t != null) _points.Add(t);
        }
        else if (waypointContainer != null)
        {
            for (int i = 0; i < waypointContainer.childCount; i++)
            {
                var c = waypointContainer.GetChild(i);
                if (c != null) _points.Add(c);
            }
        }
        else
        {
            var gos = GameObject.FindGameObjectsWithTag("Waypoint");
            foreach (var g in gos) if (g != null) _points.Add(g.transform);
        }
    }

    void FixedUpdate()
    {
        if (!randomDrive && _points.Count == 0) return;

        Vector3 targetPos;
        float dist;
        Vector3 planarToTarget;

        if (randomDrive)
        {
            targetPos = _randomTarget;
            Vector3 toTarget = targetPos - transform.position;
            planarToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            dist = planarToTarget.magnitude;

            _randomTimer -= Time.fixedDeltaTime;
            if (_randomTimer <= 0f || dist <= stoppingDistance)
            {
                ChooseRandomTarget();
            }
        }
        else
        {
            var t = _points[_currentIndex];
            if (t == null) return;
            targetPos = t.position;
            Vector3 toTarget = targetPos - transform.position;
            planarToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            dist = planarToTarget.magnitude;
        }

        if (planarToTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(planarToTarget.normalized, Vector3.up);
            float maxDeg = steeringSpeed * Time.fixedDeltaTime;
            Quaternion rot = Quaternion.RotateTowards(_rb.rotation, desired, maxDeg);
            _rb.MoveRotation(rot);
        }

        float speed = (dist > stoppingDistance) ? maxSpeed : 0f;

        Vector3 groundNormal = Vector3.up;
        bool grounded = false;
        if (stickToGround)
        {
            grounded = StickToGround(out groundNormal);
        }

        float maxSlopeCos = Mathf.Cos(slopeLimitDegrees * Mathf.Deg2Rad);
        bool slopeTooSteep = grounded && groundNormal.y < maxSlopeCos;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit wallHit, wallCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            if (wallHit.rigidbody != _rb && wallHit.normal.y < 0.25f)
            {
                slopeTooSteep = true;
            }
        }

        if (slopeTooSteep)
        {
            speed = 0f;
        }

        Vector3 moveForward = transform.forward;
        if (grounded)
        {
            moveForward = Vector3.ProjectOnPlane(moveForward, groundNormal).normalized;
            if (moveForward == Vector3.zero) moveForward = transform.forward;
        }

        Vector3 desiredVel = moveForward * speed;
        Vector3 newVel = new Vector3(desiredVel.x, _rb.linearVelocity.y, desiredVel.z);
        _rb.linearVelocity = newVel;

        if (!randomDrive && dist <= stoppingDistance)
        {
            _currentIndex++;
            if (_currentIndex >= _points.Count)
            {
                if (loop) _currentIndex = 0;
                else _currentIndex = _points.Count - 1;
            }
        }

        AnimateWheels(speed);
    }

    void ChooseRandomTarget()
    {
        _randomTimer = randomChangeInterval;
        Vector2 rnd = Random.insideUnitCircle * roamRadius;
        _randomTarget = _startPosition + new Vector3(rnd.x, 0f, rnd.y);
    }

    void OnCollisionEnter(Collision collision)
    {
        var target = collision.collider.GetComponentInParent<Health>();
        if (target != null)
        {
            target.TakeDamage(collisionDamage);
        }
    }

    void AnimateWheels(float forwardSpeed)
    {
        if (wheelRadius <= 0f) wheelRadius = 0.3f;
        float angleDelta = 0f;
        angleDelta = (forwardSpeed / wheelRadius) * Mathf.Rad2Deg * Time.fixedDeltaTime;

        if (wheelRL != null) wheelRL.Rotate(Vector3.right, angleDelta, Space.Self);
        if (wheelRR != null) wheelRR.Rotate(Vector3.right, angleDelta, Space.Self);
        if (wheelFL != null) wheelFL.Rotate(Vector3.right, angleDelta, Space.Self);
        if (wheelFR != null) wheelFR.Rotate(Vector3.right, angleDelta, Space.Self);

        if (wheelFL != null || wheelFR != null)
        {
            Vector3 velPlanar = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);
            float steer = 0f;
            if (velPlanar.sqrMagnitude > 0.001f)
            {
                steer = Vector3.SignedAngle(transform.forward, velPlanar.normalized, Vector3.up);
                steer = Mathf.Clamp(steer, -maxSteerAngle, maxSteerAngle);
            }

            if (wheelFL != null)
            {
                var local = wheelFL.localEulerAngles;
                local.y = steer;
                wheelFL.localEulerAngles = local;
            }
            if (wheelFR != null)
            {
                var local = wheelFR.localEulerAngles;
                local.y = steer;
                wheelFR.localEulerAngles = local;
            }
        }
    }

    public void RefreshWaypoints()
    {
        CollectWaypoints();
        _currentIndex = 0;
    }

    bool StickToGround(out Vector3 groundNormal)
    {
        groundNormal = Vector3.up;
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        bool hitFound = Physics.SphereCast(origin, groundProbeRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
        if (!hitFound)
        {
            hitFound = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
        }

        if (hitFound)
        {
            if (hit.rigidbody == _rb) return false; 

            groundNormal = hit.normal;
            float targetY = hit.point.y + groundOffset;
            Vector3 pos = _rb.position;
            pos.y = targetY;
            _rb.MovePosition(pos);

            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            _rb.linearVelocity = v;

            return true;
        }

        return false;
    }

    void IgnoreOtherCars()
    {
        var myColliders = GetComponentsInChildren<Collider>();
        var cars = FindObjectsOfType<CarAI>();
        foreach (var car in cars)
        {
            if (car == this) continue;
            var otherCols = car.GetComponentsInChildren<Collider>();
            foreach (var a in myColliders)
            {
                foreach (var b in otherCols)
                {
                    Physics.IgnoreCollision(a, b, true);
                }
            }
        }
    }
}
