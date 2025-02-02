using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.Z3Solvers
{
    public class Z3SimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            using (var context = new Context())
            {
                // 1. Définir une matrice de variables entières représentant le sudoku.
                IntExpr[,] cells = new IntExpr[9, 9];
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        cells[row, col] = (IntExpr)context.MkConst($"cell_{row}_{col}", context.IntSort);
                    }
                }

                // Créez le solveur
                Solver solver = context.MkSolver();

                // 2. Contraintes : chaque cellule contient un nombre entre 1 et 9.
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        solver.Assert(context.MkAnd(
                            context.MkLe(context.MkInt(1), cells[row, col]), // cell >= 1
                            context.MkLe(cells[row, col], context.MkInt(9))   // cell <= 9
                        ));
                    }
                }

                // Ajout des contraintes spécifiques au puzzle donné.
                for (int row = 0; row < s.Cells.GetLength(0); row++)
                {
                    for (int col = 0; col < s.Cells.GetLength(1); col++)
                    {
                        if (s.Cells[row, col] != 0)
                        {
                            solver.Assert(context.MkEq(cells[row, col], context.MkInt(s.Cells[row, col])));
                        }
                    }
                }

                
                // 3. Contraintes : chaque ligne contient des nombres uniques de 1 à 9.
                for (int row = 0; row < 9; row++)
                {
                    solver.Assert(context.MkDistinct(Enumerable.Range(0, 9).Select(col => cells[row, col]).ToArray()));
                }

                // 4. Contraintes : chaque colonne contient des nombres uniques de 1 à 9.
                for (int col = 0; col < 9; col++)
                {
                    solver.Assert(context.MkDistinct(Enumerable.Range(0, 9).Select(row => cells[row, col]).ToArray()));
                }

                // 5. Contraintes : chaque bloc de taille (3x3) contient des nombres uniques de 1 à 9.
                for (int blockRow = 0; blockRow < 3; blockRow++)
                {
                    for (int blockCol = 0; blockCol < 3; blockCol++)
                    {
                        var blockCells = new List<IntExpr>();

                        for (int i = blockRow * 3; i < (blockRow + 1) * 3; i++)
                        {
                            for (int j = blockCol * 3; j < (blockCol + 1) * 3; j++)
                            {
                                blockCells.Add(cells[i, j]);
                            }
                        }
                        solver.Assert(context.MkDistinct(blockCells.ToArray()));
                    }
                }

                // Résolution du Sudoku
                if (solver.Check() == Status.SATISFIABLE)
                {
                    // Extraire une solution satisfaisante
                    Model model = solver.Model;
                    SudokuGrid solvedSudoku = new SudokuGrid();
                    
                    for (int row = 0; row < 9; row++)
                    {
                        for (int col = 0; col < 9; col++)
                        {
                            int value = ((IntNum)model.Evaluate(cells[row, col])).Int;
                            solvedSudoku.Cells[row, col] = value;
                        }
                    }

                    return solvedSudoku;
                }
                else
                {
                    throw new Exception("Le Sudoku donné ne peut pas être résolu.");
                }
            }
        }
    }
}