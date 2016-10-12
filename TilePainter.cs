using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[RequireComponent(typeof(BoxCollider))]
public class TilePainter : MonoBehaviour{

	public int gridsize = 2;
	public int width = 20;
	public int height = 20;
	public GameObject tiles;
	private bool _changed = true;
	public Vector3 cursor;
	public bool focused = false;
	public GameObject[,] tileobs;
	

	int colidx = 0; 
	public UnityEngine.Object[] palette = new UnityEngine.Object[0];
	public UnityEngine.Object color = null;
	Quaternion color_rotation;

	


	public void Encode(){

	} 

	static GameObject CreatePrefab(UnityEngine.Object fab, Vector3 pos, Quaternion rot) {	
		GameObject o = PrefabUtility.InstantiatePrefab(fab as GameObject) as GameObject; 
		o.transform.position = pos;
		o.transform.rotation = rot;
		return o;
	}

	public void Restore(){


		GameObject.DestroyImmediate(GameObject.Find("palette"));
		GameObject pal = new GameObject("palette");
		pal.hideFlags = HideFlags.HideInHierarchy;
		BoxCollider bc = pal.AddComponent<BoxCollider>();
		bc.size = new Vector3(palette.Length*gridsize, 0f, gridsize);
		bc.center = new Vector3((palette.Length-1f)*gridsize*0.5f, 0f, 0f);

		pal.transform.parent = this.gameObject.transform;
		pal.transform.localPosition = new Vector3(0f,0f, -gridsize*2);
		for (int i = 0; i < palette.Length; i++){
			UnityEngine.Object o = palette[i];
			if (o != null){
				GameObject g = CreatePrefab(o, new Vector3() , Quaternion.identity);
				g.transform.parent = pal.transform;
				g.transform.localPosition = new Vector3(i*gridsize, 0f, 0f);
			}
		}

		tileobs = new GameObject[width, height];
		if (tiles == null){
			tiles = new GameObject("tiles");
			tiles.transform.parent = this.gameObject.transform;
		}
		int cnt = tiles.transform.childCount;
		List<GameObject> trash = new List<GameObject>();
		for (int i = 0; i < cnt; i++){
			GameObject tile = tiles.transform.GetChild(i).gameObject;
			Vector3 tilepos = tile.transform.position;
			int X = (int)(tilepos.x / gridsize);
			int Y = (int)(tilepos.z / gridsize);
			if (ValidCoords(X, Y)){
			tileobs[X, Y] = tile; 
			} else {
				trash.Add(tile);
			}
		}
		for (int i = 0; i < trash.Count; i++){
			if (Application.isPlaying){Destroy(trash[i]);} else {DestroyImmediate(trash[i]);}}	

		if (color == null){
			if (palette.Length > 0){
				color = palette[0];
			}
		}
	}

	public void Resize(){
		if (_changed){
			_changed = false;
			Restore(); 
		}
	}

	public void Awake(){
		Restore();
	}

	public void OnEnable(){
		Restore();
	}

	void OnValidate(){
		_changed = true;
		BoxCollider bounds = this.GetComponent<BoxCollider>();
		bounds.center = new Vector3((width*gridsize)*0.5f-gridsize*0.5f, 0f, (height*gridsize)*0.5f-gridsize*0.5f);
		bounds.size = new Vector3(width*gridsize, 0f, (height*gridsize));
	}

	public Vector3 GridV3(Vector3 pos){
		Vector3 p = pos - this.gameObject.transform.position + new Vector3(gridsize*0.5f,0f,gridsize*0.5f);
		return new Vector3((int)(p.x/gridsize), 0, (int)(p.z/gridsize));
	}

	public bool ValidCoords(int x, int y){
		if (tileobs == null) {return false;}
		
		return (x >= 0 && y >= 0 && x < tileobs.GetLength(0) && y < tileobs.GetLength(1));
	}


	public void CycleColor(){
		colidx += 1;
		if (colidx >= palette.Length){
			colidx = 0;
		}
		color = (UnityEngine.Object)palette[colidx];
	}

	public void Turn(){
		if (this.ValidCoords((int)cursor.x, (int)cursor.z)){
			GameObject o = tileobs[(int)cursor.x, (int)cursor.z];
			if (o != null){
				o.transform.Rotate(0f, 90f, 0f);
			}
		}
	}

	public Vector3 Local(Vector3 p){
		return this.transform.TransformPoint(p);
	}

