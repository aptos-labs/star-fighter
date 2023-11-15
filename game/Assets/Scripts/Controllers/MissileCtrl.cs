using UnityEngine;

public class MissileCtrl : MonoBehaviour
{
  public GameObject explosionEffectPrefab;

  public Transform target;
  public float speed = 6f;
  public float rotationSpeed = 120f;
  public float dustWait = .05f;

  private Rigidbody2D _rigidbody;

  // Use this for initialization
  void Start()
  {
    this._rigidbody = GetComponent<Rigidbody2D>();
  }

  void FixedUpdate()
  {
    this._rigidbody.velocity = transform.up * speed;
    if (target != null)
    {
      Vector2 direction = (Vector2)target.position - this._rigidbody.position;
      direction.Normalize();
      float angle = Vector3.Cross(direction, transform.up).z;
      this._rigidbody.angularVelocity = -rotationSpeed * angle;
    }
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    blowUpSelf();
  }

  void blowUpSelf()
  {
    GameObject tempExplosion = GameObject.Instantiate(this.explosionEffectPrefab, transform.position, this.explosionEffectPrefab.transform.rotation);
    GameObject.Destroy(tempExplosion, 1.2f);
    GameObject.Destroy(gameObject);
  }

  private void OnGameStart()
  {
    // Cleanup on new game
    GameObject.Destroy(gameObject);
  }
}
