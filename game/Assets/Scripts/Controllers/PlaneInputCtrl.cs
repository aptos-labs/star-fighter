using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneInputCtrl : MonoBehaviour
{
  public float speed = 5f;
  public float rotateSpeed = 100f;

  private Rigidbody2D _rigidbody;

  // Use this for initialization
  void Awake()
  {
    this._rigidbody = GetComponent<Rigidbody2D>();
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    // Update position
    this._rigidbody.velocity = transform.up * speed;

    // Compute and update rotation
    var steerAmount = Input.GetAxis("Horizontal");
    this._rigidbody.angularVelocity = steerAmount * -rotateSpeed;
  }

  private void OnGameStart()
  {
    this.enabled = true;
  }

  private void OnGameEnd()
  {
    this.enabled = false;
  }
}
