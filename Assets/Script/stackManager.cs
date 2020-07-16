using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class stackManager : MonoBehaviour {
	public Color32[] GameColors = new Color32[4];
	public Text score;
	public Material stackMat;
	public GameObject endPanel;
	private const float Bounds_size = 3.5f;
	private const float Stack_Moving_Speed = 5.0f;
	private const float Stack_Bounds_Gain = 0.25f;
	private const int Combo_Start_Game = 5;
	private GameObject[] theStack;

	private int stackIndex;
	private int Scorecard=0;//y position
	private int combo = 0;

	private float tileTransition = 0.0f; // To MoveTile
	private float tilespeed = 2.5f;
	private float errorMargin = 0.1f;
	private float secondaryPosition;// To respawn new tile over previous one

	private bool isMovingOnX = true;
	private bool gameOver = false;
	private Vector2 stackBounds = new Vector2 (Bounds_size, Bounds_size);
	private Vector3 desiredPosition;//For stack movement
	private Vector3 lastTilePosition;

	// Use this for initialization
	private void Start () {
		theStack = new GameObject[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			//new tile
			theStack [i] = transform.GetChild (i).gameObject;
			ColorMesh(theStack [i].GetComponent<MeshFilter>().mesh);
		}
		//Index Starting from the lowermost positon
		stackIndex = transform.childCount - 1;
	}

	private void CreateRubble(Vector3 pos,Vector3 scale){
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		go.transform.localPosition=pos;
		go.transform.localScale=scale;
		go.AddComponent<Rigidbody> ();
		go.GetComponent<MeshRenderer> ().material = stackMat;
		ColorMesh (go.GetComponent<MeshFilter> ().mesh);
	}
	// Update is called once per frame
	private void Update () {
		//Onclick
		if (gameOver)
			return;
		if(Input.GetMouseButtonDown(0)){
			if (PlaceTile ()) {
				SpawnTile ();
				Scorecard++;
				score.text = Scorecard.ToString ();
			} else {
				EndGame ();
			}
		}
		MoveTile ();
		//Move the stack
		transform.position = Vector3.Lerp(transform.position,desiredPosition,Stack_Moving_Speed*Time.deltaTime);
	}
	// For moving the tiles in a specific direction (X - Y alternately)
	private void MoveTile(){
		tileTransition += Time.deltaTime * tilespeed;
		//Position Movement
		if (isMovingOnX) {
			theStack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin (tileTransition) * Bounds_size, Scorecard, secondaryPosition);
		}
		else{
			theStack [stackIndex].transform.localPosition = new Vector3 (secondaryPosition, Scorecard,Mathf.Sin (tileTransition) * Bounds_size);
		}
	}
	// Create new title 
	private void SpawnTile(){
		lastTilePosition = theStack [stackIndex].transform.localPosition;
		stackIndex--;
		if (stackIndex < 0)
			stackIndex = transform.childCount - 1;
		desiredPosition = (Vector3.down) * Scorecard;
		theStack [stackIndex].transform.localPosition = new Vector3 (0, Scorecard, 0);
		theStack [stackIndex].transform.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
		ColorMesh(theStack [stackIndex].GetComponent<MeshFilter> ().mesh);
	}
	// Placement of tile over the stack with some error corrections
	private bool PlaceTile(){
		Transform t = theStack [stackIndex].transform;
		if (isMovingOnX) {
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs (deltaX) > errorMargin) {
				//Cut the tile
				combo = 0;//reset
				stackBounds.x -= Mathf.Abs (deltaX);
				if (stackBounds.x <= 0)
					return false;
				float middle = lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
				CreateRubble (
					new Vector3((t.position.x > 0)
						?t.position.x+(t.localScale.x/2)
						:t.position.x-(t.localScale.x/2)
						,t.position.y
						,t.position.z),
				new Vector3(Mathf.Abs(deltaX),1,t.localScale.z)
				);

				t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), Scorecard, lastTilePosition.z);
			} else {
				if (combo > Combo_Start_Game){
					if (stackBounds.y > Bounds_size)
						stackBounds.y = Bounds_size;
					stackBounds.x += Stack_Bounds_Gain;
					float middle = lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), Scorecard, lastTilePosition.z);
				}
				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, Scorecard, lastTilePosition.z);
			}
		} else {
			float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs (deltaZ) > errorMargin) {
				//Cut the tile
				combo = 0;//reset
				stackBounds.y -= Mathf.Abs (deltaZ);
				if (stackBounds.y <= 0)
					return false;
				float middle = lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
				CreateRubble (
					new Vector3(t.position.x
						,t.position.y,
						(t.position.z > 0)
						?t.position.z+(t.localScale.z/2)
						:t.position.z-(t.localScale.z/2)
					),
					new Vector3(t.localScale.x,1,Mathf.Abs(deltaZ))
				);
				t.localPosition = new Vector3 (lastTilePosition.x, Scorecard, middle - (lastTilePosition.z / 2));
			}
			else {
				if (combo > Combo_Start_Game){
					if (stackBounds.y > Bounds_size)
						stackBounds.y = Bounds_size;
					stackBounds.y += Stack_Bounds_Gain;
					float middle = lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);

					t.localPosition = new Vector3 (lastTilePosition.x, Scorecard, middle - (lastTilePosition.z / 2));
				}
				combo++;
				t.localPosition = lastTilePosition + Vector3.up;
			}
		}

		secondaryPosition = (isMovingOnX)
			?t.localPosition.x
			:t.localPosition.z;
		isMovingOnX = !isMovingOnX;
		return true; //true tells we managed to place the tile
	}

	//Randomly changing the color of tiles with some gradients
	private void ColorMesh(Mesh mesh){
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		float f = Mathf.Sin (Scorecard * 0.25f);

		for (int i = 0; i < vertices.Length; i++)
			colors [i] = Lerp4 (GameColors [0], GameColors [1], GameColors [2], GameColors [3], f);
		mesh.colors32 = colors;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
	}
	private Color32 Lerp4(Color32 a,Color32 b,Color32 c,Color32 d,float t)
	{
		if (t < 0.33f)
			return Color.Lerp (a, b, t / 0.33f);
		else if (t < 0.66f)
			return Color.Lerp (b, c, (t - 0.33f) / 0.33f);
		else 
			return Color.Lerp (b, c, (t - 0.66f) / 0.66f);
	}
	// When player loses the game
	private void EndGame(){
		if (PlayerPrefs.GetInt ("HighScore") < Scorecard) {
			PlayerPrefs.SetInt ("HighScore", Scorecard);
		}
		gameOver = true;
		endPanel.SetActive (true);
		theStack [stackIndex].AddComponent<Rigidbody> ();
	}
	public void OnButtonClick(string sceneName)
	{
		SceneManager.LoadScene (sceneName);
	}
		
}
