﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStatus : MonoBehaviour
{

    public bool frozen = false;
    public bool shrank = false;
    public bool blown = false;
    public bool oiled = false;
    public bool willDie = false;
    public bool falling = false;
    public float duration = 5f;
    public float shrinkDuration = 15;
    public float originalSpeed;
    public float originalScale;
    public ParticleSystem smoke;
    public ParticleSystem shatter;
    private IEnumerator curUnshrink;
    private IEnumerator curUnfreeze;
    private IEnumerator curUnoiled;
    public AudioSource freezeSound;
    public AudioSource oilSound;
    public float speedLimit = 500f;
    public GameObject gmtest;
    public bool play;
    public GameObject model;
    private Color originalMaterialColor;
    public GameObject tile;
    public AudioSource oildialogue;

    private void Start()
    {
        play = true;
        originalSpeed = gameObject.GetComponent<EnemyMovement>().forwardSpeed;
        originalScale = transform.localScale.x;
        gmtest = GameObject.FindWithTag("GameManager");
        tile = GameObject.Find("ElectricityTile(Clone)");
        oildialogue = tile.GetComponent<Ignite>().oildialogue;

        // Store original material
        GetMaterial();

    }

    //void FixedUpdate()
    //{
    //    if (shrank)
    //    {
    //        StartCoroutine(Unshrink());

    //        // Debug.Log("Waiting for unshrink!");
    //    }

    //    if (frozen)
    //    {
    //        StartCoroutine(Unfreeze());

    //        //Debug.Log("Waiting for unfreeze!");
    //    }

    //}

    public void Fall() {
        GetComponent<EnemyMovement>().chasing = false;
        GetComponent<EnemyMovement>().idling = false;
        GetComponent<EnemyMovement>().activated = false;
        GetComponent<EnemyMovement>().forwardSpeed = 0;

        GetComponent<NavMeshAgent>().enabled = false;

        falling = true;
        willDie = true;
    }
    //IEnumerator waitForStatusEnd(bool status)
    //{
    //    yield return new WaitForSeconds(duraiton);
    //    status = false;
    //    gameObject.GetComponent<EnemyMovement>().forwardSpeed = originalSpeed;
    //    Debug.Log("Effect debuffed!!");
    //}

    public void Shrink(float ratio)
    {
        shrank = true;
        //gameObject.GetComponent<EnemyMovement>().forwardSpeed = gameObject.GetComponent<EnemyMovement>().forwardSpeed * ratio;
        // float estimatedSpeed = gameObject.GetComponent<EnemyMovement>().forwardSpeed * ratio;
        // // reduce speed after being shrunk
        // if (estimatedSpeed <= speedLimit)
        // {
        //     gameObject.GetComponent<EnemyMovement>().forwardSpeed = estimatedSpeed;
        // }
        if (curUnshrink != null)
        {
            StopCoroutine(curUnshrink);
        }
        curUnshrink = Unshrink();
        StartCoroutine(curUnshrink);
        // Debug.Log("Waiting for unshrink!");
    }

    IEnumerator Unshrink()
    {
        yield return new WaitForSeconds(shrinkDuration);

        if (falling) {
            yield break;
        }

        // restore original size after being unshrink
        while (transform.localScale.x < originalScale)
        {
            transform.localScale = transform.localScale + new Vector3(1f, 1f, 1f) * (originalScale / 3) * Time.deltaTime;
            transform.position = new Vector3(transform.position.x,
                                             transform.localScale.y / 2,
                                             transform.position.z);
            yield return null;
        }

        // restore original speed after being unshrink
        if (!frozen)
        {
            gameObject.GetComponent<EnemyMovement>().forwardSpeed = originalSpeed;
        }

        shrank = false;
        // Debug.Log("Unshrank!!");
    }

    public void Freeze()
    {
        freezeSound.Play();
        frozen = true;
        // Enable iceCrystal renderer
        transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().enabled = true;

        gameObject.GetComponent<EnemyMovement>().forwardSpeed = 0;
        gameObject.GetComponent<NavMeshAgent>().speed = 0;
        //gameObject.GetComponent<NavMeshAgent>().SetDestination(transform.position);
        gameObject.GetComponent<EnemyMovement>().chasing = false;
        gameObject.GetComponent<EnemyMovement>().idling = false;

        if (curUnfreeze != null)
        {
            StopCoroutine(curUnfreeze);
        }
        curUnfreeze = Unfreeze();
        StartCoroutine(curUnfreeze);
        // Debug.Log("Waiting for unfreeze!");
    }

    IEnumerator Unfreeze()
    {
        yield return new WaitForSeconds(duration);

        // restore original speed after being unfrozen
        gameObject.GetComponent<EnemyMovement>().forwardSpeed = originalSpeed;

        frozen = false;
        transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        //Debug.Log("Unfrozen!!");
    }

    public void BlowAway(float seconds)
    {
        blown = true;
        float curSpeed = gameObject.GetComponent<EnemyMovement>().forwardSpeed;
        gameObject.GetComponent<EnemyMovement>().forwardSpeed = 0;

        StartCoroutine(UnBlown(seconds, curSpeed));
    }

    IEnumerator UnBlown(float seconds, float curSpeed)
    {
        yield return new WaitForSeconds(seconds);
        blown = false;

        if (falling) {
            yield break;
        }

        // restore original speed after being unfrozen
        if (frozen)
        {
        }
        else if (shrank)
        {
            gameObject.GetComponent<EnemyMovement>().forwardSpeed = curSpeed;
        }
        else
        {
            gameObject.GetComponent<EnemyMovement>().forwardSpeed = originalSpeed;
        }

    }

    public void Oiling()
    {
        if (gmtest.GetComponent<GameConstants>().completeLvl2)
        {
            if((!oildialogue.isPlaying)&&(play)&&(!tile.GetComponent<Ignite>().FireSound.isPlaying)){
                play = false;
                oilSound.Play();
                Invoke("Whatever", 10);
            }

        }
        oiled = true;

        // Darkened texture
        Darken(0.7f);

        gameObject.GetComponent<EnemyMovement>().forwardSpeed = gameObject.GetComponent<EnemyMovement>().forwardSpeed / 2;

        if (curUnoiled != null)
        {
            StopCoroutine(curUnoiled);
        }
        curUnoiled = Unoil();
        StartCoroutine(curUnoiled);
    }
    void Whatever(){
        play = true;
    }

    IEnumerator Unoil()
    {
        yield return new WaitForSeconds(duration);

        if (!falling || !frozen)
        {
            gameObject.GetComponent<EnemyMovement>().forwardSpeed = originalSpeed;
        }

        oiled = false;

        // Original texture
        RestoreColor();
    }

    public void Shock()
    {


        if (gameObject.GetComponent<EnemyStatus>().oiled)
        {
            DestroyMonster(4.5f);
            smoke.Play();

            gmtest.GetComponent<GameConstants>().enemyKillCount += 1;
            gmtest.GetComponent<GameConstants>().comboBoom += 1;
            gmtest.GetComponent<LevelManager>().myStats.GetComponent<StatsCounter>().incElectricOilKill();
        }
        //else
        //{
        //    gameObject.GetComponent<EnemyMovement>().forwardSpeed = gameObject.GetComponent<EnemyMovement>().forwardSpeed * 2;
        //}
    }

    public void DestroyMonster(float delay) {
        MeshRenderer[] allMesh = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in allMesh)
        {
            mesh.enabled = false;
        }
        SkinnedMeshRenderer[] allSkinMesh = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinMesh in allSkinMesh)
        {
            skinMesh.enabled = false;
        }
        Collider[] allCollider = GetComponentsInChildren<Collider>();
        foreach (Collider col in allCollider)
        {
            col.enabled = false;
        }
        NavMeshAgent[] allAgent = GetComponentsInChildren<NavMeshAgent>();
        foreach (NavMeshAgent agent in allAgent)
        {
            agent.enabled = false;
        }
        NavMeshObstacle[] allObs = GetComponentsInChildren<NavMeshObstacle>();
        foreach (NavMeshObstacle obs in allObs)
        {
            obs.enabled = false;
        }
        Destroy(gameObject, delay);
        willDie = true;
    }

    private void GetMaterial() {
        MeshRenderer[] allMesh = model.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in allMesh)
        {
            originalMaterialColor = mesh.material.color;
        }
        SkinnedMeshRenderer[] allSkinMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinMesh in allSkinMesh)
        {
            originalMaterialColor = skinMesh.material.color;
        }
    }

    public void Darken(float percent)
    {
        percent = Mathf.Clamp01(percent);
        MeshRenderer[] allMesh = model.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in allMesh)
        {
            mesh.material.color = new Color(originalMaterialColor.r * (1 - percent), originalMaterialColor.g * (1 - percent), originalMaterialColor.b * (1 - percent), originalMaterialColor.a);
        }
        SkinnedMeshRenderer[] allSkinMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinMesh in allSkinMesh)
        {
            skinMesh.material.color = new Color(originalMaterialColor.r * (1 - percent), originalMaterialColor.g * (1 - percent), originalMaterialColor.b * (1 - percent), originalMaterialColor.a);
        }
    }

    public void RestoreColor() {
        MeshRenderer[] allMesh = model.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in allMesh)
        {
            mesh.material.color = originalMaterialColor;
        }
        SkinnedMeshRenderer[] allSkinMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinMesh in allSkinMesh)
        {
            skinMesh.material.color = originalMaterialColor;
        }
    }

    public void ShatterMonster() {
        DestroyMonster(4.5f);
        shatter.Play();
    }
}
