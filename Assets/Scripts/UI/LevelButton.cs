using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // For Image

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelIndexDisplay;
    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private GameObject _starIconHolder;
    [SerializeField] private Sprite _goldStarSprite;
    [SerializeField] private Sprite _blackStarSprite;
    private Image _buttonBackground;

    private int _levelIndex;
    private bool _isOpen;
    private int _numberOfStar;

    public void InitializeButton(int index, bool isOpen, int numberOfStar)
    {
        _levelIndex = index;
        _isOpen = isOpen;
        _numberOfStar = numberOfStar;
        _buttonBackground = GetComponent<Image>();
        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        var starIcons = _starIconHolder.GetComponentsInChildren<Image>().ToList();

        if (!_isOpen)
        {
            _lockIcon.SetActive(true);
            _levelIndexDisplay.gameObject.SetActive(false);
            _starIconHolder.SetActive(false);
            _buttonBackground.color = Color.gray;
        }
        else
        {
            _lockIcon.SetActive(false);
            _levelIndexDisplay.gameObject.SetActive(true);
            _buttonBackground.color = Color.white;

            for (int i = 0; i < starIcons.Count; i++)
            {
                if (i < _numberOfStar)
                {
                    starIcons[i].sprite = _goldStarSprite;
                }
                else
                {
                    starIcons[i].sprite = _blackStarSprite;
                }
            }
        }
        _levelIndexDisplay.text = "Level " + _levelIndex;
    }
}
