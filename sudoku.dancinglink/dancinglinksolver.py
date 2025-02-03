import sys
from dlxsudoku.dancing_links import DancingLinksSolver
from dlxsudoku.sudoku import Sudoku

def main():
    # Charger la grille de Sudoku
    sudoku_file = "sudoku.dancinglink/tests/medium.sud"
    sudoku = Sudoku.load_file(sudoku_file)
    
    # Convertir la grille en problème exact cover
    dlx_matrix = DancingLinksSolver._exact_cover()
    solver = DancingLinksSolver(dlx_matrix)
    
    # Trouver une solution
    solution = solver.solve()
    
    if solution:
        solved_sudoku = sudoku.solve(solution)
        print("Solution trouvée:")
        print(solved_sudoku)
    else:
        print("Aucune solution trouvée.")

if __name__ == "__main__":
    main()
