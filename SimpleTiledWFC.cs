using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SimpleTiledWFC : MonoBehaviour{
	
	public string xmlpath = null;
	private string subset = "";

	public int gridsize = 1;
	public int width = 20;
	public int depth = 20;

	public int seed = 0;
	public bool periodic = false;
	public int iterations = 0;
	public bool incremental;

	public SimpleTiledModel model = null;
	public GameObject[,] rendering;
	public GameObject output;
	private Transform group;
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
		if (model == null){return;}
		if (model.Run(seed, iterations)){
			Draw();
		}
	}

	public void OnDrawGizmos(){
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(new Vector3(width*gridsize/2f-gridsize*0.5f, 0, depth*gridsize/2f-gridsize*0.5f),new Vector3(width*gridsize, gridsize, depth*gridsize));
		if (incremental) {
			if (model != null){
				model.Run(1, 5);
				Draw();
			} 
		}
	}

	public void Generate(){
		obmap = new  Dictionary<string, GameObject>();

		if (group != null){
			if (Application.isPlaying){DestroyImmediate(group.gameObject);} else {
				DestroyImmediate(group.gameObject);
			}	
		}	

		if (output == null){
			Transform ot = transform.Find("output-tiled");
			if (ot != null){output = ot.gameObject;}}
		if (output == null){
			output = new GameObject("output-tiled");
			output.transform.parent = transform;
			output.transform.position = this.gameObject.transform.position;
			output.transform.rotation = this.gameObject.transform.rotation;}
		group = output.transform.Find(xmlpath);

		if (group == null){
			group = new GameObject(xmlpath).transform;
			group.parent = output.transform;
			group.position = output.transform.position;
			group.rotation = output.transform.rotation;}	

		rendering = new GameObject[width, depth];
		this.model = new SimpleTiledModel(Application.dataPath+"/"+xmlpath, subset, width, depth, periodic);
	}

	public void Draw(){
		if (output == null){return;}
		if (group == null){return;}
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
							fab = (GameObject)Resources.Load(v, typeof(GameObject));
							obmap[v] = fab;
						} else {
							fab = obmap[v];
						}
						if (fab == null){
							continue;}
						Vector3 pos = new Vector3(x*gridsize, 0, y*gridsize);
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

#if UNITY_EDITOR
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
#endif