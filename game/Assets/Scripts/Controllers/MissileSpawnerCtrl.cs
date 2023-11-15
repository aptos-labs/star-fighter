using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileSpawnerCtrl : MonoBehaviour
{
  public GameObject missilePrefab;
  public Transform target;

  private Coroutine _spawnerCoroutine;

  public float spawnMinTime = 3f;
  public float spawnMaxTime = 5f;
  public UnityEngine.AnimationCurve spawnRateCurve;

  private void OnEnable()
  {
    this._spawnerCoroutine = StartCoroutine(MissileSpawner());
  }

  private void OnDisable()
  {
    this.StopCoroutine(this._spawnerCoroutine);
  }

  IEnumerator MissileSpawner()
  {
    float elapsed = 0f;
    while (this.enabled)
    {
      int j = 0;
      int i = 0;
      if (this.target.rotation.z < 180)
      {
        i = 10;
        j = 8;
      }
      else
      {
        i = -10;
        j = -8;
      }

      var offset = new Vector3(Random.Range(j, i), Random.Range(j, i), 0f);
      Vector3 spawnPosition = this.target.position + offset;
      Quaternion spawnRotation = Quaternion.identity;
      var missile = GameObject.Instantiate(missilePrefab, spawnPosition, spawnRotation, this.transform);
      var missileCtrl = missile.GetComponent<MissileCtrl>();
      missileCtrl.target = this.target;

      var maxTime = spawnRateCurve.keys[spawnRateCurve.length - 1].time;
      var multiplier = spawnRateCurve.Evaluate(System.Math.Min(elapsed, maxTime - 0.5f));

      float delay = Random.Range(this.spawnMinTime, this.spawnMaxTime) * multiplier;

      yield return new WaitForSeconds(delay);
      elapsed += delay;
    }
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
