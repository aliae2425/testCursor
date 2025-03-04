# Snake en ligne de commande

Un jeu Snake simple qui s'exécute dans le terminal, créé en Python avec la bibliothèque curses.

## Prérequis

- Python 3.x
- La bibliothèque curses :
  - Sur Windows : Installez windows-curses avec la commande :
    ```
    pip install windows-curses
    ```
  - Sur Linux/Mac : curses est déjà inclus par défaut dans Python

## Comment jouer

1. Exécutez le script avec la commande :
   ```
   python snake.py
   ```

2. Utilisez les touches ZQSD pour diriger le serpent :
   - Z : Haut
   - S : Bas
   - Q : Gauche
   - D : Droite

3. Objectif du jeu :
   - Mangez les '*' pour grandir et marquer des points
   - Évitez les murs et ne vous mordez pas la queue !
   - Le score s'affiche en haut de l'écran

4. Le jeu se termine si :
   - Vous touchez un mur
   - Vous vous mordez la queue

Amusez-vous bien ! 