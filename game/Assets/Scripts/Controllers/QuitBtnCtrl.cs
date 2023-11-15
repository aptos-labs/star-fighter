using UnityEngine;
using UI = UnityEngine.UI;

public class QuitBtnCtrl : MonoBehaviour
{
  void Awake()
  {
#if !UNITY_EDITOR && UNITY_WEBGL
    this.gameObject.SetActive(false);
#endif

    var button = GetComponent<UI.Button>();
    button?.onClick?.AddListener(() => Application.Quit());
  }
}
