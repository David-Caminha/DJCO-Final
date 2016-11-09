using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [HideInInspector]
    public ThirdPersonController localPlayer;

    public Sprite[] skillsImage;
    public Image[] skillButtonsBackground;
    public Image skill1Image;
    public Image skill2Image;
    public Button playButton;
    public GameObject HUD;
    public GameObject playerUI;
    public GameObject selectionPanel;

    private int skillsSelected;

	// Use this for initialization
	void Start () {
        playButton.interactable = false;
        skillsSelected = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SelectSkill(int index)
    {
        if(skillsSelected < 2)
        {
            if (skillsSelected == 0)
            {
                skill1Image.sprite = skillsImage[index];
            }
            else if (skillsSelected == 1)
            {
                skill2Image.sprite = skillsImage[index];
            }
            skillButtonsBackground[index].GetComponentInChildren<Button>().interactable = false;
            skillButtonsBackground[index].color = Color.white;
            localPlayer.skills[index].enabled = true;
            skillsSelected++;
        }
        if(skillsSelected == 2)
        {
            playButton.interactable = true;
        }
    }

    public void Reset()
    {
        skillsSelected = 0;
        playButton.interactable = false;
        foreach (Skill s in localPlayer.skills)
            s.enabled = false;
        foreach (var i in skillButtonsBackground)
        {
            i.color = Color.clear;
            i.GetComponentInChildren<Button>().interactable = true;
        }
    }

    public void Play()
    {
        HUD.SetActive(true);
        playerUI.SetActive(true);
        selectionPanel.SetActive(false);
        localPlayer.enabled = true;
    }
}
