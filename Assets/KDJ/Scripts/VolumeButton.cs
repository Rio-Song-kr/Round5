using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeButton : MonoBehaviour
{
    [SerializeField] private GameObject _volumePanel;

    public void OnVolumeButtonClicked()
    {
        if (_volumePanel != null)
        {
            _volumePanel.SetActive(!_volumePanel.activeSelf);
        }
    }
}
