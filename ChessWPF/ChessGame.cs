using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWPF
{
    public class ChessPiece
    {
        public bool IsWhite { get; }
        public string Symbol { get; private set; }
        public bool HasMoved { get; set; }
        public bool CanBeTakenEnPassant { get; set; }

        public ChessPiece(bool isWhite, string symbol)
        {
            IsWhite = isWhite;
            Symbol = symbol;
            HasMoved = false;
            CanBeTakenEnPassant = false;
        }

        public void Promote(string newSymbol)
        {
            Symbol = newSymbol;
        }
    }

    public class ChessGame
    {
        private readonly ChessPiece?[,] board = new ChessPiece[8, 8];
        public bool IsWhiteTurn { get; private set; } = true;
        public List<ChessPiece> CapturedPieces { get; } = new();
        public string LastMove { get; private set; } = "";
        public bool IsGameOver { get; private set; }
        public string GameResult { get; private set; } = "";
        public bool IsInCheck { get; private set; }
        public bool NeedsPromotion { get; private set; }
        public (int Row, int Col) PromotionSquare { get; private set; }

        public ChessGame()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Placement des pièces noires
            board[0, 0] = new ChessPiece(false, "♜");
            board[0, 1] = new ChessPiece(false, "♞");
            board[0, 2] = new ChessPiece(false, "♝");
            board[0, 3] = new ChessPiece(false, "♛");
            board[0, 4] = new ChessPiece(false, "♚");
            board[0, 5] = new ChessPiece(false, "♝");
            board[0, 6] = new ChessPiece(false, "♞");
            board[0, 7] = new ChessPiece(false, "♜");

            for (int i = 0; i < 8; i++)
                board[1, i] = new ChessPiece(false, "♟");

            // Placement des pièces blanches
            board[7, 0] = new ChessPiece(true, "♖");
            board[7, 1] = new ChessPiece(true, "♘");
            board[7, 2] = new ChessPiece(true, "♗");
            board[7, 3] = new ChessPiece(true, "♕");
            board[7, 4] = new ChessPiece(true, "♔");
            board[7, 5] = new ChessPiece(true, "♗");
            board[7, 6] = new ChessPiece(true, "♘");
            board[7, 7] = new ChessPiece(true, "♖");

            for (int i = 0; i < 8; i++)
                board[6, i] = new ChessPiece(true, "♙");
        }

        public ChessPiece? GetPiece(int row, int col)
        {
            if (row >= 0 && row < 8 && col >= 0 && col < 8)
                return board[row, col];
            return null;
        }

        public bool TryMove(int fromRow, int fromCol, int toRow, int toCol, string? promotionPiece = null)
        {
            if (IsGameOver) return false;

            var piece = GetPiece(fromRow, fromCol);
            if (piece == null || piece.IsWhite != IsWhiteTurn)
                return false;

            // Gestion de la promotion en attente
            if (NeedsPromotion)
            {
                if (promotionPiece == null || (fromRow != PromotionSquare.Row || fromCol != PromotionSquare.Col))
                    return false;
                
                var promotedPiece = board[fromRow, fromCol];
                promotedPiece!.Promote(promotionPiece);
                NeedsPromotion = false;
                IsWhiteTurn = !IsWhiteTurn;
                return true;
            }

            bool isRoque = IsRoqueMove(fromRow, fromCol, toRow, toCol);
            bool isEnPassant = IsEnPassantMove(fromRow, fromCol, toRow, toCol);

            if (!isRoque && !isEnPassant && !IsValidMove(fromRow, fromCol, toRow, toCol))
                return false;

            // Vérifier si le mouvement met le roi en échec
            if (!SimulateMoveAndCheckKingSafety(fromRow, fromCol, toRow, toCol))
                return false;

            // Réinitialiser les pions en passant du tour précédent
            ResetEnPassant();

            // Effectuer le mouvement
            if (isRoque)
            {
                PerformRoque(fromRow, fromCol, toRow, toCol);
            }
            else if (isEnPassant)
            {
                PerformEnPassant(fromRow, fromCol, toRow, toCol);
            }
            else
            {
                // Capturer la pièce si présente
                var capturedPiece = board[toRow, toCol];
                if (capturedPiece != null)
                    CapturedPieces.Add(capturedPiece);

                // Effectuer le mouvement
                board[toRow, toCol] = piece;
                board[fromRow, fromCol] = null;
                piece.HasMoved = true;

                // Vérifier la prise en passant possible
                if (IsPawn(piece) && Math.Abs(fromRow - toRow) == 2)
                {
                    piece.CanBeTakenEnPassant = true;
                }

                // Vérifier la promotion
                if (IsPawn(piece) && (toRow == 0 || toRow == 7))
                {
                    NeedsPromotion = true;
                    PromotionSquare = (toRow, toCol);
                    return true;
                }
            }

            // Enregistrer le coup
            LastMove = GenerateMoveName(fromRow, fromCol, toRow, toCol, isRoque, isEnPassant);

            // Vérifier l'état du jeu
            UpdateGameState();

            // Changer de tour si pas de promotion en attente
            if (!NeedsPromotion)
                IsWhiteTurn = !IsWhiteTurn;

            return true;
        }

        private void UpdateGameState()
        {
            IsInCheck = IsKingInCheck(!IsWhiteTurn);
            
            if (IsCheckmate(!IsWhiteTurn))
            {
                IsGameOver = true;
                GameResult = IsWhiteTurn ? "Les Blancs gagnent" : "Les Noirs gagnent";
            }
            else if (IsStaleMate(!IsWhiteTurn))
            {
                IsGameOver = true;
                GameResult = "Pat - Match nul";
            }
        }

        private bool IsCheckmate(bool isWhiteKing)
        {
            if (!IsKingInCheck(isWhiteKing)) return false;
            return !HasLegalMoves(isWhiteKing);
        }

        private bool IsStaleMate(bool isWhiteKing)
        {
            if (IsKingInCheck(isWhiteKing)) return false;
            return !HasLegalMoves(isWhiteKing);
        }

        private bool HasLegalMoves(bool isWhite)
        {
            for (int fromRow = 0; fromRow < 8; fromRow++)
            {
                for (int fromCol = 0; fromCol < 8; fromCol++)
                {
                    var piece = board[fromRow, fromCol];
                    if (piece == null || piece.IsWhite != isWhite) continue;

                    for (int toRow = 0; toRow < 8; toRow++)
                    {
                        for (int toCol = 0; toCol < 8; toCol++)
                        {
                            if (IsValidMove(fromRow, fromCol, toRow, toCol) &&
                                SimulateMoveAndCheckKingSafety(fromRow, fromCol, toRow, toCol))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool IsKingInCheck(bool isWhiteKing)
        {
            // Trouver la position du roi
            int kingRow = -1, kingCol = -1;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = board[row, col];
                    if (piece != null && IsKing(piece) && piece.IsWhite == isWhiteKing)
                    {
                        kingRow = row;
                        kingCol = col;
                        break;
                    }
                }
                if (kingRow != -1) break;
            }

            // Vérifier si une pièce adverse peut attaquer le roi
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = board[row, col];
                    if (piece != null && piece.IsWhite != isWhiteKing)
                    {
                        if (IsValidMove(row, col, kingRow, kingCol))
                            return true;
                    }
                }
            }

            return false;
        }

        private bool SimulateMoveAndCheckKingSafety(int fromRow, int fromCol, int toRow, int toCol)
        {
            var originalPiece = board[fromRow, fromCol];
            var targetPiece = board[toRow, toCol];

            // Simuler le mouvement
            board[toRow, toCol] = originalPiece;
            board[fromRow, fromCol] = null;

            // Vérifier si le roi est en échec
            bool isSafe = !IsKingInCheck(originalPiece!.IsWhite);

            // Restaurer la position
            board[fromRow, fromCol] = originalPiece;
            board[toRow, toCol] = targetPiece;

            return isSafe;
        }

        private bool IsRoqueMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = board[fromRow, fromCol];
            if (piece == null || !IsKing(piece) || piece.HasMoved) return false;

            // Vérifier si c'est un mouvement de roque
            if (fromRow != toRow || Math.Abs(fromCol - toCol) != 2) return false;

            int rookCol = toCol > fromCol ? 7 : 0;
            var rook = board[fromRow, rookCol];
            if (rook == null || !IsRook(rook) || rook.HasMoved) return false;

            // Vérifier si le chemin est libre
            int step = toCol > fromCol ? 1 : -1;
            for (int col = fromCol + step; col != rookCol; col += step)
            {
                if (board[fromRow, col] != null) return false;
            }

            // Vérifier si le roi n'est pas en échec et ne traverse pas de cases attaquées
            if (IsKingInCheck(piece.IsWhite)) return false;
            
            int checkCol = fromCol;
            do
            {
                checkCol += step;
                if (!SimulateMoveAndCheckKingSafety(fromRow, fromCol, fromRow, checkCol))
                    return false;
            } while (checkCol != toCol);

            return true;
        }

        private void PerformRoque(int fromRow, int fromCol, int toRow, int toCol)
        {
            var king = board[fromRow, fromCol];
            int rookFromCol = toCol > fromCol ? 7 : 0;
            int rookToCol = toCol > fromCol ? toCol - 1 : toCol + 1;
            var rook = board[fromRow, rookFromCol];

            // Déplacer le roi
            board[toRow, toCol] = king;
            board[fromRow, fromCol] = null;
            king!.HasMoved = true;

            // Déplacer la tour
            board[fromRow, rookToCol] = rook;
            board[fromRow, rookFromCol] = null;
            rook!.HasMoved = true;
        }

        private bool IsEnPassantMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = board[fromRow, fromCol];
            if (piece == null || !IsPawn(piece)) return false;

            int direction = piece.IsWhite ? -1 : 1;
            if (toRow != fromRow + direction || Math.Abs(toCol - fromCol) != 1) return false;

            var targetPiece = board[fromRow, toCol];
            return targetPiece != null && 
                   IsPawn(targetPiece) && 
                   targetPiece.IsWhite != piece.IsWhite && 
                   targetPiece.CanBeTakenEnPassant;
        }

        private void PerformEnPassant(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = board[fromRow, fromCol];
            var capturedPawn = board[fromRow, toCol];

            // Capturer le pion
            CapturedPieces.Add(capturedPawn!);
            board[fromRow, toCol] = null;

            // Déplacer le pion
            board[toRow, toCol] = piece;
            board[fromRow, fromCol] = null;
        }

        private void ResetEnPassant()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = board[row, col];
                    if (piece != null && piece.IsWhite == IsWhiteTurn)
                    {
                        piece.CanBeTakenEnPassant = false;
                    }
                }
            }
        }

        private string GenerateMoveName(int fromRow, int fromCol, int toRow, int toCol, bool isRoque, bool isEnPassant)
        {
            if (isRoque)
            {
                return toCol > fromCol ? "O-O" : "O-O-O";
            }

            var piece = board[toRow, toCol];
            string move = $"{GetSquareName(fromRow, fromCol)}-{GetSquareName(toRow, toCol)}";
            
            if (isEnPassant)
                move += " e.p.";
            else if (CapturedPieces.Count > 0 && CapturedPieces.Last().Symbol != null)
                move += $" x{CapturedPieces.Last().Symbol}";

            if (IsInCheck)
                move += IsGameOver ? "#" : "+";

            return move;
        }

        private bool IsPawn(ChessPiece piece) => 
            piece.Symbol == "♙" || piece.Symbol == "♟";

        private bool IsRook(ChessPiece piece) => 
            piece.Symbol == "♖" || piece.Symbol == "♜";

        private bool IsKing(ChessPiece piece) => 
            piece.Symbol == "♔" || piece.Symbol == "♚";

        private bool IsValidMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = board[fromRow, fromCol];
            var target = board[toRow, toCol];

            // Vérification de base
            if (piece == null || (target != null && target.IsWhite == piece.IsWhite))
                return false;

            // Règles spécifiques selon le type de pièce
            switch (piece.Symbol)
            {
                case "♙": // Pion blanc
                case "♟": // Pion noir
                    return IsValidPawnMove(fromRow, fromCol, toRow, toCol);
                case "♖":
                case "♜": // Tour
                    return IsValidRookMove(fromRow, fromCol, toRow, toCol);
                case "♘":
                case "♞": // Cavalier
                    return IsValidKnightMove(fromRow, fromCol, toRow, toCol);
                case "♗":
                case "♝": // Fou
                    return IsValidBishopMove(fromRow, fromCol, toRow, toCol);
                case "♕":
                case "♛": // Dame
                    return IsValidQueenMove(fromRow, fromCol, toRow, toCol);
                case "♔":
                case "♚": // Roi
                    return IsValidKingMove(fromRow, fromCol, toRow, toCol);
                default:
                    return false;
            }
        }

        private bool IsValidPawnMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = board[fromRow, fromCol];
            var direction = piece!.IsWhite ? -1 : 1;
            var startRow = piece.IsWhite ? 6 : 1;

            // Mouvement simple
            if (fromCol == toCol && toRow == fromRow + direction && board[toRow, toCol] == null)
                return true;

            // Double mouvement initial
            if (fromCol == toCol && fromRow == startRow && toRow == fromRow + 2 * direction)
                return board[toRow, toCol] == null && board[fromRow + direction, toCol] == null;

            // Capture en diagonale
            if (Math.Abs(fromCol - toCol) == 1 && toRow == fromRow + direction)
                return board[toRow, toCol] != null && board[toRow, toCol]!.IsWhite != piece.IsWhite;

            return false;
        }

        private bool IsValidRookMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (fromRow != toRow && fromCol != toCol)
                return false;

            return IsClearPath(fromRow, fromCol, toRow, toCol);
        }

        private bool IsValidKnightMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            int rowDiff = Math.Abs(fromRow - toRow);
            int colDiff = Math.Abs(fromCol - toCol);
            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        private bool IsValidBishopMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (Math.Abs(fromRow - toRow) != Math.Abs(fromCol - toCol))
                return false;

            return IsClearPath(fromRow, fromCol, toRow, toCol);
        }

        private bool IsValidQueenMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            return IsValidRookMove(fromRow, fromCol, toRow, toCol) ||
                   IsValidBishopMove(fromRow, fromCol, toRow, toCol);
        }

        private bool IsValidKingMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            return Math.Abs(fromRow - toRow) <= 1 && Math.Abs(fromCol - toCol) <= 1;
        }

        private bool IsClearPath(int fromRow, int fromCol, int toRow, int toCol)
        {
            int rowStep = fromRow == toRow ? 0 : (toRow - fromRow) / Math.Abs(toRow - fromRow);
            int colStep = fromCol == toCol ? 0 : (toCol - fromCol) / Math.Abs(toCol - fromCol);

            int currentRow = fromRow + rowStep;
            int currentCol = fromCol + colStep;

            while (currentRow != toRow || currentCol != toCol)
            {
                if (board[currentRow, currentCol] != null)
                    return false;

                currentRow += rowStep;
                currentCol += colStep;
            }

            return true;
        }

        private string GetSquareName(int row, int col)
        {
            return $"{(char)('a' + col)}{8 - row}";
        }
    }
} 