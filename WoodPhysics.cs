using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodPhysics : MonoBehaviour
{
    Collider fireRadius;

    public GameObject fireParticles;
    public GameObject smokeParticles;
    public GameObject distortionParticles;
    public GameObject dustExplosion;

    GameObject tempFire;
    GameObject tempSmoke;
    GameObject tempDistortion;
    GameObject tempDust;

    public Vector3 instantiatePos;

    public float woodHealth;
    public float fireDPS;
    public bool onFire;
    public bool fireContact;
    public bool fireCreated;
    public float dustMaxTimeSet;
    float fireTimer;

    // Start is called before the first frame update
    void Start()
    {
        fireRadius = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        Ignition();
        BurnDamage();
    }

    void Ignition()
    {
        if (fireContact)
        {
            fireTimer = fireTimer + Time.fixedDeltaTime;
        }
        else
        {
            fireTimer = 0;
        }
        if (fireTimer > 2)
        {
            onFire = true;
        }
    }

    void BurnDamage()
    {
        if (onFire)
        {
            if (fireCreated == false)
            {
                Particles();
                fireCreated = true;
            }

            woodHealth = woodHealth - fireDPS * Time.fixedDeltaTime;

            //GetComponent<Renderer>().material.SetColor("_Color", new Vector4(woodHealth * 2.55f, woodHealth * 2.55f, woodHealth * 2.55f, 255));

        }
        else
        {
            Destroy(tempFire);
            Destroy(tempSmoke);
            Destroy(tempDistortion);
            fireCreated = false;
        }
        
        if (woodHealth < 0)
        {
            tempDust = Instantiate(dustExplosion, instantiatePos + transform.position, gameObject.transform.rotation);
            tempDust.AddComponent<DestroyTimer>();
            tempDust.GetComponent<DestroyTimer>().dustExplosion = true;
            tempDust.GetComponent<DestroyTimer>().dustMaxTime = dustMaxTimeSet;
            //Destroy(tempFire);
            //Destroy(tempSmoke);
            //Destroy(tempDistortion);
            Destroy(gameObject);
        }
    }

    void Particles()
    {
        tempFire = Instantiate(fireParticles, instantiatePos + transform.position, gameObject.transform.rotation, gameObject.transform);
        tempSmoke = Instantiate(smokeParticles, instantiatePos + transform.position, gameObject.transform.rotation, gameObject.transform);
        tempDistortion = Instantiate(distortionParticles, instantiatePos + transform.position, gameObject.transform.rotation, gameObject.transform);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("Fire"))
        {
            onFire = true;
        }
        if (other.gameObject.CompareTag("Ice"))
        {
            onFire = false;
        }
    }

}


