using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialougueManager1 : MonoBehaviour
{
	[SerializeField] private TMP_Text textLabel;

	private void Start(){
		GetComponent<TypeWriterEffect>().Run("This is a bit of text!\n Hello", textLabel);
	}
}
