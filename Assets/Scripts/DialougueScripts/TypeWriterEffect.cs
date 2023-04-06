using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
[SerializeField] private float TypeWriterSpeed = 50f; 


public void Run(string textToType, TMP_Text textLabel)
   {
	   StartCoroutine(TypeText(textToType, textLabel));
   }
   
   private IEnumerator TypeText(string textToType, TMP_Text textLabel){

		textLabel.text = string.Empty;
		
	   yield return new WaitForSeconds(3);
	   float t = 0;
	   int CharIndex = 0;

	   while (CharIndex < textToType.Length)
	   {
		   t += Time.deltaTime * TypeWriterSpeed;
		   CharIndex = Mathf.FloorToInt(t);
		   CharIndex = Mathf.Clamp(CharIndex, 0, textToType.Length);

		   textLabel.text = textToType.Substring(0, CharIndex);

		   yield return null;
	   }
	   textLabel.text = textToType;
   }
}
