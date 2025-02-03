using System;
using Sudoku.Shared;

namespace Sudoku.SolverAmeliorer;

public class OptimizedSudokuSolver : ISudokuSolver
{
    private const int SIZE = 9;
    private bool[,] rowUsed;
    private bool[,] colUsed;
    private bool[,] boxUsed;

    public SudokuGrid Solve(SudokuGrid s)
    {
        if (s?.Cells == null || s.Cells.GetLength(0) != SIZE || s.Cells.GetLength(1) != SIZE)
        {
            throw new ArgumentException("Invalid Sudoku grid: Cells must be a 9x9 matrix.");
        }

        SudokuGrid result = new SudokuGrid { Cells = CopyGrid(s.Cells) };
        InitializeConstraints(result);
        
        if (!SolveSudoku(result, 0, 0))
        {
            throw new Exception("No solution found for the given Sudoku grid.");
        }
        return result;
    }

    private int[,] CopyGrid(int[,] original)
    {
        int[,] copy = new int[SIZE, SIZE];
        for (int i = 0; i < SIZE; i++)
            for (int j = 0; j < SIZE; j++)
                copy[i, j] = original[i, j];
        return copy;
    }

    private void InitializeConstraints(SudokuGrid grid)
    {
        rowUsed = new bool[SIZE, SIZE + 1];
        colUsed = new bool[SIZE, SIZE + 1];
        boxUsed = new bool[SIZE, SIZE + 1];

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                int num = grid.Cells[row, col];
                if (num != 0)
                {
                    rowUsed[row, num] = true;
                    colUsed[col, num] = true;
                    boxUsed[GetBoxIndex(row, col), num] = true;
                }
            }
        }
    }

    private bool SolveSudoku(SudokuGrid grid, int row, int col)
    {
        if (row == SIZE) return true;
        if (col == SIZE) return SolveSudoku(grid, row + 1, 0);
        if (grid.Cells[row, col] != 0) return SolveSudoku(grid, row, col + 1);

        for (int num = 1; num <= SIZE; num++)
        {
            if (IsValid(row, col, num))
            {
                grid.Cells[row, col] = num;
                SetConstraints(row, col, num, true);
                
                if (SolveSudoku(grid, row, col + 1)) return true;

                grid.Cells[row, col] = 0;
                SetConstraints(row, col, num, false);
            }
        }
        return false;
    }

    private bool IsValid(int row, int col, int num)
    {
        return num >= 1 && num <= SIZE && !rowUsed[row, num] && !colUsed[col, num] && !boxUsed[GetBoxIndex(row, col), num];
    }

    private void SetConstraints(int row, int col, int num, bool state)
    {
        rowUsed[row, num] = state;
        colUsed[col, num] = state;
        boxUsed[GetBoxIndex(row, col), num] = state;
    }

    private int GetBoxIndex(int row, int col)
    {
        return (row / 3) * 3 + (col / 3);
    }
}

