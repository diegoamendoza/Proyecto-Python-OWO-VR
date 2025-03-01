using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutController : MonoBehaviour
{
    public Transform Apart;
	public Transform Bpart;
    public float rotationSpeed = 1.0f;
	public List<Material> mats;
	public float magnitude = 0;
	public float multiplier = -1;
	void Start()
    {
        
    }

    void Update()
    {
		Vector3 brotation = new Vector3(-HandInputController.instance.vectorInput.x, HandInputController.instance.vectorInput.y * multiplier, HandInputController.instance.vectorInput.z);
        
        Apart.Rotate(HandInputController.instance.vectorInput * Time.deltaTime * rotationSpeed);
        Bpart.Rotate(brotation * Time.deltaTime * rotationSpeed);
		

		if (HandInputController.instance.isFist)
		{
			magnitude = Mathf.Lerp(magnitude,0.04f,Time.deltaTime * 10f);
		}
		else
		{
			magnitude = Mathf.Lerp(magnitude, 0.01f, Time.deltaTime * 10f);
		}
		for (int i = 0; i < mats.Count; ++i)
		{
			mats[i].SetFloat("_SineMagnitude", magnitude);
		}
	}
}
