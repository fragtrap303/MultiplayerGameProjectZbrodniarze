using UnityEngine;

public class LocalTest : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private WeaponSelectionUI selectionUI;

    private int[] seeds = new int[3];
    private int chosenSeed;

    private void Start()
    {
        roundManager.OnSelectionStarted += OnSelectionStarted;
        roundManager.OnRoundStarted += OnRoundStarted;
        selectionUI.OnTileChosen += OnTileChosen;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) roundManager.StartRound();  // start gry
        if (Input.GetKeyDown(KeyCode.F2)) roundManager.EndRound();    // udaj czyj¹œ œmieræ
    }

    private void OnSelectionStarted()
    {
        for (int i = 0; i < seeds.Length; i++)
            seeds[i] = Random.Range(int.MinValue, int.MaxValue);

        selectionUI.Show(seeds);
    }

    private void OnTileChosen(int index)
    {
        chosenSeed = seeds[index];
        roundManager.SubmitChoice(0);
        roundManager.SubmitChoice(1);   // udajemy, ¿e drugi gracz te¿ wybra³
    }

    private void OnRoundStarted()
    {
        BaseShooting shooting = FindFirstObjectByType<BaseShooting>();
        if (shooting == null) return;

        WeaponGenerator.Generate(chosenSeed, shooting.Weapon);
        shooting.ApplyWeapon(shooting.Weapon);
        roundManager.TeleportToSpawn(shooting.transform.root, 0);
    }
}