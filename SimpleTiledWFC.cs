using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class SimpleTiledWFC : MonoBehaviour{
	
	public string xmlpath = null;
	public string subset = null;

	public int gridsize = 2;
	public int width = 20;
	public int depth = 20;

	public int seed = 0;
	public bool periodic = false;
	public int iterations = 0;
	public bool incremental;

	public SimpleTiledModel model = null;
	public GameObject[,] rendering;
	public GameObject output;
	public Dictionary<string, GameObject> obmap = new Dictionary<string, GameObject>();

	public void destroyChildren (){
		foreach (Transform child in this.transform) {
     		GameObject.DestroyImmediate(child.gameObject);
 		}
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


	public void Run(){
		if (model.Run(seed, iterations)){
			Draw();
		}
	}

	public void OnDrawGizmos(){
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(transform.position + new Vector3(width*gridsize/2, 0, depth*gridsize/2),new Vector3(width*gridsize, gridsize, depth*gridsize));
		if (incremental) {
			if (model != null){
				model.Run(1, 5);
				Draw();
			} 
		}
	}

	public void Generate(){
		obmap = new  Dictionary<string, GameObject>();
		DestroyImmediate(output);
		output = new GameObject("output-SimpleTiledModel");
		rendering = new GameObject[width, depth];
		this.model = new SimpleTiledModel(xmlpath, subset, width, depth, periodic);
	}

	public void Draw(){
		if (output == null){return;}
		for (int y = 0; y < depth; y++){
			for (int x = 0; x < width; x++){ 
				if (rendering[x,y] == null){
					string v = model.Sample(x, y);
					int rot = 0;
					
					GameObject fab = null;
					if (v != "?"){
						rot = int.Parse(v.Substring(0,1));
						v = v.Substring(1);
						if (!obmap.ContainsKey(v)){
							fab = (GameObject)Resources.Load("tiles/"+v, typeof(GameObject));
							if (fab == null) {
								fab = (GameObject)Resources.Load("tiles/_", typeof(GameObject));}
							obmap[v] = fab;
						} else {
							fab = obmap[v];
						}
						
						Vector3 pos = new Vector3(x*gridsize, 0, y*gridsize);
						GameObject tile = (GameObject)Instantiate(fab, pos +this.gameObject.transform.position , Quaternion.identity);
						tile.transform.parent = output.transform;
						tile.transform.eulerAngles = new Vector3(0, 360-(rot*-90), 0);
						rendering[x,y] = tile;
					}


				}
			}
  		}	
	}
}

[CustomEditor (typeof(SimpleTiledWFC))]
public class TileSetEditor : Editor {
	public override void OnInspectorGUI () {
		SimpleTiledWFC me = (SimpleTiledWFC)target;

		if (me.xmlpath != null){
			if(GUILayout.Button("generate")){
				me.Generate();
			}
			if (me.model != null){
				if(GUILayout.Button("RUN")){
					me.model.Run(me.seed, me.iterations);
					me.Draw();
				}
			}
		}
		DrawDefaultInspector ();
	}
}