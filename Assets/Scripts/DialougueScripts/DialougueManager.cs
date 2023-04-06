using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialougueManager : MonoBehaviour
{

    public GameObject dBox;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Text;
    public string[] lines;
    public float textspeed;

    private int index;

    

    // Start is called before the first frame update
    void Start()
    {
        Text.text = string.Empty;
        //StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartDialougue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine(){
        foreach(char c in lines[index].ToCharArray())
        {
            Text.text +=c;
            yield return new WaitForSeconds(textspeed);
        }
    }
}
