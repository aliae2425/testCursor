using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChessWPF
{
    public partial class MainWindow : Window
    {
        private readonly ChessGame game;
        private readonly ChessAI ai;
        private Button? selectedButton;
        private (int Row, int Col)? lastMove;
        private bool isAIEnabled;
        private readonly DispatcherTimer aiTimer;

        public MainWindow()
        {
            InitializeComponent();
            game = new ChessGame();
            ai = new ChessAI();
            InitializeBoard();
            UpdateStatusMessage();

            aiTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            aiTimer.Tick += AiTimer_Tick;
        }

        private void ToggleAI_Click(object sender, RoutedEventArgs e)
        {
            isAIEnabled = !isAIEnabled;
            AIButton.Content = isAIEnabled ? "Jouer à deux" : "Jouer contre l'IA";

            if (isAIEnabled && !game.IsWhiteTurn)
            {
                aiTimer.Start();
            }
        }

        private void AiTimer_Tick(object sender, EventArgs e)
        {
            aiTimer.Stop();
            MakeAIMove();
        }

        private void MakeAIMove()
        {
            if (!isAIEnabled || game.IsGameOver || game.NeedsPromotion) return;

            var move = ai.GetBestMove(game);
            if (move != (-1, -1, -1, -1))
            {
                if (game.TryMove(move.FromRow, move.FromCol, move.ToRow, move.ToCol))
                {
                    if (game.NeedsPromotion)
                    {
                        // L'IA choisit toujours la dame pour la promotion
                        game.TryMove(move.ToRow, move.ToCol, move.ToRow, move.ToCol, game.IsWhiteTurn ? "♕" : "♛");
                    }
                    UpdateGameDisplay();
                }
            }
        }

        private void Square_Click(object sender, RoutedEventArgs e)
        {
            if (isAIEnabled && !game.IsWhiteTurn) return; // Empêcher le joueur de jouer pendant le tour de l'IA

            var button = (Button)sender;
            var position = GetButtonPosition(button);

            if (game.IsGameOver)
            {
                MessageBox.Show(game.GameResult, "Partie terminée", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectedButton == null)
            {
                // Sélectionner une pièce
                var piece = game.GetPiece(position.row, position.col);
                if (piece != null && piece.IsWhite == game.IsWhiteTurn)
                {
                    selectedButton = button;
                    button.Background = new SolidColorBrush(Color.FromRgb(130, 151, 105));
                }
            }
            else
            {
                // Tenter de déplacer la pièce
                var fromPos = GetButtonPosition(selectedButton);
                var toPos = position;

                if (game.TryMove(fromPos.row, fromPos.col, toPos.row, toPos.col))
                {
                    if (game.NeedsPromotion)
                    {
                        ShowPromotionDialog(game.IsWhiteTurn);
                        lastMove = (toPos.row, toPos.col);
                    }
                    else
                    {
                        UpdateGameDisplay();
                        if (isAIEnabled && !game.IsGameOver)
                        {
                            aiTimer.Start();
                        }
                    }
                }

                // Réinitialiser la sélection
                ResetSelectedButton(fromPos.row, fromPos.col);
            }
        }

        private void ShowPromotionDialog(bool isWhite)
        {
            PromotionDialog.Visibility = Visibility.Visible;
            var buttons = new[] { PromoteToQueen, PromoteToRook, PromoteToBishop, PromoteToKnight };
            foreach (var button in buttons)
            {
                string content = button.Content.ToString()!;
                if (!isWhite)
                {
                    // Convertir en pièce noire
                    button.Content = content switch
                    {
                        "♕" => "♛",
                        "♖" => "♜",
                        "♗" => "♝",
                        "♘" => "♞",
                        _ => content
                    };
                }
            }
        }

        private void PromotePiece_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string promotionPiece = button.Content.ToString()!;

            if (lastMove.HasValue)
            {
                game.TryMove(lastMove.Value.Row, lastMove.Value.Col, lastMove.Value.Row, lastMove.Value.Col, promotionPiece);
                UpdateGameDisplay();

                if (isAIEnabled && !game.IsGameOver)
                {
                    aiTimer.Start();
                }
            }

            PromotionDialog.Visibility = Visibility.Collapsed;
            lastMove = null;
        }

        private void UpdateGameDisplay()
        {
            UpdateBoard();
            UpdateStatusMessage();
            TurnIndicator.Text = game.IsWhiteTurn ? "Tour aux Blancs" : "Tour aux Noirs";
            MoveHistory.Items.Add($"{game.LastMove}");
            UpdateCapturedPieces();

            if (game.IsGameOver)
            {
                MessageBox.Show(game.GameResult, "Partie terminée", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateStatusMessage()
        {
            if (game.IsGameOver)
            {
                StatusMessage.Text = game.GameResult;
            }
            else if (game.IsInCheck)
            {
                StatusMessage.Text = "Échec !";
            }
            else
            {
                StatusMessage.Text = "";
            }
        }

        private void ResetSelectedButton(int row, int col)
        {
            if (selectedButton != null)
            {
                selectedButton.Background = ((row + col) % 2 == 0)
                    ? new SolidColorBrush(Color.FromRgb(240, 217, 181))
                    : new SolidColorBrush(Color.FromRgb(181, 136, 99));
                selectedButton = null;
            }
        }

        private void UpdateBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var button = GetSquareButton(row, col);
                    UpdateSquare(button, row, col);
                }
            }
        }

        private void UpdateSquare(Button button, int row, int col)
        {
            var piece = game.GetPiece(row, col);
            button.Content = piece?.Symbol ?? "";
            button.Foreground = piece?.IsWhite == true 
                ? Brushes.Black 
                : new SolidColorBrush(Color.FromRgb(100, 100, 100));
        }

        private Button GetSquareButton(int row, int col)
        {
            return (Button)ChessBoard.Children[row * 8 + col];
        }

        private (int row, int col) GetButtonPosition(Button button)
        {
            int index = ChessBoard.Children.IndexOf(button);
            return (index / 8, index % 8);
        }

        private void UpdateCapturedPieces()
        {
            CapturedPieces.Children.Clear();
            foreach (var piece in game.CapturedPieces)
            {
                var textBlock = new TextBlock
                {
                    Text = piece.Symbol,
                    FontSize = 24,
                    FontFamily = new FontFamily("Segoe UI Symbol"),
                    Foreground = piece.IsWhite ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    Margin = new Thickness(5)
                };
                CapturedPieces.Children.Add(textBlock);
            }
        }

        private void InitializeBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var button = new Button
                    {
                        Width = 60,
                        Height = 60,
                        FontSize = 36,
                        FontFamily = new FontFamily("Segoe UI Symbol"),
                        Background = (row + col) % 2 == 0 ? new SolidColorBrush(Color.FromRgb(240, 217, 181)) 
                                                        : new SolidColorBrush(Color.FromRgb(181, 136, 99))
                    };

                    button.Click += Square_Click;
                    ChessBoard.Children.Add(button);
                    UpdateSquare(button, row, col);
                }
            }
        }
    }
} 