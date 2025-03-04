import curses
import random
import time

def main(stdscr):
    # Configuration initiale
    curses.curs_set(0)
    stdscr.nodelay(1)
    stdscr.timeout(100)

    # Dimensions du jeu
    sh, sw = stdscr.getmaxyx()
    w = stdscr.subwin(sh, sw, 0, 0)
    w.box()
    
    # Position initiale du serpent
    snake_x = sw//4
    snake_y = sh//2
    snake = [(snake_y, snake_x)]
    
    # Direction initiale (droite)
    direction = 'd'
    
    # Position initiale de la nourriture
    food = (sh//2, sw//2)
    w.addch(food[0], food[1], '*')
    
    # Score
    score = 0
    
    while True:
        # Afficher le score
        w.addstr(0, 2, f'Score: {score}')
        
        # Obtenir l'entrée utilisateur
        key = w.getch()
        if key != -1:
            direction = chr(key).lower()
        
        # Calculer la nouvelle position de la tête
        head = snake[0]
        if direction == 'd':  # Droite
            new_head = (head[0], head[1] + 1)
        elif direction == 'q':  # Gauche
            new_head = (head[0], head[1] - 1)
        elif direction == 'z':  # Haut
            new_head = (head[0] - 1, head[1])
        elif direction == 's':  # Bas
            new_head = (head[0] + 1, head[1])
        else:
            continue
        
        # Ajouter la nouvelle tête
        snake.insert(0, new_head)
        
        # Vérifier si le serpent mange la nourriture
        if snake[0] == food:
            score += 1
            # Générer une nouvelle position pour la nourriture
            while True:
                food = (random.randint(1, sh-2), random.randint(1, sw-2))
                if food not in snake:
                    break
            w.addch(food[0], food[1], '*')
        else:
            # Effacer la queue du serpent
            tail = snake.pop()
            w.addch(tail[0], tail[1], ' ')
        
        # Dessiner la tête du serpent
        w.addch(snake[0][0], snake[0][1], '#')
        
        # Vérifier les collisions
        if (snake[0][0] in [0, sh-1] or 
            snake[0][1] in [0, sw-1] or 
            snake[0] in snake[1:]):
            w.addstr(sh//2, sw//2-5, 'Game Over!')
            w.refresh()
            time.sleep(2)
            break
        
        w.refresh()

if __name__ == '__main__':
    curses.wrapper(main) 