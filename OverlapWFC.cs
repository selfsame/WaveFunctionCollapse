using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
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
	private Transform group;

	public static bool IsPrefabRef(UnityEngine.Object o){
		#if UNITY_EDITOR
		return PrefabUtility.GetPrefabParent(o) == null && PrefabUtility.GetPrefabObject(o) != null;
		#endif
		return true;
	}

	static GameObject CreatePrefab(UnityEngine.Object fab, Vector3 pos, Quaternion rot) {
		#if UNITY_EDITOR
		GameObject e = PrefabUtility.InstantiatePrefab(fab as GameObject) as GameObject; 
		e.transform.position = pos;
		e.transform.rotation = rot;
		return e;
		#endif
		GameObject o = GameObject.Instantiate(fab as GameObject) as GameObject; 
		o.transform.position = pos;
		o.transform.rotation = rot;
		return o;
	}


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
		if (IsPrefabRef(training.gameObject)){

			GameObject o = CreatePrefab(training.gameObject, new Vector3(0,0,99999f), Quaternion.identity);
			training = o.GetComponent<Training>();
		}
		if (training.sample == null){
			training.Compile();
		}

		if (group != null){
			if (Application.isPlaying){DestroyImmediate(group.gameObject);} else {
				DestroyImmediate(group.gameObject);
			}	
		}


		if (output == null){
			Transform ot = transform.Find("output-overlap");
			if (ot != null){output = ot.gameObject;}}
		if (output == null){
			output = new GameObject("output-overlap");
			output.transform.parent = transform;
			output.transform.position = this.gameObject.transform.position;
			output.transform.rotation = this.gameObject.transform.rotation;}
		group = output.transform.Find(training.gameObject.name);
		if (group == null){
			group = new GameObject(training.gameObject.name).transform;
			group.parent = output.transform;
			group.position = output.transform.position;
			group.rotation = output.transform.rotation;}		



		rendering = new GameObject[width, depth];
		model = new OverlappingModel(training.sample, N, width, depth, periodicInput, periodicOutput, symmetry, foundation);
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.cyan;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(new Vector3(width*gridsize/2f-gridsize*0.5f, 0, depth*gridsize/2f-gridsize*0.5f),new Vector3(width*gridsize, gridsize, depth*gridsize));
		if (incremental) {
			if (model != null){
				model.Run(1, 5);
				Draw();
			}
		}
	}
	public void Run(){
		if (model == null){return;}
		if (model.Run(seed, iterations)){
			Draw();
		}
	}

	public void OnGUI(){
		Run();
	}

	public void Draw(){
		if (output == null){return;}
		if (group == null){return;}
		for (int y = 0; y < depth; y++){
			for (int x = 0; x < width; x++){
				if (rendering[x,y] == null){
					int v = (int)model.Sample(x, y);

					if (v != 99 && v < training.tiles.Length){
						Vector3 pos = new Vector3(x*gridsize, 0, y*gridsize);
						int rot = (int)training.RS[v];
						GameObject fab = training.tiles[v] as GameObject;
						if (fab != null){
							GameObject tile = (GameObject)Instantiate(fab, new Vector3() , Quaternion.identity);
							Vector3 fscale = tile.transform.localScale;
							tile.transform.parent = group;
							tile.transform.localPosition = pos;
							tile.transform.localEulerAngles = new Vector3(0, rot*90, 0);
							tile.transform.localScale = fscale;
							rendering[x,y] = tile;
						}
					}


				}
			}
  		}	
	}
}

 #if UNITY_EDITOR
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
#endif