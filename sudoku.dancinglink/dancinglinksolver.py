import sys
from dlxsudoku.dancing_links import DancingLinksSolver
from dlxsudoku.sudoku import Sudoku

def main():
    # Charger la grille de Sudoku
    sudoku_file = "2025-ECE-Ing4-Fin-Sudoku-Gr03/sudoku.dancinglink/tests/premier.sud"
    sudoku = Sudoku.from_file(sudoku_file)
    
    # Convertir la grille en problème exact cover
    dlx_matrix = sudoku.to_exact_cover()
    solver = DancingLinksSolver(dlx_matrix)
    
    # Trouver une solution
    solution = solver.solve()
    
    if solution:
        solved_sudoku = sudoku.apply_solution(solution)
        print("Solution trouvée:")
        print(solved_sudoku)
    else:
        print("Aucune solution trouvée.")

if __name__ == "__main__":
    main()
