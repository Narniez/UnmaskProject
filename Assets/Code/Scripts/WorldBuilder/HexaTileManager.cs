using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum BiomeType { None, Forest, Cave, Glacier, Oasis }

public class HexaTileManager : MonoBehaviour
{

    [SerializeField] private HexGrid grid;
    public GameObject biomeMenu;
    private HexTile clickedTile;

    private Button forestButton;
    private Button caveButton;
    private Button glacierButton;
    private Button oasisButton;
    private Button defaultButton;

    HexTilePathfinder pathfinder;


    public GameObject puzzleCorrectPannel;
    public GameObject puzzleWrongPannel;

    private void Start()
    {
        InitializeBiomeMenu();
        pathfinder = GetComponent<HexTilePathfinder>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);


            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                HexTile _clickedTile = hit.collider.GetComponent<HexTile>();
                if (_clickedTile != null)
                {
                    clickedTile = _clickedTile;
                    OnTileClicked(clickedTile);
                }
            }
        }
    }


    private void InitializeBiomeMenu()
    {
        Button[] buttons = biomeMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            switch (button.name)
            {
                case "ForestButton":
                    forestButton = button;
                    break;
                case "CaveButton":
                    caveButton = button;
                    break;
                case "GlacierButton":
                    glacierButton = button;
                    break;
                case "OasisButton":
                    oasisButton = button;
                    break;
                case "DefaultButton":
                    defaultButton = button;
                    break;
            }
        }

        // Set up button click handlers
        forestButton.onClick.AddListener(() => OnBiomeButtonClicked(BiomeType.Forest));
        caveButton.onClick.AddListener(() => OnBiomeButtonClicked(BiomeType.Cave));
        glacierButton.onClick.AddListener(() => OnBiomeButtonClicked(BiomeType.Glacier));
        oasisButton.onClick.AddListener(() => OnBiomeButtonClicked(BiomeType.Oasis));
        defaultButton.onClick.AddListener(OriginalBiome);

        biomeMenu.SetActive(false);
    }

    private void OnBiomeButtonClicked(BiomeType type)
    {
        if (clickedTile != null)
        {
            clickedTile.SetBiome(type);
            biomeMenu.SetActive(false);
        }
    }

    private void UpdateMenuButtons(TileType tileType)
    {
        // Enable/disable buttons based on the clicked tile's type
        switch (tileType)
        {
            case TileType.Default:
                forestButton.gameObject.SetActive(true);
                caveButton.gameObject.SetActive(false);
                glacierButton.gameObject.SetActive(false);
                oasisButton.gameObject.SetActive(false);
                break;

            case TileType.Water:
                forestButton.gameObject.SetActive(false);
                caveButton.gameObject.SetActive(false);
                glacierButton.gameObject.SetActive(true);
                oasisButton.gameObject.SetActive(false);
                break;

            case TileType.Desert:
                forestButton.gameObject.SetActive(false);
                caveButton.gameObject.SetActive(true);
                glacierButton.gameObject.SetActive(false);
                oasisButton.gameObject.SetActive(true);
                break;

            case TileType.Mountain:
                forestButton.gameObject.SetActive(false);
                caveButton.gameObject.SetActive(true);
                glacierButton.gameObject.SetActive(false);
                oasisButton.gameObject.SetActive(false);
                break;

                // Add more cases for other tile types
        }

        // The default button is always visible
        defaultButton.gameObject.SetActive(true);
    }

    private void OriginalBiome()
    {
        clickedTile.SetOriginalBiome();
        biomeMenu.SetActive(false);
    }

    public void OnTileClicked(HexTile tile)
    {
        clickedTile = tile;
        UpdateMenuButtons(tile.GetTileType());
        biomeMenu.SetActive(true);
    }

    public void CheckPuzzle()
    {
        if (pathfinder.IsPathConnected())
        {
            StartCoroutine(ShowPannel(5f, puzzleCorrectPannel));
        }
        else
        {
            StartCoroutine(ShowPannel(2f, puzzleWrongPannel));
        }
    }

    private IEnumerator ShowPannel(float waitTime, GameObject pannel)
    {
        pannel.SetActive(true);

        yield return new WaitForSeconds(waitTime);

        pannel.SetActive(false);
    }

}
