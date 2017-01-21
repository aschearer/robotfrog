using UnityEngine;
using System.Collections.Generic;


public class Level : MonoBehaviour {
    
	public GameObject TileRock;
	public GameObject TileFloating;
	public GameObject TileBouncing;


	public List<string> Map;

    public void Start()
    {
    	if(Map.Count == 0)
    	{
	    	Map.Add("XXXXXXXXXXXXXXXXXX");
	    	Map.Add("X________________X");
	    	Map.Add("X____________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("X___B________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("X____________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("X____________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("X____________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("X____________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("X____________X_X_X");
	    	Map.Add("X________________X");
	    	Map.Add("XXXXXXXXXXXXXXXXXX");
	    }
    	MakeLevel(Map);
    }

    public void MakeLevel(List<string> Map)
    {
    	int TileRadius = 1;
    	for(int i=0; i<Map.Count; ++i)
    	{
    		string row = Map[i];
    		for(int j=0; j<row.Length; ++j)
    		{
    			Vector3 Position = new Vector3(i*TileRadius, 0, j*TileRadius);
    			Quaternion Rotation = Quaternion.identity;
    			GameObject Prefab;
    			switch(row[j])
    			{
    				default:
    				case '_': Prefab = TileFloating; break;
    				case 'X': Prefab = TileRock; break;
    				case 'B': Prefab = TileBouncing; break;
    			}
    			GameObject Tile = Instantiate(Prefab, Position, Rotation, this.transform);
    			Tile.name = "Tile"+i+","+j;

    		}
    	}
    }
}
