from z3 import Solver, Int, And, Distinct
import numpy as np
from timeit import default_timer

def solve_sudoku_with_z3(grid):
    # Initialisation du solveur Z3
    solver = Solver()

    # Création des variables Z3 pour chaque cellule de la grille Sudoku (9x9)
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]

    # 1. Contraintes : Chaque cellule doit contenir un nombre entre 1 et 9.
    for i in range(9):
        for j in range(9):
            solver.add(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # 2. Contraintes : Chaque ligne contient des nombres uniques.
    for i in range(9):
        solver.add(Distinct(cells[i]))

    # 3. Contraintes : Chaque colonne contient des nombres uniques.
    for j in range(9):
        solver.add(Distinct([cells[i][j] for i in range(9)]))

    # 4. Contraintes : Chaque bloc de taille (3x3) contient des nombres uniques.
    for block_row in range(0, 9, 3):       # Parcours par blocs de lignes
        for block_col in range(0, 9, 3):   # Parcours par blocs de colonnes
            block = []
            for i in range(block_row, block_row + 3):   # Lignes du bloc actuel
                for j in range(block_col, block_col + 3):   # Colonnes du bloc actuel
                    block.append(cells[i][j])
            solver.add(Distinct(block))   # Les valeurs dans le bloc doivent être distinctes

     
    # 5. Contraintes : Intégrer les valeurs fixes du puzzle donné.
    for i in range(9):
        for j in range(9):
            if grid[i, j] != 0:  # Si une cellule est déjà remplie dans le puzzle
                solver.add(cells[i][j] == grid[i, j])

    # Résolution du Sudoku par Z3
    if solver.check() == 'sat':  # "satisfiable" signifie qu'une solution existe
        model = solver.model()

        # Construire la grille résolue en récupérant les valeurs depuis le modèle Z3
        solved_grid = np.zeros((9, 9), dtype=int)
        for i in range(9):
            for j in range(9):
                solved_grid[i, j] = model.evaluate(cells[i][j]).as_long()
        
        return solved_grid
    else:
        print("Aucune solution trouvée.")
        return None

# Définir `instance` uniquement si non déjà défini par PythonNET
if 'instance' not in locals():
    instance = np.array([
        [0,0,0,0,9,4,0,3,0],
        [0,0,0,5,1,0,0,0,7],
        [0,8,9,0,0,0,0,4,0],
        [0,0,0,0,0,0,2,0,8],
        [0,6,0,2,0,1,0,5,0],
        [1,0,2,0,0,0,0,0,0],
        [0,7,0,0,0,0,5,2,0],
        [9,0,0,0,6,5,0,0,0],
        [0,4,0,9,7,0,0,0,0]
    ], dtype=int)

def is_valid_sudoku(grid):
    # Vérifier lignes et colonnes
    for i in range(9):
        if len(set(grid[i]) - {0}) != len([x for x in grid[i] if x != 0]):
            return False
        col = [grid[row][i] for row in range(9)]
        if len(set(col) - {0}) != len([x for x in col if x != 0]):
            return False

    # Vérifier blocs (sous-grilles)
    for block_row in range(0, 9, 3):
        for block_col in range(0, 9, 3):
            block = []
            for i in range(block_row, block_row + 3):
                for j in range(block_col, block_col + 3):
                    if grid[i][j] != 0:
                        block.append(grid[i][j])
            if len(set(block)) != len(block):
                return False

    return True

# Exemple d'utilisation avant résolution avec Z³.
if not is_valid_sudoku(instance.tolist()):
    print("La grille initiale comporte déjà une contradiction.")


start = default_timer()
# Exécuter la résolution de Sudoku
if solve_sudoku_with_z3(instance):
    # print("Sudoku résolu par backtracking avec succès.")
    result = instance  # `result` sera utilisé pour récupérer la grille résolue depuis C#
else:
    print("Aucune solution trouvée.")
execution = default_timer() - start
print("Le temps de résolution est de : ", execution * 1000, " ms")