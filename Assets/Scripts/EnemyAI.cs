﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, ITakeDamage
{
    EnemyAudioScript enemyAudioScript= new EnemyAudioScript();
    const string RUN_TRIGGER = "Run";
    const string CROUCH_TRIGGER = "Crouch";
    const string SHOOT_TRIGGER = "Shoot";

    [SerializeField] private Material deadMaterial; // Material para el estado destruido
    [SerializeField] private Collider enemyCollider; // Collider del enemigo
    [SerializeField] private float startingHealth;
    [SerializeField] private float minTimeUnderCover;
    [SerializeField] private float maxTimeUnderCover;
    [SerializeField] private int minShotsToTake;
    [SerializeField] private int maxShotsToTake;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float damage;
    [Range(0, 100)]
    [SerializeField] private float shootingAccuracy;

    [SerializeField] private Transform shootingPosition;
    [SerializeField] private ParticleSystem bloodSplatterFX;

    [Header("Haptic Vest Events")]
    [SerializeField] private UnityEvent hapticEvent1;
    [SerializeField] private UnityEvent hapticEvent2;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName; // Nombre de la siguiente escena
    
    private int remainingEnemies;
    private bool isShooting;
    private int currentShotsTaken;
    private int currentMaxShotsToTake;
    private NavMeshAgent agent;
    private Player player;
    private Transform occupiedCoverSpot;
    private Animator animator;
    [SerializeField] private AudioClip[] audios;
    private AudioSource controlAudio;
    private Renderer enemyRenderer; // Renderer del modelo del enemigo
    private bool isDestroyed = false; // Flag para verificar si el enemigo ha sido destruido
    
    // Contadores
    public static int EnemywoundedCount = 0;
    public static int EnemydeadCount = 0;

    bool flag=false;


    private float _health;
    public float health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = Mathf.Clamp(value, 0, startingHealth);
        }
    }

    private void Awake()
    {

        enemyRenderer = GetComponentInChildren<Renderer>(); // Obtener el renderer del modelo del enemigo
        
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.SetTrigger(RUN_TRIGGER);
        _health = startingHealth;
       
        controlAudio = GetComponent<AudioSource>();
    }

    public void Init(Player player, Transform coverSpot)
    {
        occupiedCoverSpot = coverSpot;
        this.player = player;
        GetToCover();
    }

    private void GetToCover()
    {
        agent.isStopped = false;
        agent.SetDestination(occupiedCoverSpot.position);
        SeleccionAudio(1, 0.2f);
    }

    private void Update()
    {
        if (agent.isStopped == false && (transform.position - occupiedCoverSpot.position).sqrMagnitude <= 0.1f)
        {
            agent.isStopped = true;
            //detenemos el la reproduccion de sonido por unica vez
            controlAudio.Stop();
            StartCoroutine(InitializeShootingCO());
           
        }
        if (isShooting)
        {
            RotateTowardsPlayer();
        }
    }

    private IEnumerator InitializeShootingCO()
    {
        HideBehindCover();
        yield return new WaitForSeconds(UnityEngine.Random.Range(minTimeUnderCover, maxTimeUnderCover));
        StartShooting();
    }

    private void HideBehindCover()
    {
        animator.SetTrigger(CROUCH_TRIGGER);
    }

    private void StartShooting()
    {
        isShooting = true;
        currentMaxShotsToTake = UnityEngine.Random.Range(minShotsToTake, maxShotsToTake);
        currentShotsTaken = 0;
        animator.SetTrigger(SHOOT_TRIGGER);
        //SeleccionAudio(0, 0.5f);
    }

    private void SeleccionAudio(int indice, float volumen)
    {
        controlAudio.PlayOneShot(audios[indice], volumen);
    }

    public void Shoot()
    {

       
            
            RaycastHit hit;
            Vector3 direction = player.GetHeadPosition() - shootingPosition.position;
            if (Physics.Raycast(shootingPosition.position, direction, out hit))
            {
                enemyAudioScript.PlayShootSound();

                Debug.DrawRay(shootingPosition.position, direction, Color.green, 2.0f);
                Player player = hit.collider.GetComponentInParent<Player>();


                if (player)
                {
                    SeleccionAudio(0, 0.5f);

                    //probabilidad del 50% de que el enemigo acierte
                    if(UnityEngine.Random.Range(0, 100) < shootingAccuracy)
                    {
                        player.TakeDamage(damage);
                    }
                 
                   
                    
                }
                else
                {
                    Debug.LogWarning("Ray hit something, but it's not the player.");
                }
            }
            else
            {
                Debug.DrawRay(shootingPosition.position, direction, Color.red, 2.0f);
                Debug.LogWarning("Ray did not hit anything.");
            }
        
        
        currentShotsTaken++;
        if (currentShotsTaken >= currentMaxShotsToTake)
        {
            StartCoroutine(InitializeShootingCO());
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = player.GetHeadPosition() - transform.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }

    public void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint)
    {   
        ParticleSystem effect = Instantiate(bloodSplatterFX, contactPoint, Quaternion.LookRotation(weapon.transform.position - contactPoint));
        effect.Play();
        health -= weapon.GetDamage();
        if (health <= 0 && !isDestroyed)
        {
           if (deadMaterial != null && enemyRenderer != null)
            {
                enemyRenderer.material = deadMaterial;
            }

            // Desactivar el collider del enemigo
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }
            
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }

            // Establecer el flag de destruido a true
            isDestroyed = true;

            // Detener el comportamiento del enemigo (pausarlo)
            agent.isStopped = true;
            animator.enabled = false;
            
            NotifyEnemyDestroyed();
            EnemydeadCount++;
        }
        else
        { //agregar 1 flag con un if aca para que solo se active 1 vez si se desea implementarlo
            EnemywoundedCount++;
        }
           

    }
