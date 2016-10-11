using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class OverlapWFC : MonoBehaviour{
	public Training training = null;

	public int gridsize = 2;
	public int width = 20;
	public int depth = 20;
	public int seed = 0;
	public int N = 2;
	public bool periodicInput = false;
	public bool periodicOutput = false;
	public int symmetry = 1;
	public int foundation = 0;
	public int iterations = 0;
	public bool incremental = false;

	public OverlappingModel model = null;

	public GameObject[,] rendering;
	public GameObject output;

	void Start(){
		Generate();
		Run();
	}

	void Update(){
		if (incremental){
			Run();
		}
	}

	public void Generate() {
		if (training == null){Debug.Log("Can't Generate: no designated Training component");}
		if (training.sample == null){
			training.Compile();
		}
		DestroyImmediate(output);
		output = new GameObject("output-OverlapModel");
		rendering = new GameObject[width, depth];
		model = new OverlappingModel(training.sample, N, width, depth, periodicInput, periodicOutput, symmetry, foundation);
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(transform.position + new Vector3(width*gridsize/2, 0, depth*gridsize/2),new Vector3(width*gridsize, gridsize, depth*gridsize));
		if (incremental) {
			if (model != null){
				model.Run(1, 5);
				Draw();
			}
		}
	}
	public void Run(){
		if (model.Run(seed, iterations)){
			Draw();
		}
	}

	public void Draw(){
		if (output == null){return;}
		for (int y = 0; y < depth; y++){
			for (int x = 0; x < width; x++){
				if (rendering[x,y] == null){
					int v = (int)model.Sample(x, y);

					if (v != 99 && v < training.tiles.Length){
						Vector3 pos = new Vector3(x*gridsize, 0, y*gridsize);
						int rot = (int)training.RS[v];
						GameObject fab = training.tiles[v] as GameObject;
						if (fab != null){
							GameObject tile = (GameObject)Instantiate(fab, pos +this.gameObject.transform.position , Quaternion.identity);
							tile.transform.parent = output.transform;
							tile.transform.eulerAngles = new Vector3(0, rot*90, 0);
							rendering[x,y] = tile;
						}
					}


				}
			}
  		}	
	}
}

[CustomEditor (typeof(OverlapWFC))]
public class WFCGeneratorEditor : Editor {
	public override void OnInspectorGUI () {
		OverlapWFC generator = (OverlapWFC)target;
		if (generator.training != null){
			if(GUILayout.Button("generate")){
				generator.Generate();
			}
			if (generator.model != null){
				if(GUILayout.Button("RUN")){
					generator.Run();
				}
			}
		}
		DrawDefaultInspector ();
	}
}