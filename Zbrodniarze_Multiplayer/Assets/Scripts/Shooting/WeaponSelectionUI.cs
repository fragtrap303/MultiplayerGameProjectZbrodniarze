using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;            
    [SerializeField] private Button[] tileButtons;        
    [SerializeField] private TMP_Text[] tileTexts;        
    [SerializeField] private WeaponConfig[] previewConfigs; 
    [SerializeField] private TMP_Text timerText;          
    [SerializeField] private float selectionTime = 10f;

    public event Action<int> OnTileChosen;   

    private int[] seeds;
    private bool chosen;
    private float timeLeft;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Show(int[] tileSeeds)
    {
        seeds = tileSeeds;
        chosen = false;
        timeLeft = selectionTime;
        panel.SetActive(true);

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (int i = 0; i < tileButtons.Length; i++)
        {
            int idx = i;
            WeaponGenerator.Generate(seeds[i], previewConfigs[i]);
            tileTexts[i].text = BuildText(previewConfigs[i]);

            tileButtons[i].onClick.RemoveAllListeners();
            tileButtons[i].onClick.AddListener(() => Choose(idx));
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!panel.activeSelf || chosen) return;

        timeLeft -= Time.deltaTime;
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0f)
            Choose(UnityEngine.Random.Range(0, seeds.Length));  
    }

    private void Choose(int index)
    {
        if (chosen) return;
        chosen = true;
        Hide();
        OnTileChosen?.Invoke(index);
    }

    private static string BuildText(WeaponConfig w)
    {
        string mode = w.fireMode == FireMode.Single ? "Pojedynczy"
                    : w.fireMode == FireMode.Automatic ? "Automat" : "Seria";
        string mods = string.Join("\n", w.modifierLabels);
        return $"<b>{mode}</b>\n" +
               $"Obra¿enia: {w.damage}\n" +
               $"Szybkostrzelnoœæ: {1f / w.fireRate:0.#}/s\n" +
               $"Magazynek: {w.magazineSize}\n" +
               $"Prze³adowanie: {w.reloadTime:0.#}s\n\n{mods}";
    }
}