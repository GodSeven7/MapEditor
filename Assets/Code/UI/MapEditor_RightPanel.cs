using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditor_RightPanel : MonoBehaviour
{
    public GameObject pnl;
    public Toggle toggle;
    public Transform mapParent;

    void Start()
    {
        if (toggle && pnl)
        {
            toggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged(toggle);
            });
            pnl.SetActive(toggle.isOn);
        }
    }

    public void ToggleValueChanged(Toggle change)
    {
        pnl.SetActive(change.isOn);
    }

    public void ShowPrefab()
    {
        if (mapParent)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Tree01");
            GameObject go = GameObject.Instantiate(prefab);
            go.AddComponent<Operate_SceneObj>();
            go.transform.SetParent(mapParent);
        }
    }
}
