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
            int[,] board = sudoku.Cells;
            Dictionary<(int, int), HashSet<int>> domains = InitializeDomains(board);

            if (ColorSudoku(board, domains))
            {
                sudoku.Cells = board;
            }

            return sudoku;
        }

        private Dictionary<(int, int), HashSet<int>> InitializeDomains(int[,] board)
        {
            Dictionary<(int, int), HashSet<int>> domains = new();

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
            HashSet<int> possibleValues = new(Enumerable.Range(1, 9));

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

        private bool ColorSudoku(int[,] board, Dictionary<(int, int), HashSet<int>> domains)
        {
            if (!domains.Any()) return true;

            // 🔥 HEURISTIQUE MRV + DEGRE
            var emptyCell = domains.OrderBy(d => d.Value.Count)
                                   .ThenByDescending(d => GetDegree(board, d.Key.Item1, d.Key.Item2))
                                   .First();
            int row = emptyCell.Key.Item1;
            int col = emptyCell.Key.Item2;
            List<int> values = emptyCell.Value.OrderBy(v => GetLeastConstrainingValue(board, row, col, v)).ToList();

            foreach (int value in values)
            {
                if (IsValidAssignment(board, row, col, value))
                {
                    board[row, col] = value;
                    var affectedDomains = ForwardCheck(domains, row, col, value);

                    if (affectedDomains != null && ColorSudoku(board, domains))
                    {
                        return true;
                    }

                    // BACKTRACK
                    board[row, col] = 0;
                    RestoreDomains(domains, affectedDomains);
                }
            }

            return false;
        }

        // 🔥 HEURISTIQUE DU DEGRÉ : compte combien de cases sont affectées
        private int GetDegree(int[,] board, int row, int col)
        {
            int count = 0;
            for (int i = 0; i < Size; i++)
            {
                if (board[row, i] == 0) count++;
                if (board[i, col] == 0) count++;
            }

            int startRow = (row / SubgridSize) * SubgridSize;
            int startCol = (col / SubgridSize) * SubgridSize;
            for (int i = 0; i < SubgridSize; i++)
            {
                for (int j = 0; j < SubgridSize; j++)
                {
                    if (board[startRow + i, startCol + j] == 0) count++;
                }
            }

            return count;
        }

        // 🔥 VALEUR LA MOINS CONTRAIGNANTE
        private int GetLeastConstrainingValue(int[,] board, int row, int col, int value)
        {
            int count = 0;
            for (int i = 0; i < Size; i++)
            {
                if (board[row, i] == 0 && GetPossibleValues(board, row, i).Contains(value)) count++;
                if (board[i, col] == 0 && GetPossibleValues(board, i, col).Contains(value)) count++;
            }
            return count;
        }

        // 🔥 FORWARD CHECKING : met à jour les domaines après une assignation
        private Dictionary<(int, int), HashSet<int>> ForwardCheck(Dictionary<(int, int), HashSet<int>> domains, int row, int col, int value)
        {
            Dictionary<(int, int), HashSet<int>> affected = new();

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
                            // Problème détecté, on annule l'opération
                            RestoreDomains(domains, affected);
                            return null;
                        }
                    }
                }
            }

            return affected;
        }

        // 🔥 RESTAURATION DES DOMAINES APRÈS BACKTRACKING
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
                if (board[row, i] == value && i != col) return false;
                if (board[i, col] == value && i != row) return false;
            }

            int startRow = (row / SubgridSize) * SubgridSize;
            int startCol = (col / SubgridSize) * SubgridSize;
            for (int i = 0; i < SubgridSize; i++)
            {
                for (int j = 0; j < SubgridSize; j++)
                {
                    if (board[startRow + i, startCol + j] == value && (startRow + i != row || startCol + j != col))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
