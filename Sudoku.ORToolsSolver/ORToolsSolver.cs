using Sudoku.Shared;

namespace Sudoku.ORToolsSolvers
{
    public class ORToolsSimpleSolvers : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid grid)
        {
            // Ici, vous pouvez mettre la logique pour résoudre un Sudoku
            // Pour l'instant, cela renvoie simplement le grid sans rien changer
            return grid;
        }
    }
}
