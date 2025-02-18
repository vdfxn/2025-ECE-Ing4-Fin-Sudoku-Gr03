using Google.OrTools.Sat;
using Sudoku.Shared;

namespace Sudoku.Constraint
{
    public class ConstraintSimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            CpModel model = new CpModel();
            IntVar[,] grid = new IntVar[9, 9];
            
            // Déclaration des variables avec domaine {1-9}
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    grid[i, j] = model.NewIntVar(1, 9, $"cell_{i}_{j}");
                }
            }
            
            // Contraintes de lignes et colonnes
            for (int i = 0; i < 9; i++)
            {
                model.AddAllDifferent(GetRow(grid, i));
                model.AddAllDifferent(GetColumn(grid, i));
            }
            
            // Contraintes des sous-grilles 3x3
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    model.AddAllDifferent(GetSubGrid(grid, i * 3, j * 3));
                }
            }
            
            // Appliquer la grille initiale
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        model.Add(grid[i, j] == s.Cells[i, j]);
                    }
                }
            }
            
            // Résolution
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);
            
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        s.Cells[i, j] = (int)solver.Value(grid[i, j]);
                    }
                }
            }
            
            return s;
        }
        
        private IntVar[] GetRow(IntVar[,] grid, int row)
        {
            return Enumerable.Range(0, 9).Select(j => grid[row, j]).ToArray();
        }
        
        private IntVar[] GetColumn(IntVar[,] grid, int col)
        {
            return Enumerable.Range(0, 9).Select(i => grid[i, col]).ToArray();
        }
        
        private IntVar[] GetSubGrid(IntVar[,] grid, int row, int col)
        {
            return Enumerable.Range(0, 3)
                .SelectMany(i => Enumerable.Range(0, 3).Select(j => grid[row + i, col + j]))
                .ToArray();
        }
    }
}
