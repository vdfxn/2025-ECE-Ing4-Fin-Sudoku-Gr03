using Sudoku.Shared;

namespace Solver1;

public class SolverTest : ISudokuSolver
{
    private const int SIZE = 9;
    
    public SudokuGrid Solve(SudokuGrid s)
    {
        int[,] grid = ConvertTo2DArray(s.Cells);
        if (SolveSudoku(grid))
            s.Cells = ConvertTo1DArray(grid);
        return s;
    }

    private bool SolveSudoku(int[,] grid)
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (grid[row, col] == 0)
                {
                    for (int num = 1; num <= SIZE; num++)
                    {
                        if (IsValid(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            if (SolveSudoku(grid))
                                return true;
                            grid[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsValid(int[,] grid, int row, int col, int num)
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (grid[row, i] == num || grid[i, col] == num)
                return false;
        }
        
        int startRow = row - row % 3, startCol = col - col % 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[i + startRow, j + startCol] == num)
                    return false;
            }
        }
        return true;
    }

    private int[,] ConvertTo2DArray(int[] flatArray)
    {
        int[,] grid = new int[SIZE, SIZE];
        for (int i = 0; i < SIZE; i++)
            for (int j = 0; j < SIZE; j++)
                grid[i, j] = flatArray[i * SIZE + j];
        return grid;
    }

    private int[] ConvertTo1DArray(int[,] grid)
    {
        int[] flatArray = new int[SIZE * SIZE];
        for (int i = 0; i < SIZE; i++)
            for (int j = 0; j < SIZE; j++)
                flatArray[i * SIZE + j] = grid[i, j];
        return flatArray;
    }
}
