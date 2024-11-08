using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    public int playerId = -1;
    public TextMeshProUGUI content;
    private Image cellImage = null;
    public Sprite[] cellSprites;
    public Color32 defaultColor = Color.black;
    public Color32 selectedColor = Color.white;
    public int row;
    public int col;
    public bool isSelected = false;

    public void SetTextContent(int _playerId=0, string letter="", Color _color = default, Sprite gridSprite = null)
    {
        if(gridSprite != null) this.cellSprites[0] = gridSprite;
        this.playerId = _playerId;
        if (this.cellImage == null) 
            this.cellImage = this.GetComponent<Image>();

        this.SetButtonColor(_color);
        this.cellImage.sprite = this.cellSprites[0];

        if (this.content != null) {
            this.content.text = letter;
            this.content.color = this.defaultColor;
        }
    }

    public void SetButtonColor(Color _color = default)
    {
        if (_color != default(Color))
            this.cellImage.color = _color;
        else
            this.cellImage.color = Color.white;
    }

    public void Selected()
    {
        this.isSelected = true;
        if (this.content != null)
        {
            this.content.color = this.selectedColor;
        }
        this.cellImage.color = this.defaultColor;
        //this.cellImage.sprite = this.cellSprites[1];
    }

    public void DisSelected()
    {
        this.isSelected = false;
        if (this.content != null)
        {
            this.content.color = this.defaultColor;
        }
        this.cellImage.color = this.selectedColor;
        //this.cellImage.sprite = this.cellSprites[0];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var player = GameController.Instance.playerControllers[this.playerId];
        // Only initiate drawing if it's not already selected
        if (!isSelected && !player.IsConnectWord)
        {
            player.StartConnection();
            player.SelectCell(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var player = GameController.Instance.playerControllers[this.playerId];
        if (player.IsConnectWord) // Check if drawing is active
        {
            player.SelectCell(this);
        }
    }

}
