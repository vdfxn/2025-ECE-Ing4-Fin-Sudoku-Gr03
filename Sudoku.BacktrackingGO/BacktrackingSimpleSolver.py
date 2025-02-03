import numpy as np
from timeit import default_timer

def find_empty_cell(grid):
    """
    Trouve la prochaine cellule vide en utilisant l'heuristique MRV (Minimum Remaining Values).
    Retourne les coordonnées (row, col) de la meilleure cellule vide ou None si la grille est complète.
    """
    min_options = 10  # Nombre maximum de possibilités (1-9)
    best_cell = None

    for row in range(9):
        for col in range(9):
            if grid[row, col] == 0:
                possible_values = {i for i in range(1, 10)} - set(grid[row]) - set(grid[:, col]) - set(
                    grid[row // 3 * 3:row // 3 * 3 + 3, col // 3 * 3:col // 3 * 3 + 3].flatten()
                )
                num_options = len(possible_values)
                
                if num_options < min_options:
                    min_options = num_options
                    best_cell = (row, col)

    return best_cell

def is_valid(grid, row, col, num):
    """
    Vérifie si un nombre `num` peut être placé dans la cellule (row, col) en respectant les règles du Sudoku.
    """
    if num in grid[row] or num in grid[:, col]:  # Vérifie la ligne et la colonne
        return False

    start_row, start_col = 3 * (row // 3), 3 * (col // 3)
    if num in grid[start_row:start_row + 3, start_col:start_col + 3]:
        return False  # Vérifie le bloc 3x3
    
    return True

def solve_sudoku(grid):
    """
    Résout un Sudoku en utilisant l'algorithme de backtracking optimisé avec MRV.
    Retourne True si la solution est trouvée, sinon False.
    """
    empty_cell = find_empty_cell(grid)
    if not empty_cell:
        return True  # Toutes les cases sont remplies, Sudoku résolu

    row, col = empty_cell

    for num in range(1, 10):
        if is_valid(grid, row, col, num):
            grid[row, col] = num

            if solve_sudoku(grid):
                return True  # Solution trouvée

            grid[row, col] = 0  # Annule le choix (backtracking)

    return False  # Aucune solution trouvée

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

if solve_sudoku(instance):
    result = instance  #  Définit `result` pour être récupéré par Python.NET
else:
    result = None  #  Définit `result` à `None` si aucune solution


start = default_timer()
# Exécuter la résolution de Sudoku
if solve_sudoku(instance):
    print("\n :D Sudoku résolu :")
    print(instance)
else:
    print("\n :'( Aucune solution trouvée.")

execution = default_timer() - start
print(f"\n Temps de résolution : {execution * 1000:.2f} ms")
