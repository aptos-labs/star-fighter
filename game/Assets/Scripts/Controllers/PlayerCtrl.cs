using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
  private SpriteRenderer spriteRenderer;
  private Vector3 initialPosition;
  private Quaternion initialRotation;

  private void Awake()
  {
    initialPosition = transform.localPosition;
    initialRotation = transform.localRotation;
    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    this.blowUp(other.transform);
  }

  private void blowUp(Transform missile)
  {
    spriteRenderer.gameObject.SetActive(false);

    // GameObject brokenPlane = GameObject.Instantiate(brokenPlanePrefab, this.transform.position, this.transform.rotation, this.transform);
    // foreach (Transform child in brokenPlane.transform)
    // {
    //   var partRigidbody = child.GetComponent<Rigidbody2D>();
    //   var forceDirection = (Vector2)missile.position - partRigidbody.position;
    //   partRigidbody.AddForce(forceDirection * -5f, ForceMode2D.Impulse);
    // }

    SendMessageUpwards("OnPlayerExploded", SendMessageOptions.DontRequireReceiver);
  }

  private void OnGameEnd()
  {

  }

  private void OnGameStart()
  {
    spriteRenderer?.gameObject?.SetActive(true);
    transform.localPosition = initialPosition;
    transform.localRotation = initialRotation;
  }
}
