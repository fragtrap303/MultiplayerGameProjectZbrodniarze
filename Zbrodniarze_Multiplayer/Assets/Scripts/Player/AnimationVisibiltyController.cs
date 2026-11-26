using Unity.Netcode;
using UnityEngine;

public class AnimationVisibilityController : NetworkBehaviour
{
    [SerializeField] private GameObject firstPersonHands;
    [SerializeField] private GameObject thirdPersonBody;
    [SerializeField] private Animator fpsAnimator;
    [SerializeField] private Animator tpAnimator;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Dla gracza lokalnego: pokaż widok pierwszoosobowy, ukryj widok trzecioosobowy
            ShowFirstPerson();
            HideThirdPerson();
        }
        else
        {
            // Dla innych graczy: pokaż widok trzecioosobowy, ukryj widok pierwszoosobowy
            HideFirstPerson();
            ShowThirdPerson();
        }
    }

    private void ShowFirstPerson()
    {
        firstPersonHands.SetActive(true);
        if (fpsAnimator) fpsAnimator.enabled = true;
    }

    private void HideFirstPerson()
    {
        firstPersonHands.SetActive(false);
        if (fpsAnimator) fpsAnimator.enabled = false;
    }

    private void ShowThirdPerson()
    {
        thirdPersonBody.SetActive(true);
        if (tpAnimator) tpAnimator.enabled = true;
    }

    private void HideThirdPerson()
    {
        thirdPersonBody.SetActive(false);
        if (tpAnimator) tpAnimator.enabled = false;
    }
}