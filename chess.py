import random
import time

class Piece:
    def __init__(self, color, symbol):
        self.color = color  # 'blanc' ou 'noir'
        self.symbol = symbol
        self.has_moved = False

class Roi(Piece):
    def __init__(self, color):
        super().__init__(color, '♔' if color == 'blanc' else '♚')

class Dame(Piece):
    def __init__(self, color):
        super().__init__(color, '♕' if color == 'blanc' else '♛')

class Tour(Piece):
    def __init__(self, color):
        super().__init__(color, '♖' if color == 'blanc' else '♜')

class Fou(Piece):
    def __init__(self, color):
        super().__init__(color, '♗' if color == 'blanc' else '♝')

class Cavalier(Piece):
    def __init__(self, color):
        super().__init__(color, '♘' if color == 'blanc' else '♞')

class Pion(Piece):
    def __init__(self, color):
        super().__init__(color, '♙' if color == 'blanc' else '♟')

class Echiquier:
    def __init__(self, mode_ia=True):
        self.plateau = [[None for _ in range(8)] for _ in range(8)]
        self.initialiser_plateau()
        self.tour_blanc = True
        self.en_cours = True
        self.mode_ia = mode_ia
        self.historique_coups = []
        self.en_echec = False

    def initialiser_plateau(self):
        # Placement des pièces noires
        self.plateau[0] = [
            Tour('noir'), Cavalier('noir'), Fou('noir'), Dame('noir'),
            Roi('noir'), Fou('noir'), Cavalier('noir'), Tour('noir')
        ]
        for i in range(8):
            self.plateau[1][i] = Pion('noir')

        # Placement des pièces blanches
        self.plateau[7] = [
            Tour('blanc'), Cavalier('blanc'), Fou('blanc'), Dame('blanc'),
            Roi('blanc'), Fou('blanc'), Cavalier('blanc'), Tour('blanc')
        ]
        for i in range(8):
            self.plateau[6][i] = Pion('blanc')

    def afficher(self):
        print("\n  a b c d e f g h")
        print("  ─────────────")
        for i in range(8):
            print(f"{8-i}│", end=" ")
            for j in range(8):
                piece = self.plateau[i][j]
                if piece is None:
                    print("·", end=" ")
                else:
                    print(piece.symbol, end=" ")
            print(f"│{8-i}")
        print("  ─────────────")
        print("  a b c d e f g h")
        print(f"\nTour aux {'Blancs' if self.tour_blanc else 'Noirs'}")

    def est_mouvement_valide(self, depart, arrivee):
        # Conversion des coordonnées algébriques en indices
        dx, dy = ord(depart[0]) - ord('a'), 8 - int(depart[1])
        ax, ay = ord(arrivee[0]) - ord('a'), 8 - int(arrivee[1])

        # Vérification des limites du plateau
        if not (0 <= dx < 8 and 0 <= dy < 8 and 0 <= ax < 8 and 0 <= ay < 8):
            return False

        piece = self.plateau[dy][dx]
        # Vérification si la pièce existe et appartient au bon joueur
        if piece is None or (piece.color == 'blanc') != self.tour_blanc:
            return False

        # Vérification si la case d'arrivée contient une pièce de la même couleur
        if self.plateau[ay][ax] and self.plateau[ay][ax].color == piece.color:
            return False

        # Calcul du déplacement
        delta_x = ax - dx
        delta_y = ay - dy

        # Règles spécifiques pour chaque type de pièce
        if isinstance(piece, Pion):
            # Direction du mouvement selon la couleur
            direction = 1 if piece.color == 'noir' else -1
            
            # Mouvement simple vers l'avant
            if delta_x == 0 and delta_y == direction and self.plateau[ay][ax] is None:
                return True
                
            # Double mouvement initial
            if not piece.has_moved and delta_x == 0 and delta_y == 2 * direction:
                # Vérifier si le chemin est libre
                case_intermediaire = self.plateau[dy + direction][dx]
                return case_intermediaire is None and self.plateau[ay][ax] is None
                
            # Prise en diagonale
            if abs(delta_x) == 1 and delta_y == direction:
                return self.plateau[ay][ax] is not None

            return False

        elif isinstance(piece, Cavalier):
            # Le cavalier se déplace en L
            return (abs(delta_x) == 2 and abs(delta_y) == 1) or (abs(delta_x) == 1 and abs(delta_y) == 2)

        elif isinstance(piece, Fou):
            # Le fou se déplace en diagonale
            if abs(delta_x) != abs(delta_y):
                return False
            # Vérifier si le chemin est libre
            pas_x = 1 if delta_x > 0 else -1
            pas_y = 1 if delta_y > 0 else -1
            x, y = dx + pas_x, dy + pas_y
            while x != ax:
                if self.plateau[y][x] is not None:
                    return False
                x += pas_x
                y += pas_y
            return True

        elif isinstance(piece, Tour):
            # La tour se déplace horizontalement ou verticalement
            if delta_x != 0 and delta_y != 0:
                return False
            # Vérifier si le chemin est libre
            if delta_x == 0:
                pas = 1 if delta_y > 0 else -1
                y = dy + pas
                while y != ay:
                    if self.plateau[y][dx] is not None:
                        return False
                    y += pas
            else:
                pas = 1 if delta_x > 0 else -1
                x = dx + pas
                while x != ax:
                    if self.plateau[dy][x] is not None:
                        return False
                    x += pas
            return True

        elif isinstance(piece, Dame):
            # La dame combine les mouvements de la tour et du fou
            if delta_x == 0 or delta_y == 0:  # Mouvement comme une tour
                if delta_x == 0:
                    pas = 1 if delta_y > 0 else -1
                    y = dy + pas
                    while y != ay:
                        if self.plateau[y][dx] is not None:
                            return False
                        y += pas
                else:
                    pas = 1 if delta_x > 0 else -1
                    x = dx + pas
                    while x != ax:
                        if self.plateau[dy][x] is not None:
                            return False
                        x += pas
                return True
            elif abs(delta_x) == abs(delta_y):  # Mouvement comme un fou
                pas_x = 1 if delta_x > 0 else -1
                pas_y = 1 if delta_y > 0 else -1
                x, y = dx + pas_x, dy + pas_y
                while x != ax:
                    if self.plateau[y][x] is not None:
                        return False
                    x += pas_x
                    y += pas_y
                return True
            return False

        elif isinstance(piece, Roi):
            # Le roi se déplace d'une case dans n'importe quelle direction
            return abs(delta_x) <= 1 and abs(delta_y) <= 1

        return False

    def obtenir_pieces(self, couleur):
        pieces = []
        for i in range(8):
            for j in range(8):
                piece = self.plateau[i][j]
                if piece and piece.color == couleur:
                    pieces.append((i, j, piece))
        return pieces

    def obtenir_coups_possibles(self, pos_y, pos_x):
        piece = self.plateau[pos_y][pos_x]
        if not piece:
            return []

        coups_possibles = []
        for y in range(8):
            for x in range(8):
                depart = chr(pos_x + ord('a')) + str(8 - pos_y)
                arrivee = chr(x + ord('a')) + str(8 - y)
                if self.est_mouvement_valide(depart, arrivee):
                    # Calculer le score du coup
                    score = 0
                    piece_cible = self.plateau[y][x]
                    if piece_cible:
                        # Valeurs des pièces
                        valeurs = {
                            Pion: 1,
                            Cavalier: 3,
                            Fou: 3,
                            Tour: 5,
                            Dame: 9,
                            Roi: 0  # Le roi a une valeur spéciale
                        }
                        score = valeurs[type(piece_cible)]
                    
                    # Bonus pour le contrôle du centre
                    if 2 <= y <= 5 and 2 <= x <= 5:
                        score += 0.5

                    coups_possibles.append((depart, arrivee, score))

        return coups_possibles

    def coup_ia(self):
        pieces_noires = self.obtenir_pieces('noir')
        tous_coups = []
        
        for y, x, piece in pieces_noires:
            coups = self.obtenir_coups_possibles(y, x)
            tous_coups.extend(coups)

        if tous_coups:
            # Trier les coups par score et prendre un des meilleurs (aléatoirement parmi les 3 meilleurs)
            tous_coups.sort(key=lambda x: x[2], reverse=True)
            meilleurs_coups = tous_coups[:3]
            coup_choisi = random.choice(meilleurs_coups)
            return coup_choisi[0], coup_choisi[1]
        return None

    def trouver_roi(self, couleur):
        for y in range(8):
            for x in range(8):
                piece = self.plateau[y][x]
                if isinstance(piece, Roi) and piece.color == couleur:
                    return y, x
        return None

    def est_en_echec(self, couleur):
        # Trouver la position du roi
        pos_roi = self.trouver_roi(couleur)
        if not pos_roi:
            return False
        
        roi_y, roi_x = pos_roi
        couleur_adverse = 'noir' if couleur == 'blanc' else 'blanc'
        
        # Vérifier si une pièce adverse peut atteindre le roi
        for y in range(8):
            for x in range(8):
                piece = self.plateau[y][x]
                if piece and piece.color == couleur_adverse:
                    depart = chr(x + ord('a')) + str(8 - y)
                    arrivee = chr(roi_x + ord('a')) + str(8 - roi_y)
                    # Temporairement changer le tour pour vérifier le mouvement
                    tour_original = self.tour_blanc
                    self.tour_blanc = (couleur_adverse == 'blanc')
                    est_valide = self.est_mouvement_valide(depart, arrivee)
                    self.tour_blanc = tour_original
                    if est_valide:
                        return True
        return False

    def promouvoir_pion(self, x, y):
        piece = self.plateau[y][x]
        if not isinstance(piece, Pion):
            return
            
        # Vérifier si le pion a atteint la dernière rangée
        if (piece.color == 'blanc' and y == 0) or (piece.color == 'noir' and y == 7):
            # En mode IA, promouvoir automatiquement en Dame
            if self.mode_ia and piece.color == 'noir':
                self.plateau[y][x] = Dame('noir')
            else:
                print("\nPromotion du pion!")
                print("Choisissez la pièce (D: Dame, T: Tour, F: Fou, C: Cavalier): ")
                while True:
                    choix = input().upper()
                    if choix in ['D', 'T', 'F', 'C']:
                        if choix == 'D':
                            self.plateau[y][x] = Dame(piece.color)
                        elif choix == 'T':
                            self.plateau[y][x] = Tour(piece.color)
                        elif choix == 'F':
                            self.plateau[y][x] = Fou(piece.color)
                        elif choix == 'C':
                            self.plateau[y][x] = Cavalier(piece.color)
                        break
                    print("Choix invalide! Utilisez D, T, F ou C.")

    def deplacer(self, depart, arrivee):
        if not self.est_mouvement_valide(depart, arrivee):
            return False

        # Conversion des coordonnées
        dx, dy = ord(depart[0]) - ord('a'), 8 - int(depart[1])
        ax, ay = ord(arrivee[0]) - ord('a'), 8 - int(arrivee[1])

        # Sauvegarder le coup dans l'historique
        piece_capturee = self.plateau[ay][ax]
        self.historique_coups.append({
            'depart': depart,
            'arrivee': arrivee,
            'piece': self.plateau[dy][dx],
            'capture': piece_capturee
        })

        # Effectuer le déplacement
        self.plateau[ay][ax] = self.plateau[dy][dx]
        self.plateau[dy][dx] = None
        self.plateau[ay][ax].has_moved = True

        # Vérifier la promotion des pions
        self.promouvoir_pion(ax, ay)

        # Vérifier si le roi est en échec après le mouvement
        couleur_active = 'blanc' if self.tour_blanc else 'noir'
        self.en_echec = self.est_en_echec(couleur_active)
        
        if self.en_echec:
            print(f"\nÉchec au {'Roi blanc' if couleur_active == 'blanc' else 'Roi noir'}!")

        # Changer de tour
        self.tour_blanc = not self.tour_blanc
        return True

def main():
    jeu = Echiquier(mode_ia=True)
    print("\nBienvenue au jeu d'échecs!")
    print("Vous jouez les Blancs contre l'ordinateur")
    print("Pour jouer, entrez les coordonnées de départ et d'arrivée (ex: e2 e4)")
    print("Pour quitter, tapez 'quit'\n")

    while jeu.en_cours:
        jeu.afficher()
        
        if jeu.tour_blanc:
            coup = input("\nEntrez votre coup (ex: e2 e4) : ")
            
            if coup.lower() == 'quit':
                break
                
            try:
                depart, arrivee = coup.split()
                if not jeu.deplacer(depart, arrivee):
                    print("Coup invalide! Réessayez.")
                    continue
            except ValueError:
                print("Format invalide! Utilisez le format: e2 e4")
                continue
        else:
            print("\nL'ordinateur réfléchit...")
            time.sleep(1)  # Pause pour simulation de réflexion
            coup_ia = jeu.coup_ia()
            if coup_ia:
                depart, arrivee = coup_ia
                print(f"L'ordinateur joue : {depart} {arrivee}")
                jeu.deplacer(depart, arrivee)
            else:
                print("L'ordinateur ne peut plus jouer!")
                break

if __name__ == "__main__":
    main() 