using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour {

	// effect GameObject
	public GameObject eff1;
	public GameObject eff2;
	public GameObject eff3;
	public GameObject eff4;
	public GameObject eff5;

	public GameObject eff_name;	// effect_name_label

	public int count = 0;


	void Start () {
		eff1.SetActive (true);
		eff2.SetActive (false);
		eff3.SetActive (false);
		eff4.SetActive (false);
		eff5.SetActive (false);
		count = 0;
		NameChange ();
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			count++;

			switch (count) 
			{
			case 0: 
				eff1.SetActive (true);
				eff2.SetActive (false);
				eff3.SetActive (false);
				eff4.SetActive (false);
				eff5.SetActive (false);
				break;
			case 1:
				eff1.SetActive (false);
				eff2.SetActive (true);
				eff3.SetActive (false);
				eff4.SetActive (false);
				eff5.SetActive (false);
				break;
			case 2:
				eff1.SetActive (false);
				eff2.SetActive (false);
				eff3.SetActive (true);
				eff4.SetActive (false);
				eff5.SetActive (false);
				break;
			case 3:
				eff1.SetActive (false);
				eff2.SetActive (false);
				eff3.SetActive (false);
				eff4.SetActive (true);
				eff5.SetActive (false);
				break;
			case 4:
				eff1.SetActive (false);
				eff2.SetActive (false);
				eff3.SetActive (false);
				eff4.SetActive (false);
				eff5.SetActive (true);
				break;
			case 5: 
				eff1.SetActive (true);
				eff2.SetActive (false);
				eff3.SetActive (false);
				eff4.SetActive (false);
				eff5.SetActive (false);
				break;

			}
			if (count == 5) {
				count = 0;
			}
			NameChange ();
		}
	}



	void NameChange(){
		if (eff1.activeSelf == true) {
			eff_name.GetComponent<Text> ().text = eff1.name;
		}
		if (eff2.activeSelf == true) {
			eff_name.GetComponent<Text> ().text = eff2.name;
		}
		if (eff3.activeSelf == true) {
			eff_name.GetComponent<Text> ().text = eff3.name;
		}
		if (eff4.activeSelf == true) {
			eff_name.GetComponent<Text> ().text = eff4.name;
		}
		if (eff5.activeSelf == true) {
			eff_name.GetComponent<Text> ().text = eff5.name;
		}

	}
}
