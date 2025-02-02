using System;
using Sudoku.Shared;

namespace Sudoku.JM;

public class DynamicProgrammingSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        if (s?.Cells == null || s.Cells.Length != 81)
        {
            throw new ArgumentException("Invalid Sudoku grid: Cells must be a 9x9 matrix.");
        }
        
        SudokuGrid result = new SudokuGrid { Cells = (int[,])s.Cells.Clone() };
        if (!SolveSudoku(result))
        {
            throw new Exception("No solution found for the given Sudoku grid.");
        }
        return result;
    }

    private bool SolveSudoku(SudokuGrid grid)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (grid.Cells[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsValid(grid, row, col, num))
                        {
                            grid.Cells[row, col] = num;

                            if (SolveSudoku(grid))
                                return true;

                            grid.Cells[row, col] = 0; // Backtrack
                        }
                    }
                    return false;
                }
            }
        }
        return true; // Solution found
    }

    private bool IsValid(SudokuGrid grid, int row, int col, int num)
    {
        if (grid?.Cells == null || row < 0 || row >= 9 || col < 0 || col >= 9)
        {
            throw new ArgumentOutOfRangeException($"Index out of range: row={row}, col={col}");
        }
        
        for (int i = 0; i < 9; i++)
        {
            if (grid.Cells[row, i] == num || grid.Cells[i, col] == num)
                return false;
        }
        
        int startRow = row - row % 3, startCol = col - col % 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int checkRow = i + startRow;
                int checkCol = j + startCol;
                if (checkRow < 9 && checkCol < 9 && grid.Cells[checkRow, checkCol] == num)
                    return false;
            }
        }
        return true;
    }
}
