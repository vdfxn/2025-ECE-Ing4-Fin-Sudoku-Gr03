using Sudoku.Shared;
using System;
using System.Collections.Generic;

namespace SolverBacktrackingSimple
{
    public class SolverBacktrackingSimple : ISudokuSolver
    {
        private const int SIZE = 9; // Taille de la grille de Sudoku
        private const int EMPTY = 0; // Valeur représentant une case vide

        public SudokuGrid Solve(SudokuGrid s)
        {
            int[] grid = ConvertGridToArray(s);
            if (Solve(grid))
            {
                return ConvertArrayToGrid(grid);
            }
            else
            {
                throw new InvalidOperationException("No solution exists for the given Sudoku grid");
            }
        }

        private bool Solve(int[] grid)
        {
            for (int i = 0; i < SIZE * SIZE; i++)
            {
                if (grid[i] == EMPTY)
                {
                    List<int> possibleNumbers = GetPossibleNumbers(i, grid);
                    foreach (int num in possibleNumbers)
                    {
                        grid[i] = num;
                        if (Solve(grid))
                        {
                            return true;
                        }
                        grid[i] = EMPTY;
                    }
                    return false;
                }
            }
            return true;
        }

        private List<int> GetPossibleNumbers(int index, int[] grid)
        {
            if (grid[index] != EMPTY)
            {
                return new List<int>();
            }
            HashSet<int> possibleNumbers = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            possibleNumbers.ExceptWith(GetRowNumbers(index, grid));
            possibleNumbers.ExceptWith(GetColumnNumbers(index, grid));
            possibleNumbers.ExceptWith(GetSquareNumbers(index, grid));
            return new List<int>(possibleNumbers);
        }

        private List<int> GetRowNumbers(int index, int[] grid)
        {
            int row = index / SIZE;
            List<int> numbers = new List<int>();
            for (int i = 0; i < SIZE; i++)
            {
                int cell = grid[row * SIZE + i];
                if (cell != EMPTY)
                {
                    numbers.Add(cell);
                }
            }
            return numbers;
        }

        private List<int> GetColumnNumbers(int index, int[] grid)
        {
            int col = index % SIZE;
            List<int> numbers = new List<int>();
            for (int i = 0; i < SIZE; i++)
            {
                int cell = grid[i * SIZE + col];
                if (cell != EMPTY)
                {
                    numbers.Add(cell);
                }
            }
            return numbers;
        }

        private List<int> GetSquareNumbers(int index, int[] grid)
        {
            int startRow = (index / SIZE) / 3 * 3;
            int startCol = (index % SIZE) / 3 * 3;
            List<int> numbers = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int cell = grid[(startRow + i) * SIZE + (startCol + j)];
                    if (cell != EMPTY)
                    {
                        numbers.Add(cell);
                    }
                }
            }
            return numbers;
        }

        private int[] ConvertGridToArray(SudokuGrid s)
        {
            int[] grid = new int[SIZE * SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    grid[i * SIZE + j] = s.Cells[i, j];
                }
            }
            return grid;
        }

        private SudokuGrid ConvertArrayToGrid(int[] grid)
        {
            SudokuGrid s = new SudokuGrid();
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    s.Cells[i, j] = grid[i * SIZE + j];
                }
            }
            return s;
        }
    }
}