/*
private void OnDestroy()
    {
        // Almacenar el número de enemigos eliminados al momento del Game Over o Victoria
        PlayerPrefs.SetInt("EnemydeadCount", EnemydeadCount);
        PlayerPrefs.Save();
    }*/

private void NotifyEnemyDestroyed()
{
    // Restar al contador de enemigos spawneados cuando un enemigo sea destruido
    EnemySpawner.totalEnemiesSpawned--;

    // Verificar si se han eliminado todos los enemigos
    if (EnemySpawner.totalEnemiesSpawned <= 0)
    {
       // PlayerPrefs.SetInt("EnemydeadCount", EnemydeadCount);
       // PlayerPrefs.Save();
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.StopTimer();
        }
        flag=true;
        PlayerPrefs.SetFloat("Elapsed_Time", Player.elapsedTime);
        PlayerPrefs.SetInt("Shots_Fired_Pistol", Pistol.shotsFiredPistol);
        PlayerPrefs.SetInt("Shots_Fired_Tester", StaticShooter.shotsFiredProbe);
        PlayerPrefs.SetInt("Enemy_Dead_Count", EnemydeadCount);
        PlayerPrefs.SetInt("Enemy_Wounded_Count", EnemywoundedCount);
        PlayerPrefs.SetInt("Civil_Dead_Count", CivilAI.CivildeadCount);
        PlayerPrefs.SetInt("Civil_Wounded_Count", CivilAI.CivilwoundedCount);

        // Llamar a la función de victoria
        
        
        //esperamos 5 segundos antes de cambiar de escena con el SceneManager sin usar invoke
        StartCoroutine(WaitAndLoadScene(5f));

    


        //SceneManager.LoadScene(nextSceneName); // Aquí asumiendo que GameManager es un singleton que maneja la lógica del juego
    }
    
}

private IEnumerator WaitAndLoadScene(float seconds)
{
    yield return new WaitForSeconds(seconds);
    SceneManager.LoadScene(nextSceneName);
}

    
    /* private void LoadNextScene()
     {
         // Verificar si el nombre de la siguiente escena está configurado
         if (!string.IsNullOrEmpty(nextSceneName))
         {
             // Cargar la siguiente escena
             SceneManager.LoadScene(nextSceneName);
         }
         else
         {
             Debug.LogWarning("El nombre de la siguiente escena no está configurado en el inspector.");
         }
     } */

    /*
     private void PlayEnemyShootSound()
     {
         audioManager.Play("EnemyShootSound");
     }
      */


      private void OnGUI()
    {
      //  GUI.Label(new Rect(30, 50, 100, 20), "Wounded: " + EnemywoundedCount);
        GUI.Label(new Rect(40, 50, 200, 20), "Amenazas abatidas: " + EnemydeadCount);
        if(flag==true){
            //texto ubicado en el medio de la pantalla, de gran tamaño
            GUI.Label(new Rect(Screen.width/2-100, Screen.height/2-50, 200, 100), "Misión cumplida");
        

        

        }
    }
}

