import time
from dlxsudoku import Sudoku

def load_grids(file_path):
    grids = []
    with open(file_path, 'r') as file:
        content = file.read().splitlines()

    current_grid = []
    
    for line in content:
        if line.startswith('Grid'):
            if current_grid:
                grids.append(''.join(current_grid))
            current_grid = []
        elif line.strip():
            current_grid.append(line.strip())
    
    if current_grid:
        grids.append(''.join(current_grid))
    
    return grids

def solve_grids(file_path):
    grids = load_grids(file_path)
    
    total_start_time = time.time()
    
    for index, grid in enumerate(grids, start=1):
        print(f"Résolution de la grille {index}:")
        sudoku = Sudoku(grid)
        
        sudoku.solve(verbose=False, allow_brute_force=True)
        
        print(sudoku)
        print("\n" + "-"*30 + "\n")
    
    total_end_time = time.time()
    total_time = total_end_time - total_start_time
    print(f"Temps total de résolution : {total_time:.4f} secondes")

solve_grids('Sudoku.PierreLouis/sudokus/grids_2.sud')