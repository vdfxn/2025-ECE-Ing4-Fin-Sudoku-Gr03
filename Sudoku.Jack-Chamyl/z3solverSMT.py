from itertools import product
from z3 import Solver, Int, And, Distinct, sat
from timeit import default_timer

def solve_sudoku_smt(grid):
    """
    Résout un Sudoku 9x9 en utilisant une approche SMT avec des variables entières.
    
    grid : itérable 9x9 (tuple ou liste de tuples), 
           0 représente une case vide, sinon un entier entre 1 et 9.
    
    Retourne la grille solution sous forme de liste 9x9, ou None si aucune solution n'est trouvée.
    """
    # Création du solveur SMT
    solver = Solver()
    solver.set(timeout=10_000)  # Timeout de 10 secondes
    solver.set(threads=4)       # Exploiter 4 cœurs si possible

    # Déclaration des variables entières : cell[i][j] représente la valeur de la case (i,j)
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]
    constraints = []

    # 1. Chaque case doit contenir une valeur entre 1 et 9
    for i, j in product(range(9), range(9)):
        constraints.append(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # 2. Chaque ligne doit comporter des valeurs distinctes
    for i in range(9):
        constraints.append(Distinct(cells[i]))

    # 3. Chaque colonne doit comporter des valeurs distinctes
    for j in range(9):
        col = [cells[i][j] for i in range(9)]
        constraints.append(Distinct(col))

    # 4. Chaque bloc 3x3 doit comporter des valeurs distinctes
    for block_i in range(3):
        for block_j in range(3):
            block = [cells[3 * block_i + di][3 * block_j + dj] for di in range(3) for dj in range(3)]
            constraints.append(Distinct(block))

    # 5. Contraintes pour les cases déjà remplies (indices donnés)
    for i, j in product(range(9), range(9)):
        if grid[i][j] != 0:
            constraints.append(cells[i][j] == grid[i][j])

    # Ajout de toutes les contraintes au solveur
    solver.add(*constraints)

    # Mesure du temps de résolution
    start_time = default_timer()
    result = solver.check()
    end_time = default_timer()
    solve_duration = (end_time - start_time) * 1000  # en millisecondes

    if result == sat:
        print(f"Solution trouvée en {solve_duration:.2f} ms")
        model = solver.model()
        # Reconstruction de la grille solution en évaluant chaque variable
        solution = [[model.evaluate(cells[i][j]).as_long() for j in range(9)] for i in range(9)]
        return solution
    else:
        print("Aucune solution trouvée.")
        return None

# -------------------------------------------------------------------------
# Exemple de Sudoku
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

# Chronométrage global (construction + résolution)
t0 = default_timer()
result = solve_sudoku_smt(instance)
t1 = default_timer()
total_ms = (t1 - t0) * 1000

if result:
    print("Sudoku résolu :")
    for row in result:
        print(row)
    print(f"Temps total (construction + résolution + modélisation) = {total_ms:.2f} ms.")
else:
    print("Pas de solution.")