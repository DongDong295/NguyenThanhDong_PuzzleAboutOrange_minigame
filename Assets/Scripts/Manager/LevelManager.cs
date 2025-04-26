using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZBase.Foundation.Singletons;
using DG.Tweening;
using System.Threading;
public class LevelManager : MonoBehaviour, IManager
{
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private List<Puzzle> _puzzlePieces;

    [SerializeField] private List<Node> _levelNodes;
    private float _stageTime;
    private float _oneStar, _twoStar, _threeStar;

    [SerializeField] private float _inputDelay;
    private bool _canMove = true;
    private CancellationTokenSource _cts;

    private GameObject _board;

    public async UniTask OnApplicationStart()
    {
        await UniTask.CompletedTask;
    }

    public async UniTask OnEnterLevel()
    {
        await UniTask.CompletedTask;
    }

    public void Start()
    {
        CreateBoard().Forget();
    }

    void Update()
    {
        if (_canMove)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(Vector2.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2.right);
            if (Input.GetKeyDown(KeyCode.UpArrow)) Move(Vector2.up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Move(Vector2.down);
        }
    }

    private async UniTask CreateBoard()
    {
        _levelNodes = new List<Node>();
        _board = new GameObject("board-holder");
        Vector2 offset = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector2 spawnPosition = new Vector2(x, y) - offset;
                var node = await LoadLevelObject("node-prefab");
                node.transform.position = spawnPosition;
                node.transform.SetParent(_board.transform);
                _levelNodes.Add(node.GetComponent<Node>());
            }
        }

        await LoadLevelBackground();
        await LoadBoardVisual(offset);
        await PlacePuzzle();
    }

    private void Move(Vector2 moveDir)
    {
        _canMove = false;
        InputCooldown().Forget();
        var objects = _puzzlePieces.OrderBy(b => b.Position.x).ThenBy(b => b.Position.y).ToList();
        if (moveDir == Vector2.right || moveDir == Vector2.up) objects.Reverse();

        foreach (var o in objects)
        {
            var next = o.Node;
            do
            {
                o.SetBlock(next);
                var possibleNode = GetNodePosition(next.Position + moveDir);
                if (possibleNode != null)
                {
                    if (possibleNode.OccupiedObject == null)
                        next = possibleNode;
                }
            } while (next != o.Node);
            o.transform.DOMove(o.Node.transform.position, 0.3f);
        }
    }

    private async UniTask InputCooldown()
    {
        await UniTask.WaitForSeconds(_inputDelay);
        _canMove = true;
    }

    private Node GetNodePosition(Vector2 pos)
    {
        return _levelNodes.FirstOrDefault(n => n.Position == pos);
    }

    private async UniTask LoadBoardVisual(Vector2 offset)
    {
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        //var board = Instantiate(_boardPrefab, center - offset, Quaternion.identity);
        var board = await LoadLevelObject("board-prefab");
        board.GetComponent<SpriteRenderer>().size = new Vector2(_width + 0.5f, _height + 0.85f);
        board.transform.SetParent(_board.transform);
    }

    private async UniTask PlacePuzzle()
    {
        _puzzlePieces = new List<Puzzle>();
        for (int i = 0; i < 4; i++)
        {
            var freeNode = _levelNodes.Where(n => n.OccupiedObject == null).ToList();
            var piece = await LoadLevelObject($"puzzle-piece-{i}-prefab");
            var targetNode = freeNode[Random.Range(0, freeNode.Count)];

            piece.transform.position = targetNode.Position;

            var puzzlePiece = piece.GetComponent<Puzzle>();
            targetNode.OccupiedObject = puzzlePiece;
            _puzzlePieces.Add(piece.GetComponent<Puzzle>());
            puzzlePiece.Node = targetNode;
            piece.transform.SetParent(_board.transform);
        }
    }

    private async UniTask LoadLevelBackground()
    {
        var background = await LoadLevelObject("level-background-prefab");
        var canvas = background.GetComponentInChildren<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        background.transform.SetParent(_board.transform);
    }

    private async UniTask<GameObject> LoadLevelObject(string objectToLoad)
    {
        var gameObject = await SingleBehaviour.Of<PoolingManager>().Rent(objectToLoad);
        return gameObject;
    }

    private void UnloadLevelObjects()
    {

    }
}
