using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWPF
{
    public class ChessAI
    {
        private readonly Random random = new();
        private const int MaxDepth = 4;

        private static readonly Dictionary<string, int> PieceValues = new()
        {
            { "♙", 100 },  // Pion blanc
            { "♟", -100 }, // Pion noir
            { "♖", 500 },  // Tour blanche
            { "♜", -500 }, // Tour noire
            { "♘", 300 },  // Cavalier blanc
            { "♞", -300 }, // Cavalier noir
            { "♗", 300 },  // Fou blanc
            { "♝", -300 }, // Fou noir
            { "♕", 900 },  // Dame blanche
            { "♛", -900 }, // Dame noire
            { "♔", 10000 }, // Roi blanc
            { "♚", -10000 } // Roi noir
        };

        private static readonly int[,] PawnPositionBonus = {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 5,  5, 10, 25, 25, 10,  5,  5 },
            { 0,  0,  0, 20, 20,  0,  0,  0 },
            { 5, -5,-10,  0,  0,-10, -5,  5 },
            { 5, 10, 10,-20,-20, 10, 10,  5 },
            { 0,  0,  0,  0,  0,  0,  0,  0 }
        };

        public (int FromRow, int FromCol, int ToRow, int ToCol) GetBestMove(ChessGame game)
        {
            var bestMove = MinimaxRoot(game, MaxDepth, true);
            return bestMove;
        }

        private (int FromRow, int FromCol, int ToRow, int ToCol) MinimaxRoot(ChessGame game, int depth, bool isMaximizing)
        {
            var bestMove = (-1, -1, -1, -1);
            var bestValue = isMaximizing ? int.MinValue : int.MaxValue;
            var moves = GetAllPossibleMoves(game, !game.IsWhiteTurn);

            foreach (var move in moves)
            {
                var gameCopy = CloneGame(game);
                if (gameCopy.TryMove(move.FromRow, move.FromCol, move.ToRow, move.ToCol))
                {
                    var value = Minimax(gameCopy, depth - 1, int.MinValue, int.MaxValue, !isMaximizing);
                    if (isMaximizing && value > bestValue || !isMaximizing && value < bestValue)
                    {
                        bestValue = value;
                        bestMove = move;
                    }
                }
            }

            return bestMove;
        }

        private int Minimax(ChessGame game, int depth, int alpha, int beta, bool isMaximizing)
        {
            if (depth == 0 || game.IsGameOver)
                return EvaluatePosition(game);

            var moves = GetAllPossibleMoves(game, !game.IsWhiteTurn);

            if (isMaximizing)
            {
                var value = int.MinValue;
                foreach (var move in moves)
                {
                    var gameCopy = CloneGame(game);
                    if (gameCopy.TryMove(move.FromRow, move.FromCol, move.ToRow, move.ToCol))
                    {
                        value = Math.Max(value, Minimax(gameCopy, depth - 1, alpha, beta, false));
                        alpha = Math.Max(alpha, value);
                        if (beta <= alpha)
                            break;
                    }
                }
                return value;
            }
            else
            {
                var value = int.MaxValue;
                foreach (var move in moves)
                {
                    var gameCopy = CloneGame(game);
                    if (gameCopy.TryMove(move.FromRow, move.FromCol, move.ToRow, move.ToCol))
                    {
                        value = Math.Min(value, Minimax(gameCopy, depth - 1, alpha, beta, true));
                        beta = Math.Min(beta, value);
                        if (beta <= alpha)
                            break;
                    }
                }
                return value;
            }
        }

        private List<(int FromRow, int FromCol, int ToRow, int ToCol)> GetAllPossibleMoves(ChessGame game, bool forWhite)
        {
            var moves = new List<(int FromRow, int FromCol, int ToRow, int ToCol)>();

            for (int fromRow = 0; fromRow < 8; fromRow++)
            {
                for (int fromCol = 0; fromCol < 8; fromCol++)
                {
                    var piece = game.GetPiece(fromRow, fromCol);
                    if (piece != null && piece.IsWhite == forWhite)
                    {
                        for (int toRow = 0; toRow < 8; toRow++)
                        {
                            for (int toCol = 0; toCol < 8; toCol++)
                            {
                                var gameCopy = CloneGame(game);
                                if (gameCopy.TryMove(fromRow, fromCol, toRow, toCol))
                                {
                                    moves.Add((fromRow, fromCol, toRow, toCol));
                                }
                            }
                        }
                    }
                }
            }

            // Mélanger les coups pour plus de variété dans le jeu
            return moves.OrderBy(x => random.Next()).ToList();
        }

        private int EvaluatePosition(ChessGame game)
        {
            int score = 0;

            // Évaluer les pièces et leur position
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = game.GetPiece(row, col);
                    if (piece != null)
                    {
                        // Valeur de base de la pièce
                        score += PieceValues[piece.Symbol];

                        // Bonus de position pour les pions
                        if (piece.Symbol == "♙") // Pion blanc
                            score += PawnPositionBonus[row, col];
                        else if (piece.Symbol == "♟") // Pion noir
                            score -= PawnPositionBonus[7 - row, col];
                    }
                }
            }

            // Bonus/Malus pour les situations spéciales
            if (game.IsGameOver)
            {
                if (game.GameResult.Contains("Blancs gagnent"))
                    score += 20000;
                else if (game.GameResult.Contains("Noirs gagnent"))
                    score -= 20000;
            }
            else if (game.IsInCheck)
            {
                if (game.IsWhiteTurn)
                    score -= 50; // Les noirs donnent échec
                else
                    score += 50; // Les blancs donnent échec
            }

            return score;
        }

        private ChessGame CloneGame(ChessGame original)
        {
            var clone = new ChessGame();
            // Copier l'état du jeu
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = original.GetPiece(row, col);
                    if (piece != null)
                    {
                        clone.TryMove(row, col, row, col); // Place la pièce à la même position
                    }
                }
            }
            return clone;
        }
    }
} 