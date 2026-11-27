using System;
using System.Collections;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Maps")]
    [SerializeField] private GameObject[] mapPrefabs;

    [Header("Round")]
    [SerializeField] private float delayAfterKill = 4f;

    private GameObject currentMapInstance;
    private MapData currentMapData;
    private int currentMapIndex = -1;
    private bool roundActive;
    private bool transitioning;
    private bool[] chosen = new bool[2];

    
    public event Action OnSelectionStarted;   //kafelki z broni¹
    public event Action<int> OnMapLoaded;     
    public event Action OnRoundStarted;       
    public event Action OnRoundEnded;         //ktoœ zgin¹³

    public int CurrentMapIndex => currentMapIndex;
    public bool RoundActive => roundActive;

    

   
    public void StartRound()
    {
        if (transitioning) return;
        StartCoroutine(StartRoundRoutine(0f));
    }

    public void EndRound()
    {
        if (!roundActive || transitioning) return;

        roundActive = false;
        OnRoundEnded?.Invoke();
        StartCoroutine(StartRoundRoutine(delayAfterKill));
    }

    
    public void SubmitChoice(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < chosen.Length)
            chosen[playerIndex] = true;
    }

    
    public int GetRandomMapIndex()
    {
        if (mapPrefabs.Length == 0) return -1;

        int newIndex = UnityEngine.Random.Range(0, mapPrefabs.Length);
        if (mapPrefabs.Length > 1)
        {
            while (newIndex == currentMapIndex)
                newIndex = UnityEngine.Random.Range(0, mapPrefabs.Length);
        }
        return newIndex;
    }

   
    public void LoadMap(int index)
    {
        if (index < 0 || index >= mapPrefabs.Length)
        {
            return;
        }

        if (currentMapInstance != null)
            Destroy(currentMapInstance);

        currentMapInstance = Instantiate(mapPrefabs[index], Vector3.zero, Quaternion.identity);
        currentMapData = currentMapInstance.GetComponent<MapData>();
        currentMapIndex = index;

        if (currentMapData == null)
            

        OnMapLoaded?.Invoke(index);
    }

    public Transform GetSpawnPoint(int playerIndex)
    {
        if (currentMapData == null || currentMapData.spawnPoints == null) return null;
        if (playerIndex < 0 || playerIndex >= currentMapData.spawnPoints.Length) return null;
        return currentMapData.spawnPoints[playerIndex];
    }

    
    public void TeleportToSpawn(Transform player, int playerIndex)
    {
        Transform sp = GetSpawnPoint(playerIndex);
        if (player == null || sp == null) return;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(sp.position + Vector3.up * 0.1f, sp.rotation);

        if (cc != null) cc.enabled = true;
    }

    

    private IEnumerator StartRoundRoutine(float delay)
    {
        transitioning = true;
        yield return new WaitForSeconds(delay);

        //wybór broni 
        chosen[0] = false;
        chosen[1] = false;
        OnSelectionStarted?.Invoke();

        while (!(chosen[0] && chosen[1]))
            yield return null;

        //mapa
        LoadMap(GetRandomMapIndex());

        //start
        transitioning = false;
        roundActive = true;
        OnRoundStarted?.Invoke();
    }
}