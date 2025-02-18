using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.ColorSolverBilal
{
    public class ColorSolverBilal : ISudokuSolver
    {
        private const int Size = 9;
        private const int SubgridSize = 3;

        public SudokuGrid Solve(SudokuGrid sudoku)
        {
            int[,] board = (int[,])sudoku.Cells.Clone();
            Dictionary<(int, int), int> domains = InitializeDomains(board);

            if (SolveWithBacktracking(board, domains))
            {
                sudoku.Cells = board;
            }

            return sudoku;
        }

        private Dictionary<(int, int), int> InitializeDomains(int[,] board)
        {
            var domains = new Dictionary<(int, int), int>();

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (board[row, col] == 0)
                    {
                        domains[(row, col)] = GetPossibleValues(board, row, col);
                    }
                }
            }
            return domains;
        }

        private int GetPossibleValues(int[,] board, int row, int col)
        {
            int possibleValues = 0b111111111; // Toutes les valeurs de 1 à 9 sont possibles

            for (int i = 0; i < Size; i++)
            {
                if (board[row, i] != 0) possibleValues &= ~(1 << (board[row, i] - 1)); // Ligne
                if (board[i, col] != 0) possibleValues &= ~(1 << (board[i, col] - 1)); // Colonne
            }

            int startRow = (row / SubgridSize) * SubgridSize;
            int startCol = (col / SubgridSize) * SubgridSize;

            for (int i = 0; i < SubgridSize; i++)
            {
                for (int j = 0; j < SubgridSize; j++)
                {
                    if (board[startRow + i, startCol + j] != 0)
                    {
                        possibleValues &= ~(1 << (board[startRow + i, startCol + j] - 1)); // Bloc
                    }
                }
            }

            return possibleValues;
        }

        private bool SolveWithBacktracking(int[,] board, Dictionary<(int, int), int> domains)
        {
            if (!domains.Any()) return true;

            var cell = domains.OrderBy(d => BitCount(d.Value)).First();
            int row = cell.Key.Item1;
            int col = cell.Key.Item2;
            int possibleValues = cell.Value;

            for (int value = 1; value <= Size; value++)
            {
                if ((possibleValues & (1 << (value - 1))) != 0)
                {
                    if (IsValidAssignment(board, row, col, value))
                    {
                        board[row, col] = value;
                        var affectedDomains = ForwardCheck(domains, row, col, value);

                        if (affectedDomains != null && SolveWithBacktracking(board, domains))
                        {
                            return true;
                        }

                        board[row, col] = 0;
                        RestoreDomains(domains, affectedDomains);
                    }
                }
            }

            return false;
        }

        private Dictionary<(int, int), int> ForwardCheck(Dictionary<(int, int), int> domains, int row, int col, int value)
        {
            var affected = new Dictionary<(int, int), int>();

            foreach (var key in domains.Keys.ToList())
            {
                if (key.Item1 == row || key.Item2 == col || (key.Item1 / SubgridSize == row / SubgridSize && key.Item2 / SubgridSize == col / SubgridSize))
                {
                    if ((domains[key] & (1 << (value - 1))) != 0)
                    {
                        affected[key] = domains[key];
                        domains[key] &= ~(1 << (value - 1));

                        if (domains[key] == 0)
                        {
                            RestoreDomains(domains, affected);
                            return null;
                        }
                    }
                }
            }

            return affected;
        }

        private void RestoreDomains(Dictionary<(int, int), int> domains, Dictionary<(int, int), int> affected)
        {
            if (affected == null) return;

            foreach (var key in affected.Keys)
            {
                domains[key] = affected[key];
            }
        }

        private bool IsValidAssignment(int[,] board, int row, int col, int value)
        {
            for (int i = 0; i < Size; i++)
            {
                if (board[row, i] == value) return false;
                if (board[i, col] == value) return false;
            }

            int startRow = (row / SubgridSize) * SubgridSize;
            int startCol = (col / SubgridSize) * SubgridSize;
            for (int i = 0; i < SubgridSize; i++)
            {
                for (int j = 0; j < SubgridSize; j++)
                {
                    if (board[startRow + i, startCol + j] == value) return false;
                }
            }

            return true;
        }

        private int BitCount(int value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }
    }
}