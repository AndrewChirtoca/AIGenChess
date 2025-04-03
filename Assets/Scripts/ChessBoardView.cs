using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIGenChess
{
    public class ChessBoardView : MonoBehaviour
    {
        public ChessManager chessManager;
        public GridLayoutGroup gridLayout;
        public GameObject cellPrefab;
        public ChessSquareView[,] boardSquareViews;

        public Sprite kingSprite;
        public Sprite queenSprite;
        public Sprite rookSprite;
        public Sprite bishopSprite;
        public Sprite knightSprite;
        public Sprite cloneSprite;
        public Sprite pawnSprite;

        private Sprite GetPieceSprite(PieceType type)
        {
            return type switch
            {
                PieceType.King => kingSprite,
                PieceType.Queen => queenSprite,
                PieceType.Rook => rookSprite,
                PieceType.Bishop => bishopSprite,
                PieceType.Knight => knightSprite,
                PieceType.Clone => cloneSprite,
                PieceType.Pawn => pawnSprite,
                _ => null,
            };
        }

        private int _rowNumber;
        private int _columnNumber;
        
        private void Start()
        {
            _rowNumber = chessManager.Board.GetLength(0);
            _columnNumber = chessManager.Board.GetLength(1);
            boardSquareViews = new ChessSquareView[_rowNumber, _columnNumber];
            
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = _columnNumber;
            
            for (int y = 0; y < _rowNumber; y++)
            {
                for (int x = 0; x < _columnNumber; x++)
                {
                    GameObject instantiatedCell = Instantiate(cellPrefab, gridLayout.transform);
                    var squareView = instantiatedCell.GetComponent<ChessSquareView>();
                    squareView.squareImage.color = GetSquareColor(y, x);
                    squareView.squareText.color = GetTextColor(y, x);
                    squareView.name = $"x {x} y {y}";
                    squareView.squareText.text = squareView.name;
                    boardSquareViews[y, x] = squareView;
                    int squareRow = y;
                    int squareColumn = x;
                    squareView.squareButton.onClick.AddListener(() => OnClick(squareRow, squareColumn));
                }
            }
        }

        private Piece _selectedPiece;
        private void OnClick(int row, int column)
        {
            if (_selectedPiece == null)
            {
                Piece piece = chessManager.Board[row, column];
                if (piece != null)
                {
                    _selectedPiece = piece;
                }
            }
            else
            {
                chessManager.MovePiece(_selectedPiece, new Vector2Int(row, column));
                _selectedPiece = null;
            }
        }

        private void Update()
        {
            for (int y = 0; y < _rowNumber; y++)
            {
                for (int x = 0; x < _columnNumber; x++)
                {
                    Piece chessPiece = chessManager.Board[y, x];
                    bool isPieceOnSquare = chessPiece != null;
                    boardSquareViews[y, x].pieceImage.enabled = isPieceOnSquare;
                    
                    if (isPieceOnSquare)
                    {
                        boardSquareViews[y, x].pieceImage.sprite = GetPieceSprite(chessPiece.Type);
                        boardSquareViews[y, x].pieceImage.color = chessPiece == _selectedPiece
                            ? GetSelectedTeamColor(chessPiece.Team) : GetDefaultTeamColor(chessPiece.Team);
                    }

                    // boardSquareViews[y, x].squareText.text = chessPiece != null
                    //     ? chessPiece.Type.ToString() + '\n' + chessPiece.Team : boardSquareViews[y, x].name;
                }
            }
        }

        public Color GetSquareColor(int row, int column)
        {
            // Check if the sum of row and column is even or odd
            if ((row + column) % 2 == 0)
            {
                return Color.cyan; // Dark square
            }
            else
            {
                return Color.white; // Light square
            }
        }
        
        public Color GetTextColor(int row, int column)
        {
            // Check if the sum of row and column is even or odd
            if ((row + column) % 2 != 0)
            {
                return Color.black; // Dark square
            }
            else
            {
                return Color.white; // Light square
            }
        }

        public Color GetDefaultTeamColor(Team team)
        {
            if (team == Team.Black)
            {
                return Color.black;
            }
            else
            {
                return Color.white;
            }
        }
        
        public Color GetSelectedTeamColor(Team team)
        {
            if (team == Team.Black)
            {
                return Color.magenta;
            }
            else
            {
                return Color.yellow;
            }
        }
    }
}
