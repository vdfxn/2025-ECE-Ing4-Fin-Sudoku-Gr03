using Google.OrTools.Sat;
using Sudoku.Shared;
using System;
using System.Collections.Generic;

namespace Sudoku.ORToolsSolver
{
    public class ORToolsSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid grid)
        {
            CpModel model = new CpModel();
            IntVar[,] cells = new IntVar[9, 9];

            // Define variables (possible values from 1 to 9 for each cell)
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    cells[row, col] = model.NewIntVar(1, 9, $"cell_{row}_{col}");

                    // If the cell is already filled (non-zero), enforce that value
                    if (grid.Cells[row, col] != 0)
                    {
                        model.Add(cells[row, col] == grid.Cells[row, col]);
                    }
                }
            }

            // Add row constraints (each row must contain digits 1 to 9 without repetition)
            for (int row = 0; row < 9; row++)
            {
                List<IntVar> rowVars = new List<IntVar>();
                for (int col = 0; col < 9; col++)
                {
                    rowVars.Add(cells[row, col]);
                }
                model.AddAllDifferent(rowVars); // Constraints for the row
            }

            // Add column constraints (each column must contain digits 1 to 9 without repetition)
            for (int col = 0; col < 9; col++)
            {
                List<IntVar> colVars = new List<IntVar>();
                for (int row = 0; row < 9; row++)
                {
                    colVars.Add(cells[row, col]);
                }
                model.AddAllDifferent(colVars); // Constraints for the column
            }

            // Add 3x3 subgrid constraints (each subgrid must contain digits 1 to 9 without repetition)
            for (int row = 0; row < 9; row += 3)
            {
                for (int col = 0; col < 9; col += 3)
                {
                    List<IntVar> blockVars = new List<IntVar>();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            blockVars.Add(cells[row + i, col + j]);
                        }
                    }
                    model.AddAllDifferent(blockVars); // Constraints for the 3x3 block
                }
            }

            // Solve the model using OR-Tools
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);

            // If a solution is found, update the grid with the solved values
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        grid.Cells[row, col] = (int)solver.Value(cells[row, col]);
                    }
                }
            }
            else
            {
                Console.WriteLine("No solution found.");
            }

            return grid;
        }
    }
}