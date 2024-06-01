using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class Evaluator : MonoBehaviour
{
    [Header("Settings")]
    public float maxSimulationTime = 90f; // 1 minute and 30 seconds
    public float maxScore = 10f;
    public float minPassingScore = 6f;
    public float scorePenaltyPerEnemy = 1f;
    public float scorePenaltyForCivilian = 2f;
    public float scorePenaltyForExtraBullet = 0.25f;
    public float scorePenaltyForUnloading = 1f;

    private float currentScore=10f;
    private float elapsedTime;
    private bool simulationEnded = false;
    private bool PlayerDead = false;

    private int enemytoneutralize=0;
    private int civilianhit=0;
    private int totalEnemys;
     private int bullettouse;
     private int bulletused;
     private SpawnManager spawnManager;
     Pistol pistol;

     private char condition;
    void Start()
    {
        spawnManager = FindObjectOfType<SpawnManager>();
        
        bullettouse=spawnManager.GetTotalEnemies();
        bulletused=0;
        
        //currentScore = maxScore;
        elapsedTime = 0f;
        pistol = FindObjectOfType<Pistol>();
    }

    void Update()
    {
        if (simulationEnded)
            return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= maxSimulationTime)
        {
            CheckUnloadPenalty();
            EndSimulation();
        }
    }
    public void EnemyNeutralized()
    {
        enemytoneutralize--;
    }
    public void EnemyMissed()
    {
        currentScore -= scorePenaltyPerEnemy*(totalEnemys+enemytoneutralize);
    }

    public void CivilianHit()
    {
        currentScore -= scorePenaltyForCivilian;
    }

    public void BulletUsed()
    {
         bulletused++;
    }

    public void TotalpointBulletUsed()
    {
        currentScore -= scorePenaltyForExtraBullet*(bulletused-bullettouse);
    }

    public void CheckUnloadPenalty()
    {
       
        if (pistol != null && (!pistol.isSafetyOn))
        {
            UnloadPenalty();
        }
    }

    public void UnloadPenalty()
    {
        currentScore -= scorePenaltyForUnloading;
        
    }
    
    public void ReceiveShot()
    {   PlayerDead=true;
        EndSimulation();
    }

    public void EarlyEndSimulation(){
        Invoke("EndSimulation", 5f);
    }
    private void EndSimulation()
    {   totalEnemys=spawnManager.GetTotalEnemies();
        Debug.Log("Total Enemies to Spawn: " + spawnManager.GetTotalEnemies());
        Debug.Log("Total Civilians to Spawn: " + spawnManager.GetTotalCivilians());
        EnemyMissed();
        TotalpointBulletUsed();

        simulationEnded = true;

        //SaveDataToCSV();
        if (currentScore < minPassingScore || PlayerDead==true)
        {
            condition='D';
            SceneManager.LoadScene("3 GameOver");
        }
        else
        {
            condition='A';
            SceneManager.LoadScene("4 Victory");
        }

        //mover todos los playerPrefs aca

    }

    public float GetCurrentScore()
    {
        return currentScore;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public bool IsSimulationEnded()
    {
        return simulationEnded;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(40, 50, 200, 20), "Amenazas restantes: " + enemytoneutralize);
        GUI.Label(new Rect(450, 20, 300, 20), "Tiempo transcurrido: " + elapsedTime);
        GUI.Label(new Rect(40, 90, 200, 20), "Disparos realizados: " + bulletused);
        GUI.Label(new Rect(40, 110, 200, 20), "Puntaje actual: " + currentScore);
    }
/*
    private void SaveDataToCSV()
{
    // Crear el nombre del archivo con la fecha y hora actual
    string fileName = "evaluator_data_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
    string filePath;

    // Verificar si se está ejecutando en un Oculus Quest 2
   /* if (IsOculusQuest2())
    {
        // Si es un Oculus Quest 2, guardar en la carpeta Documents
        filePath = Path.Combine("/storage/emulated/0/Documents", fileName);
    }
    else
    {
        // De lo contrario, guardar en la carpeta StreamingAssets
        filePath = Path.Combine(Application.streamingAssetsPath, fileName);
    //}

    // Crear el archivo CSV y escribir los datos
    using (StreamWriter writer = new StreamWriter(filePath))
    {
        // Escribir los encabezados de las columnas
        writer.WriteLine("Tiempo de simulacion;Enemigos Faltantes;Civiles impactados;Exceso de balas;Seguro al final del escenario;Muerte del agente;Puntaje Final;Condicion Aprobado o Desaprobado");

        // Escribir los datos de las variables
        writer.WriteLine($"{elapsedTime};{totalEnemys + enemytoneutralize};{civilianhit};{bulletused - bullettouse};{pistol.isSafetyOn};{PlayerDead};{currentScore};{condition}");
    }

    Debug.Log("Datos guardados en: " + filePath);
}
*/
// Método para verificar si se está ejecutando en un Oculus Quest 2
/*
private bool IsOculusQuest2()
{
    // Verificar si el dispositivo actual es de la familia Oculus Quest
    if (XRDevice
    {
        // Verificar si es un Oculus Quest 2 basado en su nombre de modelo
        if (XRDevice.model.Contains("Quest 2"))
        {
            return true;
        }
    }
    
    return false;
}*/

}