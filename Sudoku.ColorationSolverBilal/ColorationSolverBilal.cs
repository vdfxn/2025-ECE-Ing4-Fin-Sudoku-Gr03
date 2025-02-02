namespace Sudoku.ColorationSolverBilal;

using System;
using System.Collections.Generic;
using System.Linq;

public class ColorationSolverBilal
{
    private const int Size = 9;
    private int[,] board;
    private Dictionary<(int, int), HashSet<int>> domains;

    public ColorationSolverBilal(int[,] initialBoard)
    {
        board = initialBoard;
        domains = new Dictionary<(int, int), HashSet<int>>();
        InitializeDomains();
    }

    private void InitializeDomains()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                if (board[row, col] == 0)
                {
                    domains[(row, col)] = GetPossibleValues(row, col);
                }
            }
        }
    }

    private HashSet<int> GetPossibleValues(int row, int col)
    {
        HashSet<int> possibleValues = new HashSet<int>(Enumerable.Range(1, 9));

        for (int i = 0; i < Size; i++)
        {
            possibleValues.Remove(board[row, i]);
            possibleValues.Remove(board[i, col]);
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                possibleValues.Remove(board[startRow + i, startCol + j]);
            }
        }
        return possibleValues;
    }

    public bool Solve()
    {
        return ColorSudoku();
    }

    private bool ColorSudoku()
    {
        var emptyCell = domains.OrderBy(d => d.Value.Count).FirstOrDefault();
        if (emptyCell.Key == default)
        {
            return true;
        }
        
        int row = emptyCell.Key.Item1;
        int col = emptyCell.Key.Item2;
        foreach (int value in emptyCell.Value)
        {
            board[row, col] = value;
            domains.Remove((row, col));
            if (IsValidAssignment(row, col, value) && ColorSudoku())
            {
                return true;
            }
            board[row, col] = 0;
            domains[(row, col)] = GetPossibleValues(row, col);
        }
        return false;
    }

    private bool IsValidAssignment(int row, int col, int value)
    {
        for (int i = 0; i < Size; i++)
        {
            if (board[row, i] == value && i != col)
                return false;
            if (board[i, col] == value && i != row)
                return false;
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[startRow + i, startCol + j] == value && (startRow + i != row || startCol + j != col))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void PrintBoard()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Console.Write(board[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
}
