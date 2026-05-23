using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    public int id;
    [SerializeField] private float deactivateDelay = 1.5f;
    [SerializeField] private GameObject destroyEffect;

    public void Interact(PlayerContoller player)
    {
        int itemId = id;

        if (player.IsCollected(itemId))
            return;

        player.MarkCollected(itemId);
        player.TriggerCollectUI(itemId);
        player.PickupItemSound();

        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null)
        {
            playerAnim.SetTrigger("Pick");
        }

        Collect();
        GameManager.Instance.GetScore(25, 1);
    }

    public void Collect()
    {
        Invoke(nameof(Deactivate), deactivateDelay);
    }

    private void Deactivate()
    {
        GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        Destroy(effect, 2f);
        gameObject.SetActive(false);
    }
}