using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIcon : MonoBehaviour
{
    public Color32 playerColor = Color.black;
    public Image[] playerIcons;
    public TextMeshProUGUI[] playerNames, playerNos;
    public Image[] PlayerIcons
    {
        get { return this.playerIcons; }
        set { this.playerIcons = value; }
    }

    public TextMeshProUGUI[] PlayerNames
    {
        get { return this.playerNames; }
        set { this.playerNames = value; }
    }

    public TextMeshProUGUI[] PlayerNos
    {
        get { return this.playerNos; }
        set { this.playerNos = value; }
    }

    public void SetStatus(bool _status = false, string _playerName = "", Sprite _icon = null)
    {
        this.gameObject.SetActive(_status);
        if(_status) { 
            for (int i = 0; i < this.PlayerNames.Length; i++)
            {
                if (this.PlayerNames[i] != null)
                    this.PlayerNames[i].text = _playerName;
            }

            for (int i = 0; i < this.PlayerIcons.Length; i++)
            {
                if (this.PlayerIcons[i] != null)
                {
                    this.PlayerIcons[i].sprite = _icon;
                }
            }

            for (int i = 0; i < this.PlayerNos.Length; i++)
            {
                if (this.PlayerNos[i] != null)
                {
                    this.PlayerNos[i].color = playerColor;
                }
            }
        }
    }
}
