using UnityEngine;
using System.Collections.Generic;


public class Level : MonoBehaviour {
    
    public GameObject TileRock;
    public GameObject TileFloating;
    public GameObject TileBouncing;
    public GameObject Player;


    public List<string> Map;

    public void Start()
    {
        if(Map.Count == 0)
        {
            Map.Add("XXXXXXXXXXXXXXXXXX");
            Map.Add("X________________X");
            Map.Add("X___1________X_X_X");
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
            Map.Add("X___________2X_X_X");
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
                Vector3 Position = this.transform.localPosition + new Vector3(i*TileRadius, 0, j*TileRadius);
                Quaternion Rotation = Quaternion.identity;
                GameObject Prefab = null;
                string name = string.Empty;
                switch(row[j])
                {
                    default: break;
                    case '_': Prefab = TileFloating; name = "Floating"; break;
                    case 'X': Prefab = TileRock; name = "Rock"; break;
                    case 'B': Prefab = TileBouncing; name = "Bouncing"; break;
                }

                if (Prefab != null)
                {
                    GameObject Tile = Instantiate(Prefab, Position, Rotation, this.transform);
                    Tile.name = name + i + "," + j;
                }
                else
                {
                    // Create an ordinary ground tile
                    GameObject Tile = Instantiate(TileFloating, Position, Rotation, this.transform);
                    Tile.name = "Tile" + i + "," + j;

                    Prefab = Player;
                    ControllerId controllerId = ControllerId.KeyboardLeft;
                    switch (row[j])
                    {
                        case '1':
                            name = "Player1";
                            break;
                        case '2':
                            name = "Player2";
                            controllerId = ControllerId.KeyboardRight;
                            break;
                    }

                    Position.y += 1;
                    GameObject player = Instantiate(Prefab, Position, Rotation, this.transform);
                    player.GetComponent<Player>().playerId = controllerId;
                    player.name = name;
                }
            }
        }
    }
}
