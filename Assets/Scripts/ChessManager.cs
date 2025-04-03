using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIGenChess
{
    public enum PieceType
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Clone,
        Pawn
    }
    
    public enum Team
    {
        White,
        Black
    }
    
    public enum GameState
    {
        Ongoing,
        Check,
        Checkmate,
        Stalemate
    }
    
    public class Piece
    {
        public PieceType Type;
        public Team Team;
        public Vector2Int Position;
        public bool HasMoved; // Track if the piece has moved
        
        public Piece(PieceType type, Team team, Vector2Int position)
        {
            Type = type;
            Team = team;
            Position = position;
            HasMoved = false;
        }
    }
    
    public class ChessManager : MonoBehaviour
    {
        private Piece[,] board = new Piece[10, 10];
        private Team currentTurn = Team.White;
        private GameState gameState = GameState.Ongoing;

        public Piece[,] Board => board;
        
        void Awake()
        {
            SetupBoard();
        }
        
        void SetupBoard()
        {
            // Setup white pieces
            for (int i = 0; i < 10; i++)
            {
                if (i < 10) // Each player has 10 pawns
                    board[1, i] = new Piece(PieceType.Pawn, Team.White, new Vector2Int(1, i));
            }
            
            board[0, 0] = new Piece(PieceType.Rook, Team.White, new Vector2Int(0, 0));
            board[0, 1] = new Piece(PieceType.Knight, Team.White, new Vector2Int(0, 1));
            board[0, 2] = new Piece(PieceType.Clone, Team.White, new Vector2Int(0, 2));
            board[0, 3] = new Piece(PieceType.Bishop, Team.White, new Vector2Int(0, 3));
            board[0, 4] = new Piece(PieceType.Queen, Team.White, new Vector2Int(0, 4));
            board[0, 5] = new Piece(PieceType.King, Team.White, new Vector2Int(0, 5));
            board[0, 6] = new Piece(PieceType.Bishop, Team.White, new Vector2Int(0, 6));
            board[0, 7] = new Piece(PieceType.Clone, Team.White, new Vector2Int(0, 7));
            board[0, 8] = new Piece(PieceType.Knight, Team.White, new Vector2Int(0, 8));
            board[0, 9] = new Piece(PieceType.Rook, Team.White, new Vector2Int(0, 9));
            
            // Setup black pieces (mirrored alignment with the swap)
            for (int i = 0; i < 10; i++)
            {
                if (i < 10) // Each player has 10 pawns
                    board[8, i] = new Piece(PieceType.Pawn, Team.Black, new Vector2Int(8, i));
            }
            
            board[9, 0] = new Piece(PieceType.Rook, Team.Black, new Vector2Int(9, 0));
            board[9, 1] = new Piece(PieceType.Knight, Team.Black, new Vector2Int(9, 1));
            board[9, 2] = new Piece(PieceType.Clone, Team.Black, new Vector2Int(9, 2));
            board[9, 3] = new Piece(PieceType.Bishop, Team.Black, new Vector2Int(9, 3));
            board[9, 4] = new Piece(PieceType.Queen, Team.Black, new Vector2Int(9, 4));
            board[9, 5] = new Piece(PieceType.King, Team.Black, new Vector2Int(9, 5));
            board[9, 6] = new Piece(PieceType.Bishop, Team.Black, new Vector2Int(9, 6));
            board[9, 7] = new Piece(PieceType.Clone, Team.Black, new Vector2Int(9, 7));
            board[9, 8] = new Piece(PieceType.Knight, Team.Black, new Vector2Int(9, 8));
            board[9, 9] = new Piece(PieceType.Rook, Team.Black, new Vector2Int(9, 9));
        }
        
        public bool IsMoveValid(Piece piece, Vector2Int targetPosition)
        {
            // Check bounds
            if (targetPosition.x < 0 || targetPosition.x >= 10 || targetPosition.y < 0 || targetPosition.y >= 10)
                return false;
            
            // Check if target position is occupied by the same team
            Piece targetPiece = board[targetPosition.x, targetPosition.y];
            if (targetPiece != null && targetPiece.Team == piece.Team)
                return false;
            
            // Handle castling
            if (piece.Type == PieceType.King && !piece.HasMoved)
            {
                if (IsCastlingMoveValid(piece, targetPosition))
                    return true; // Valid castling move
            }
            
            // Implement piece movement rules
            bool isValidMove = piece.Type switch
            {
                PieceType.Pawn => IsPawnMoveValid(piece, targetPosition),
                PieceType.Rook => IsRookMoveValid(piece, targetPosition),
                PieceType.Knight => IsKnightMoveValid(piece, targetPosition),
                PieceType.Bishop => IsBishopMoveValid(piece, targetPosition),
                PieceType.Queen => IsQueenMoveValid(piece, targetPosition),
                PieceType.King => IsKingMoveValid(piece, targetPosition),
                PieceType.Clone => IsCloneMoveValid(piece, targetPosition),
                _ => false,
            };
            
            if (!isValidMove)
                return false;
            
            // Temporarily simulate the move
            Vector2Int originalPosition = piece.Position;
            Piece tempTargetPiece = board[targetPosition.x, targetPosition.y];
            
            // Move the piece
            board[targetPosition.x, targetPosition.y] = piece;
            board[originalPosition.x, originalPosition.y] = null;
            piece.Position = targetPosition;
            
            // Check if the king is in check after the move
            bool isInCheckAfterMove = IsInCheck(piece.Team);
            
            // Undo the move
            board[originalPosition.x, originalPosition.y] = piece;
            board[targetPosition.x, targetPosition.y] = tempTargetPiece;
            piece.Position = originalPosition;
            
            // If the king is in check after the move, it's not a valid move
            return !isInCheckAfterMove;
        }
        
        private bool IsPawnMoveValid(Piece piece, Vector2Int targetPosition)
        {
            int direction = piece.Team == Team.White ? 1 : -1;
            if (targetPosition.y == piece.Position.y)
            {
                // Move forward
                if (targetPosition.x == piece.Position.x + direction &&
                    board[targetPosition.x, targetPosition.y] == null)
                    return true;
                
                // Initial double move
                if (piece.Position.x == (piece.Team == Team.White ? 1 : 6) &&
                    targetPosition.x == piece.Position.x + 2 * direction &&
                    board[piece.Position.x + direction, piece.Position.y] == null &&
                    board[targetPosition.x, targetPosition.y] == null)
                    return true;
            }
            else if (Mathf.Abs(targetPosition.y - piece.Position.y) == 1)
            {
                // Capture move
                if (targetPosition.x == piece.Position.x + direction &&
                    board[targetPosition.x, targetPosition.y] != null)
                    return true;
            }
            
            return false;
        }
        
        private bool IsRookMoveValid(Piece piece, Vector2Int targetPosition)
        {
            if (piece.Position.x != targetPosition.x && piece.Position.y != targetPosition.y)
                return false;
            
            // Check for clear path
            return IsPathClear(piece.Position, targetPosition);
        }
        
        private bool IsKnightMoveValid(Piece piece, Vector2Int targetPosition)
        {
            int dx = Mathf.Abs(targetPosition.x - piece.Position.x);
            int dy = Mathf.Abs(targetPosition.y - piece.Position.y);
            return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
        }
        
        private bool IsBishopMoveValid(Piece piece, Vector2Int targetPosition)
        {
            if (Mathf.Abs(piece.Position.x - targetPosition.x) != Mathf.Abs(piece.Position.y - targetPosition.y))
                return false;
            
            // Check for clear path
            return IsPathClear(piece.Position, targetPosition);
        }
        
        private bool IsQueenMoveValid(Piece piece, Vector2Int targetPosition)
        {
            // Queen moves like both Rook and Bishop
            if (piece.Position.x == targetPosition.x || piece.Position.y == targetPosition.y ||
                Mathf.Abs(piece.Position.x - targetPosition.x) == Mathf.Abs(piece.Position.y - targetPosition.y))
            {
                return IsPathClear(piece.Position, targetPosition);
            }
            
            return false;
        }
        
        private bool IsPathClear(Vector2Int start, Vector2Int target)
        {
            int xStep = target.x == start.x ? 0 : (target.x > start.x ? 1 : -1);
            int yStep = target.y == start.y ? 0 : (target.y > start.y ? 1 : -1);
            Vector2Int current = start;
            
            while (current != target)
            {
                current += new Vector2Int(xStep, yStep);
                if (current != target && board[current.x, current.y] != null)
                {
                    return false; // Path is blocked
                }
            }
            
            return true; // Path is clear
        }
        
        private bool IsKingMoveValid(Piece piece, Vector2Int targetPosition)
        {
            int dx = Mathf.Abs(targetPosition.x - piece.Position.x);
            int dy = Mathf.Abs(targetPosition.y - piece.Position.y);
            return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
        }
        
        private bool IsCloneMoveValid(Piece piece, Vector2Int targetPosition)
        {
            // Clone can move like a Bishop or a Knight
            return IsBishopMoveValid(piece, targetPosition) || IsKnightMoveValid(piece, targetPosition);
        }
        
        
        private bool IsCastlingMoveValid(Piece king, Vector2Int targetPosition)
        {
            int direction = targetPosition.y - king.Position.y > 0 ? 1 : -1; // Determine direction
            int rookY = direction > 0 ? 9 : 0; // Right or left rook position
            Piece rook = board[king.Position.x, rookY];
            
            // Check if the rook is valid for castling
            if (rook != null && rook.Type == PieceType.Rook && !rook.HasMoved)
            {
                // Ensure the path is clear between the king and rook
                int startY = king.Position.y + direction;
                for (int y = startY; y != rookY; y += direction)
                {
                    if (board[king.Position.x, y] != null)
                        return false; // Path is blocked
                }
                
                // Ensure the king does not move through check
                Vector2Int tempPosition = new Vector2Int(king.Position.x, king.Position.y + direction);
                for (int i = 0; i < 2; i++)
                {
                    if (IsInCheckAfterMove(king.Team, tempPosition))
                        return false; // King moves through check
                    tempPosition.y += direction;
                }
                
                return true; // Valid castling move
            }
            
            return false; // Rook is not valid for castling
        }
        
        private bool IsInCheckAfterMove(Team team, Vector2Int position)
        {
            Vector2Int originalKingPosition = FindKingPosition(team);
            Piece originalKing = board[originalKingPosition.x, originalKingPosition.y];
            
            // Temporarily move the king
            board[position.x, position.y] = originalKing;
            board[originalKingPosition.x, originalKingPosition.y] = null;
            originalKing.Position = position;
            
            bool isInCheck = IsInCheck(team);
            
            // Undo the move
            board[originalKingPosition.x, originalKingPosition.y] = originalKing;
            board[position.x, position.y] = null;
            originalKing.Position = originalKingPosition;
            
            return isInCheck;
        }
        
        public void MovePiece(Piece piece, Vector2Int targetPosition)
        {
            if (IsMoveValid(piece, targetPosition))
            {
                // Handle castling
                if (piece.Type == PieceType.King && Mathf.Abs(targetPosition.y - piece.Position.y) == 2)
                {
                    // Castling move
                    HandleCastling(piece, targetPosition);
                }
                else
                {
                    board[targetPosition.x, targetPosition.y] = piece;
                    board[piece.Position.x, piece.Position.y] = null;
                    piece.Position = targetPosition;
                }
                
                piece.HasMoved = true; // Mark the piece as moved
                
                // Check for game state after move
                CheckGameState();
                
                // Switch turn
                currentTurn = (currentTurn == Team.White) ? Team.Black : Team.White;
            }
        }
        
        private void HandleCastling(Piece king, Vector2Int targetPosition)
        {
            int direction = targetPosition.y - king.Position.y > 0 ? 1 : -1; // Determine direction
            int rookY = direction > 0 ? 9 : 0; // Right or left rook position
            Piece rook = board[king.Position.x, rookY];
            
            // Move the king
            board[targetPosition.x, targetPosition.y] = king;
            board[king.Position.x, king.Position.y] = null;
            king.Position = targetPosition;
            
            // Move the rook next to the king
            Vector2Int rookTargetPosition = new Vector2Int(king.Position.x, king.Position.y - direction);
            board[rookTargetPosition.x, rookTargetPosition.y] = rook;
            board[rook.Position.x, rook.Position.y] = null;
            rook.Position = rookTargetPosition;
            
            king.HasMoved = true; // Mark the king as moved
            rook.HasMoved = true; // Mark the rook as moved
        }
        
        private void CheckGameState()
        {
            // Check if the king of the current player is in check
            if (IsInCheck(currentTurn))
            {
                if (IsCheckmate(currentTurn))
                    gameState = GameState.Checkmate;
                else
                    gameState = GameState.Check;
            }
            else if (IsStalemate(currentTurn))
            {
                gameState = GameState.Stalemate;
            }
            else
            {
                gameState = GameState.Ongoing;
            }
        }
        
        private bool IsInCheck(Team team)
        {
            Vector2Int kingPosition = FindKingPosition(team);
            foreach (Piece piece in GetOpponentPieces(team))
            {
                if (IsMoveValid(piece, kingPosition))
                    return true;
            }
            
            return false;
        }
        
        private Vector2Int FindKingPosition(Team team)
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Piece piece = board[x, y];
                    if (piece != null && piece.Type == PieceType.King && piece.Team == team)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            
            return Vector2Int.zero; // King not found (shouldn't happen)
        }
        
        private List<Piece> GetOpponentPieces(Team team)
        {
            List<Piece> opponentPieces = new List<Piece>();
            Team opponent = (team == Team.White) ? Team.Black : Team.White;
            
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Piece piece = board[x, y];
                    if (piece != null && piece.Team == opponent)
                    {
                        opponentPieces.Add(piece);
                    }
                }
            }
            
            return opponentPieces;
        }
        
        private bool IsCheckmate(Team team)
        {
            // Check if the current player's king is in check and cannot make any valid moves
            Vector2Int kingPosition = FindKingPosition(team);
            foreach (Piece piece in GetPlayerPieces(team))
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        if (IsMoveValid(piece, new Vector2Int(x, y)))
                        {
                            // Simulate the move
                            Piece targetPiece = board[x, y];
                            Piece originalPiece = board[piece.Position.x, piece.Position.y];
                            
                            board[x, y] = originalPiece;
                            board[piece.Position.x, piece.Position.y] = null;
                            originalPiece.Position = new Vector2Int(x, y);
                            
                            // Check if the king is still in check
                            if (!IsInCheck(team))
                            {
                                // Undo the move
                                board[piece.Position.x, piece.Position.y] = originalPiece;
                                board[x, y] = targetPiece;
                                originalPiece.Position = piece.Position;
                                return false; // Not checkmate
                            }
                            
                            // Undo the move
                            board[piece.Position.x, piece.Position.y] = originalPiece;
                            board[x, y] = targetPiece;
                            originalPiece.Position = piece.Position;
                        }
                    }
                }
            }
            
            return true; // No valid moves, checkmate
        }
        
        private List<Piece> GetPlayerPieces(Team team)
        {
            List<Piece> playerPieces = new List<Piece>();
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Piece piece = board[x, y];
                    if (piece != null && piece.Team == team)
                    {
                        playerPieces.Add(piece);
                    }
                }
            }
            
            return playerPieces;
        }
        
        private bool IsStalemate(Team team)
        {
            // If the king is not in check but there are no valid moves
            if (!IsInCheck(team))
            {
                foreach (Piece piece in GetPlayerPieces(team))
                {
                    for (int x = 0; x < 10; x++)
                    {
                        for (int y = 0; y < 10; y++)
                        {
                            if (IsMoveValid(piece, new Vector2Int(x, y)))
                                return false; // Found a valid move, not stalemate
                        }
                    }
                }
                
                return true; // No valid moves, stalemate
            }
            
            return false; // King is in check, not stalemate
        }
    }
}
