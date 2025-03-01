using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlumaFisica : MonoBehaviour
{
    //public MeshRenderer mesh;
    public List<Material> mats;
    public float lerpSpeed = 5f;
    public float maxClamp = 2f;
    private Vector3 previusPos;
	private Vector3 input;
	void Start()
    {
        //mats.Add(mesh.sharedMaterial);
        previusPos = transform.position;
	}

    void Update()
    {
        input += previusPos - transform.position;
		previusPos = transform.position;
		input = Vector3.Lerp(input, Vector3.zero, Time.deltaTime * lerpSpeed);
        for (int i = 0; i < mats.Count; ++i)
        {
            mats[i].SetVector("_Input", Vector3.ClampMagnitude(input, maxClamp));
		}
    }
}
