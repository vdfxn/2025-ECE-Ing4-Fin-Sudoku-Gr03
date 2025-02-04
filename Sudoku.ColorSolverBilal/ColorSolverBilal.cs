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
            Dictionary<(int, int), HashSet<int>> domains = InitializeDomains(board);

            if (SolveWithBacktracking(board, domains))
            {
                sudoku.Cells = board;
            }

            return sudoku;
        }

        private Dictionary<(int, int), HashSet<int>> InitializeDomains(int[,] board)
        {
            var domains = new Dictionary<(int, int), HashSet<int>>();

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

        private HashSet<int> GetPossibleValues(int[,] board, int row, int col)
        {
            var possibleValues = new HashSet<int>(Enumerable.Range(1, 9));
            
            for (int i = 0; i < Size; i++)
            {
                possibleValues.Remove(board[row, i]); // Ligne
                possibleValues.Remove(board[i, col]); // Colonne
            }

            int startRow = (row / SubgridSize) * SubgridSize;
            int startCol = (col / SubgridSize) * SubgridSize;
            
            for (int i = 0; i < SubgridSize; i++)
            {
                for (int j = 0; j < SubgridSize; j++)
                {
                    possibleValues.Remove(board[startRow + i, startCol + j]);
                }
            }

            return possibleValues;
        }

        private bool SolveWithBacktracking(int[,] board, Dictionary<(int, int), HashSet<int>> domains)
        {
            if (!domains.Any()) return true;

            var cell = domains.OrderBy(d => d.Value.Count).First();
            int row = cell.Key.Item1;
            int col = cell.Key.Item2;
            List<int> values = cell.Value.OrderBy(v => GetConstrainingValueCount(board, row, col, v)).ToList();

            foreach (int value in values)
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

            return false;
        }

        private int GetConstrainingValueCount(int[,] board, int row, int col, int value)
        {
            int count = 0;
            for (int i = 0; i < Size; i++)
            {
                if (board[row, i] == 0 && GetPossibleValues(board, row, i).Contains(value)) count++;
                if (board[i, col] == 0 && GetPossibleValues(board, i, col).Contains(value)) count++;
            }
            return count;
        }

        private Dictionary<(int, int), HashSet<int>> ForwardCheck(Dictionary<(int, int), HashSet<int>> domains, int row, int col, int value)
        {
            var affected = new Dictionary<(int, int), HashSet<int>>();

            foreach (var key in domains.Keys.ToList())
            {
                if (key.Item1 == row || key.Item2 == col || (key.Item1 / SubgridSize == row / SubgridSize && key.Item2 / SubgridSize == col / SubgridSize))
                {
                    if (domains[key].Contains(value))
                    {
                        affected[key] = new HashSet<int>(domains[key]);
                        domains[key].Remove(value);

                        if (domains[key].Count == 0)
                        {
                            RestoreDomains(domains, affected);
                            return null;
                        }
                    }
                }
            }

            return affected;
        }

        private void RestoreDomains(Dictionary<(int, int), HashSet<int>> domains, Dictionary<(int, int), HashSet<int>> affected)
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
    }
}
