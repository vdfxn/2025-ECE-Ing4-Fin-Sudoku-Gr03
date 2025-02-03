from z3 import Solver, Int, And, Distinct, sat

def solve_sudoku(instance):
    """
    Résout un Sudoku 9x9 en utilisant une variable entière par case.

    Paramètre:
      instance: liste de 9 listes (chacune de 9 entiers) représentant la grille.
                La valeur 0 indique une case vide.

    Retourne:
      Un tuple (status, solution) où:
       - status est "sat" si une solution a été trouvée,
       - solution est une liste de listes (9x9) contenant la solution.
    """
    solver = Solver()

    # Création d'une matrice 9x9 de variables entières.
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]

    # Contrainte pour chaque cellule : valeur entre 1 et 9 (en une seule contrainte).
    for i in range(9):
        for j in range(9):
            solver.add(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # Contraintes sur les lignes : chaque ligne contient des valeurs distinctes.
    for i in range(9):
        solver.add(Distinct(cells[i]))

    # Contraintes sur les colonnes : chaque colonne contient des valeurs distinctes.
    for j in range(9):
        col = [cells[i][j] for i in range(9)]
        solver.add(Distinct(col))

    # Contraintes sur les blocs 3x3 : chaque bloc contient des valeurs distinctes.
    for bi in range(3):
        for bj in range(3):
            block = []
            for di in range(3):
                for dj in range(3):
                    block.append(cells[3 * bi + di][3 * bj + dj])
            solver.add(Distinct(block))

    # Appliquer les valeurs déjà données dans la grille d'entrée.
    for i in range(9):
        for j in range(9):
            val = instance[i][j]
            if val != 0:
                solver.add(cells[i][j] == val)

    # Résolution du problème
    if solver.check() == sat:
        model = solver.model()
        # Reconstruction de la solution sous forme d'une grille 9x9.
        solution = [[model.evaluate(cells[i][j]).as_long() for j in range(9)] for i in range(9)]
        return ("sat", solution)
    else:
        return ("unsat", None)

# Test local (lorsque le script est exécuté directement)
if __name__ == "__main__":
    instance = (
        (0, 0, 0, 0, 9, 4, 0, 3, 0),
        (0, 0, 0, 5, 1, 0, 0, 0, 7),
        (0, 8, 9, 0, 0, 0, 0, 4, 0),
        (0, 0, 0, 0, 0, 0, 2, 0, 8),
        (0, 6, 0, 2, 0, 1, 0, 5, 0),
        (1, 0, 2, 0, 0, 0, 0, 0, 0),
        (0, 7, 0, 0, 0, 0, 5, 2, 0),
        (9, 0, 0, 0, 6, 5, 0, 0, 0),
        (0, 4, 0, 9, 7, 0, 0, 0, 0),
    )
    status, sol = solve_sudoku(instance)
    if status == "sat":
        print("Sudoku résolu :")
        for row in sol:
            print(row)
    else:
        print("Pas de solution")