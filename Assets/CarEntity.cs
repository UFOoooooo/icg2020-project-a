using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEntity : MonoBehaviour
{
    public BoxCollider parallelParkingAreaCollider;
    public BoxCollider reverseParkingAreaCollider;

    public Camera camera_parallelParking;
    public Camera camera_reverseParking;
    public Camera camera_wheel;

    public GameObject wheelFrontRight;
    public GameObject wheelFrontLeft;
    public GameObject wheelBackRight;
    public GameObject wheelBackLeft;
    public GameObject textTarget;
    public GameObject textTarget_success;

    MeshCollider carBodyMeshCollider;

    float m_FrontWheelAngle = 0;
    const float WHEEL_ANGLE_LIMIT = 40f;
    public float turnAngularVelocity = 0.1f;

    public float m_Velocity;

    public float acceleration = 0.01f;
    public float deceleration = 1f;
    public float maxVelocity = 1f;

    float m_DeltaMovement;

    const float WHEEL_DISTANCE = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        camera_parallelParking.enabled = false;
        camera_reverseParking.enabled = false;
        textTarget.SetActive(false);
        textTarget_success.SetActive(false);

        carBodyMeshCollider = this.transform.Find("body").GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            m_Velocity = Mathf.Min(maxVelocity, m_Velocity + Time.fixedDeltaTime * acceleration);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            m_Velocity = Mathf.Max(-1f, m_Velocity - Time.fixedDeltaTime * deceleration);
        }
        else
        {
            if (m_Velocity > 0)
            {

                m_Velocity = Mathf.Abs(m_Velocity - Time.fixedDeltaTime * acceleration * 3);
                if (m_Velocity < 1) m_Velocity = 0;
            }
            else
            {
                m_Velocity = Mathf.Abs(m_Velocity + Time.fixedDeltaTime * acceleration * 3);
                if (m_Velocity > -1) m_Velocity = 0;
            }
        }

        m_DeltaMovement = m_Velocity * Time.fixedDeltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            m_FrontWheelAngle = Mathf.Clamp(
                m_FrontWheelAngle + Time.fixedDeltaTime * turnAngularVelocity,
                -WHEEL_ANGLE_LIMIT,
                WHEEL_ANGLE_LIMIT);
            UpdateWheels();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            m_FrontWheelAngle = Mathf.Clamp(
                m_FrontWheelAngle - Time.fixedDeltaTime * turnAngularVelocity,
                -WHEEL_ANGLE_LIMIT,
                WHEEL_ANGLE_LIMIT);
            UpdateWheels();
        }
        else
        {
            if (m_FrontWheelAngle > 0)
            {
                m_FrontWheelAngle -= Time.fixedDeltaTime * turnAngularVelocity * 4;
                if (m_FrontWheelAngle < 4) m_FrontWheelAngle = 0;
            }
            else if (m_FrontWheelAngle < 0)
            {
                m_FrontWheelAngle += Time.fixedDeltaTime * turnAngularVelocity * 4;
                if (m_FrontWheelAngle > -4) m_FrontWheelAngle = 0;
            }
            UpdateWheels();
        }

        float bodyAngleDelta =
            1 / WHEEL_DISTANCE *
            Mathf.Tan(Mathf.Deg2Rad * m_FrontWheelAngle) *
            m_DeltaMovement;

        this.transform.Rotate(0f, -bodyAngleDelta * Mathf.Rad2Deg, 0f);

        this.transform.Translate(Vector3.forward * m_DeltaMovement);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.name.Substring(0,12));
        //if (collision.gameObject.name.Substring(0, 12) == "roadTile_002")
        //{
        //    textTarget.SetActive(true);
        //    stop();
        //}
        foreach (ContactPoint contact in collision.contacts)
        {
            var colName = contact.thisCollider.name;
            if (colName == "body")
            {
                //Debug.Log(colName);
                textTarget.SetActive(true);

                if (collision.gameObject.name.Length>=12 &&
                    collision.gameObject.name.Substring(0, 12) == "roadTile_002")
                {
                    stop();
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            var colName = contact.thisCollider.name;
            if (colName == "body")
            {
                //Debug.Log(colName);
                textTarget.SetActive(true);
                stop();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        textTarget.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "parallelParkingDetect")
        {
            camera_parallelParking.enabled = true;
        }
        else if(other.name == "reverseParkingDetect")
        {
            camera_reverseParking.enabled = true;
        }

        if (other.name == "startAreaDetect")
        {
            camera_wheel.enabled = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (parallelParkingAreaCollider.bounds.Intersects(carBodyMeshCollider.bounds))
        {
            if(parallelParkingAreaCollider.bounds.Contains(carBodyMeshCollider.bounds.max)
                && parallelParkingAreaCollider.bounds.Contains(carBodyMeshCollider.bounds.min))
            {
                textTarget_success.SetActive(true);
            }
            else
            {
                textTarget_success.SetActive(false);
            }
        }else if (reverseParkingAreaCollider.bounds.Intersects(carBodyMeshCollider.bounds))
        {
            if (reverseParkingAreaCollider.bounds.Contains(carBodyMeshCollider.bounds.max)
                && reverseParkingAreaCollider.bounds.Contains(carBodyMeshCollider.bounds.min))
            {
                textTarget_success.SetActive(true);
            }
            else
            {
                textTarget_success.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "parallelParkingDetect")
        {
            camera_parallelParking.enabled = false;
        }
        else if (other.name == "reverseParkingDetect")
        {
            camera_reverseParking.enabled = false;
        }

        if (other.name == "startAreaDetect")
        {
            camera_wheel.enabled = false;
        }
    }

    void stop()
    {
        m_Velocity = 0;
    }

    void UpdateWheels()
    {
        Vector3 localEulerAngles_left = new Vector3(0f, m_FrontWheelAngle, 0f);
        Vector3 localEulerAngles_right = new Vector3(0f, -m_FrontWheelAngle + 180, 0f);
        wheelFrontLeft.transform.localEulerAngles = localEulerAngles_left;
        wheelFrontRight.transform.localEulerAngles = localEulerAngles_right;
    }
}
