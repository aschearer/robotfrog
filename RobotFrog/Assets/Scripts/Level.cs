using UnityEngine;
using System.Collections.Generic;
using System;

public class Level : MonoBehaviour {
    
    public GameObject TileRock;
    public GameObject TileFloating;
    public GameObject TileBouncing;
    public GameObject Player;


    public List<string> Map;

    private List<Tile> tiles = new List<Tile>();

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

    public void Update()
    {
        // Test code to trigger explosions via mouse
        ////if (Input.GetMouseButtonDown(0))
        ////{
        ////    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ////    RaycastHit hit;
        ////    if (Physics.Raycast(ray, out hit))
        ////    {
        ////        var tile = hit.transform.gameObject.GetComponent<Tile>();
        ////        if (tile != null)
        ////        {
        ////            this.ExplodeAt(tile, 1);
        ////        }
        ////    }
        ////}
    }

    public void ExplodeAt(Tile tile, int radius)
    {
        foreach (var neighbor in this.tiles)
        {
            if (neighbor == null)
            {
                continue;
            }

            var distance = Mathf.Abs(tile.Column - neighbor.Column) + Mathf.Abs(tile.Row - neighbor.Row);
            if (distance <= radius)
            {
                this.StartCoroutine(neighbor.HandleExplode(distance * 0.4f));
            }
        }
    }

    internal Tile GetTileAt(int column, int row)
    {
        foreach (var tile in this.tiles)
        {
            if (tile != null && tile.Column == column && tile.Row == row)
            {
                return tile;
            }
        }

        return null;
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
                    if (Prefab == TileFloating || Prefab == TileRock)
                    {
                        this.tiles.Add(Tile.GetComponent<Tile>());
                    }
                    else
                    {
                        this.tiles.Add(null);
                    }
                }
                else
                {
                    // Create an ordinary ground tile
                    GameObject Tile = Instantiate(TileFloating, Position, Rotation, this.transform);
                    Tile.name = "Tile" + i + "," + j;
                    this.tiles.Add(Tile.GetComponent<Tile>());

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
                    player.name = name;
                    var playerView = player.GetComponent<Player>();
                    playerView.playerId = controllerId;
                    playerView.Level = this;
                }

                if (this.tiles[this.tiles.Count - 1] != null)
                {
                    var tile = this.tiles[this.tiles.Count - 1];
                    tile.Column = i;
                    tile.Row = j;
                }
            }
        }
    }
}
