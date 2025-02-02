using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.Constraint
{
    public class ConstraintSimpleSolver : ISudokuSolver
    {
        private Dictionary<(int, int), HashSet<int>> domains;

        public SudokuGrid Solve(SudokuGrid s)
        {
            // Initialisation des domaines possibles pour chaque case
            InitializeDomains(s);
            
            // Résolution avec programmation par contraintes
            if (SolveSudoku(s))
            {
                return s;
            }

            throw new Exception("No solution found");
        }

        private void InitializeDomains(SudokuGrid s)
        {
            domains = new Dictionary<(int, int), HashSet<int>>();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (s.Cells[row, col] == 0)
                    {
                        domains[(row, col)] = GetPossibleValues(s, row, col);
                    }
                }
            }
        }

        private bool SolveSudoku(SudokuGrid s)
        {
            if (!domains.Any()) return true; // Toutes les cases sont remplies

            // Sélectionner la case avec le moins de choix possibles (MRV)
            var (row, col) = domains.OrderBy(d => d.Value.Count).First().Key;
            var possibleValues = domains[(row, col)].ToList();

            // Supprimer la case de la liste des domaines
            domains.Remove((row, col));

            foreach (var num in possibleValues)
            {
                // Créer une copie de l'état actuel des domaines
                var backupDomains = domains.ToDictionary(d => d.Key, d => new HashSet<int>(d.Value));

                s.Cells[row, col] = num;
                if (ForwardCheck(s, row, col, num))
                {
                    if (SolveSudoku(s)) return true;
                }

                // Restaurer l'état précédent si l'essai échoue
                s.Cells[row, col] = 0;
                domains = backupDomains;
            }

            // Réinsérer la case dans les domaines pour d'autres essais
            domains[(row, col)] = new HashSet<int>(possibleValues);
            return false;
        }

        private bool ForwardCheck(SudokuGrid s, int row, int col, int num)
        {
            foreach (var (r, c) in GetRelatedCells(row, col))
            {
                if (domains.ContainsKey((r, c)))
                {
                    domains[(r, c)].Remove(num);
                    if (domains[(r, c)].Count == 0) return false; // Échec si une case n'a plus d'options
                }
            }
            return true;
        }

        private HashSet<int> GetPossibleValues(SudokuGrid s, int row, int col)
        {
            var possibleValues = new HashSet<int>(Enumerable.Range(1, 9));

            foreach (var (r, c) in GetRelatedCells(row, col))
            {
                possibleValues.Remove(s.Cells[r, c]);
            }

            return possibleValues;
        }

        private IEnumerable<(int, int)> GetRelatedCells(int row, int col)
        {
            for (int i = 0; i < 9; i++)
            {
                yield return (row, i); // Ligne
                yield return (i, col); // Colonne
            }

            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    yield return (startRow + i, startCol + j); // Bloc 3x3
                }
            }
        }
    }
}