	public void Drag(Vector3 mouse, TileLayerEditor.TileOperation op){
		Resize();
		if (tileobs == null){Restore();}
		if (this.ValidCoords((int)cursor.x, (int)cursor.z)){
			if (op == TileLayerEditor.TileOperation.Sampling){
				UnityEngine.Object s = PrefabUtility.GetPrefabParent(tileobs[(int)cursor.x, (int)cursor.z]);
				if (s != null){
					color = s;
					color_rotation = tileobs[(int)cursor.x, (int)cursor.z].transform.rotation;
				}
			} else {
				DestroyImmediate(tileobs[(int)cursor.x, (int)cursor.z]); 
				if (op == TileLayerEditor.TileOperation.Drawing){
					if (color == null){return;}
					GameObject o = CreatePrefab(color, new Vector3() , color_rotation);
					o.transform.parent = tiles.transform;
					o.transform.position = (cursor*gridsize)+this.gameObject.transform.position;
					tileobs[(int)cursor.x, (int)cursor.z] = o;
				}
			}
		} else {
			if (op == TileLayerEditor.TileOperation.Sampling){
				if (cursor.z == -1 && cursor.x >= 0 && cursor.x < palette.Length){
					color = palette[(int)cursor.x];
					color_rotation = Quaternion.identity;
				}
			}
		}
	}

	public void Clear(){
		tileobs = new GameObject[width, height];
		DestroyImmediate(tiles);
		tiles = new GameObject("tiles");
		tiles.transform.parent = gameObject.transform;
	}

	public void OnDrawGizmos(){
		Gizmos.color = Color.white;

		if (focused){
			Gizmos.color = new Color(1f,0f,0f,0.6f);
			Gizmos.DrawRay(Local(cursor*gridsize)+Vector3.forward*-49999f, Vector3.forward*99999f);
			Gizmos.DrawRay(Local(cursor*gridsize)+Vector3.right*-49999f, Vector3.right*99999f);
			Gizmos.DrawRay(Local(cursor*gridsize)+Vector3.up*-49999f, Vector3.up*99999f);
			Gizmos.color = Color.yellow;
		}

		Gizmos.DrawWireCube(transform.position + new Vector3((width*gridsize)*0.5f-gridsize*0.5f, 0f, (height*gridsize)*0.5f-gridsize*0.5f),
			new Vector3(width*gridsize, 0f, (height*gridsize)));
	}
}
 

 [CustomEditor(typeof(TilePainter))]
 public class TileLayerEditor : Editor{
 	public enum TileOperation {None, Drawing, Erasing, Sampling};
 	private TileOperation operation;

	public override void OnInspectorGUI () {
		TilePainter me = (TilePainter)target;
		GUILayout.Label("Assign a prefab to the color property"); 
		GUILayout.Label("or the pallete array.");
		GUILayout.Label("drag        : paint tiles");
		GUILayout.Label("[s]+click  : sample tile color");
		GUILayout.Label("[x]+drag  : erase tiles");
		GUILayout.Label("[space]    : rotate tile");
		GUILayout.Label("[b]          : cycle color");
		if(GUILayout.Button("CLEAR")){
			me.Clear();}
		DrawDefaultInspector();}

	private bool AmHovering(Event e){
		TilePainter me = (TilePainter)target;
		RaycastHit hit;
		if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit, Mathf.Infinity) && 
			 	hit.collider.GetComponentInParent<TilePainter>() == me)
		{
			me.cursor = me.GridV3(hit.point);
			me.focused = true;

			Renderer rend = me.gameObject.GetComponentInChildren<Renderer>( );
			if( rend ) EditorUtility.SetSelectedWireframeHidden( rend, false );
			return true;
		}
		me.focused = false;
		return false;
	}

	public void ProcessEvents(){
		TilePainter me = (TilePainter)target;
        int controlID = GUIUtility.GetControlID(1778, FocusType.Passive);
        EditorWindow currentWindow = EditorWindow.mouseOverWindow;
        if(currentWindow && AmHovering(Event.current)){
            Event current = Event.current;
 			bool leftbutton = (current.button == 0);
            switch(current.type){
                case EventType.keyDown:

                	if (current.keyCode == KeyCode.S) operation = TileOperation.Sampling;
                	if (current.keyCode == KeyCode.X) operation = TileOperation.Erasing;
                    current.Use();
                    return;
                case EventType.keyUp:
                	operation = TileOperation.None;
                	if (current.keyCode == KeyCode.Space) me.Turn();
                	if (current.keyCode == KeyCode.B) me.CycleColor();
                    current.Use();
                    return;
                case EventType.mouseDown:
                    if (leftbutton)
                    {
                    	if (operation == TileOperation.None){
                    		operation = TileOperation.Drawing;
                    	}
                    	me.Drag(current.mousePosition, operation);

                        current.Use();
                        return;
                    }
                    break;
                case EventType.mouseDrag:
                    if (leftbutton)
                    {
                        if (operation != TileOperation.None){
                        	me.Drag(current.mousePosition, operation);
                        	current.Use();
                        }
                        
                        return;
                    }
                    break;
                case EventType.mouseUp:
                    if (leftbutton)
                    {
                    	operation = TileOperation.None;
                        current.Use();
                        return;
                    }
                break;
                case EventType.mouseMove:
                	me.Resize();
                	current.Use();
                break;
                case EventType.repaint:
                break;
                case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            }
        }
    }
	
	void OnSceneGUI (){
	 	ProcessEvents();
	}

	 void DrawEvents(){
	     Handles.BeginGUI();
	     Handles.EndGUI();
	 }}



