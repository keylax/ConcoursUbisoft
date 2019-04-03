using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AMenuOption : MonoBehaviour
{
    public bool isActive = false;
    private Image _image;
    protected Text _text;

    private Color _activeColor;
    private Color _inactiveColor;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        _activeColor = new Color(0.6862f, 0.6862f, 0.6862f);
        _inactiveColor = Color.white;
        _image = GetComponentInChildren<Image>();
        _text = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_image == null)
            return;
        if (isActive)
        {
            _image.color = _activeColor;
            if (_text)
                _text.color = Color.white;
        }
        else
        {
            _image.color = _inactiveColor;
            if (_text)
                _text.color = Color.black;
        }
    }

    public abstract void Validate();
}
