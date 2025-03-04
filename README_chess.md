# Jeu d'Échecs en Ligne de Commande

Un jeu d'échecs simple qui s'exécute dans le terminal, créé en Python.

## Comment jouer

1. Lancez le jeu avec la commande :
   ```
   python chess.py
   ```

2. Le plateau est affiché avec les coordonnées :
   - Les colonnes sont marquées de a à h
   - Les lignes sont numérotées de 1 à 8
   - Les pièces blanches sont en majuscules : ♔ ♕ ♖ ♗ ♘ ♙
   - Les pièces noires sont en minuscules : ♚ ♛ ♜ ♝ ♞ ♟
   - Les cases vides sont représentées par un point (·)

3. Pour déplacer une pièce :
   - Entrez les coordonnées de départ et d'arrivée
   - Format : `[colonne][ligne] [colonne][ligne]`
   - Exemple : `e2 e4` pour avancer le pion du roi de deux cases

4. Mode de jeu :
   - Vous jouez les Blancs contre l'ordinateur qui joue les Noirs
   - L'ordinateur utilise une stratégie simple basée sur :
     - La capture des pièces (avec différentes valeurs pour chaque type)
     - Le contrôle du centre de l'échiquier
     - Une part d'aléatoire pour varier son jeu

5. Règles de déplacement :
   - Pion (♙/♟) :
     - Avance d'une case vers l'avant
     - Peut avancer de deux cases lors de son premier mouvement
     - Capture en diagonale
     - Se transforme en une autre pièce en atteignant la dernière rangée (promotion)
   - Cavalier (♘/♞) :
     - Se déplace en "L" (2 cases dans une direction puis 1 case perpendiculairement)
     - Peut sauter par-dessus les autres pièces
   - Fou (♗/♝) :
     - Se déplace en diagonale
     - Ne peut pas sauter par-dessus les autres pièces
   - Tour (♖/♜) :
     - Se déplace horizontalement ou verticalement
     - Ne peut pas sauter par-dessus les autres pièces
   - Dame (♕/♛) :
     - Combine les mouvements du fou et de la tour
     - Ne peut pas sauter par-dessus les autres pièces
   - Roi (♔/♚) :
     - Se déplace d'une seule case dans n'importe quelle direction
     - Doit être protégé de l'échec

6. Fonctionnalités spéciales :
   - Détection de l'échec
   - Promotion des pions :
     - Choisissez D (Dame), T (Tour), F (Fou) ou C (Cavalier)
     - L'ordinateur promeut automatiquement en Dame

7. Règles générales :
   - Les blancs commencent
   - Les joueurs jouent à tour de rôle
   - Pour quitter la partie, tapez 'quit'

## Symboles des pièces
- ♔/♚ : Roi
- ♕/♛ : Dame
- ♖/♜ : Tour
- ♗/♝ : Fou
- ♘/♞ : Cavalier
- ♙/♟ : Pion

## Notes
- Le jeu vérifie maintenant :
  - La validité des mouvements selon les règles des échecs
  - La situation d'échec
  - La promotion des pions
- Les coups spéciaux (roque, prise en passant) ne sont pas encore implémentés
- Le jeu ne vérifie pas encore l'échec et mat
- L'IA utilise une stratégie simple et peut être améliorée 