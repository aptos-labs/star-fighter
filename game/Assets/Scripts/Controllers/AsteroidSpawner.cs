using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
  public GameObject[] prefabs;
  public float initialVelocityMultiplier = 1f;
  public float initialRotationMultiplier = 0.5f;

  // Start is called before the first frame update
  private void Start()
  {
    makeClouds();
  }

  void makeClouds()
  {

    for (float i = -60; i < 60; i += (int)Random.Range(4f, 14f))
    {
      for (float j = -60; j < 60; j += (int)Random.Range(4f, 14f))
      {
        // Don't spawn in the initial area
        if (System.Math.Abs(i) < 7 && System.Math.Abs(j) < 7)
        {
          continue;
        }

        var cloudPrefab = this.prefabs[Random.Range(0, prefabs.Length)];
        var position = new Vector3(i, j, 0f);
        var rotation = Quaternion.identity;
        var scale = Random.Range(0.4f, 0.6f);
        var prefab = GameObject.Instantiate(cloudPrefab, position, rotation, this.transform);
        prefab.transform.localScale = new Vector3(scale, scale, scale);
        prefab.GetComponent<Rigidbody2D>().velocity = initialVelocityMultiplier * (new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        prefab.GetComponent<Rigidbody2D>().angularVelocity = initialRotationMultiplier * Random.Range(-1f, 1f);
      }
    }
  }
}
