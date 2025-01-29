from z3 import Int, Solver, And, Distinct, sat
from timeit import default_timer

def solve_sudoku_with_z3(grid):
    """
    Résout un Sudoku en utilisant le solveur Z3.
    Retourne la grille résolue ou None si pas de solution.
    """
    # Configuration du solveur avec un timeout de 10 secondes
    solver = Solver()
    solver.set("timeout", 10000)  # Timeout de 10 secondes

    # Création des variables pour la grille
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]

    # Ajout des contraintes de base
    for i in range(9):
        for j in range(9):
            solver.add(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # Ajout des contraintes de ligne
    for i in range(9):
        solver.add(Distinct(cells[i]))

    # Ajout des contraintes de colonne
    for j in range(9):
        solver.add(Distinct([cells[i][j] for i in range(9)]))

    # Précalcule des boîtes 3x3 pour une meilleure performance
    boxes = [
        [(i*3 + di, j*3 + dj) for di in range(3) for dj in range(3)]
        for i in range(3) for j in range(3)
    ]

    # Ajout des contraintes de boîtes
    for box in boxes:
        solver.add(Distinct([cells[i][j] for i, j in box]))

    # Ajout des valeurs initiales
    for i in range(9):
        for j in range(9):
            if grid[i][j] != 0:
                solver.add(cells[i][j] == grid[i][j])

    # Mesure du temps de résolution
    start_time = default_timer()
    
    # Résolution
    if solver.check() == sat:
        model = solver.model()
        solved_grid = [
            [
                model[cells[i][j]].as_long() 
                for j in range(9)
            ] 
            for i in range(9)
        ]
        end_time = default_timer()
        print(f"Solution trouvée en {end_time - start_time:.2f} secondes")
        return solved_grid
    else:
        print("Aucune solution trouvée.")
        return None

# Exemple d'utilisation
grille = [
    [0, 0, 0, 0, 9, 4, 0, 3, 0],
    [0, 0, 0, 5, 1, 0, 0, 0, 7],
    [0, 8, 9, 0, 0, 0, 0, 4, 0],
    [0, 0, 0, 0, 0, 0, 2, 0, 8],
    [0, 6, 0, 2, 0, 1, 0, 5, 0],
    [1, 0, 2, 0, 0, 0, 0, 0, 0],
    [0, 7, 0, 0, 0, 0, 5, 2, 0],
    [9, 0, 0, 0, 6, 5, 0, 0, 0],
    [0, 4, 0, 9, 7, 0, 0, 0, 0],
]

solution = solve_sudoku_with_z3(grille)
if solution:
    for ligne in solution:
        print(ligne)