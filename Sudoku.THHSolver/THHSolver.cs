using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.THHSolver
{
    public class THHSolver : ISudokuSolver
    {
        private int[] _grid; // Grille de Sudoku (tableau 1D pour plus de performance)
        private List<int>[] _candidates; // Candidats pour chaque case

        // Pré-calcul des indices pour chaque ligne, colonne et bloc
        private static readonly int[][] _units = new int[27][];
        private static readonly int[][] _peers = new int[81][];

        static THHSolver()
        {
            // Initialisation des unités et des pairs
            for (int i = 0; i < 9; i++)
            {
                // Lignes
                _units[i] = Enumerable.Range(i * 9, 9).ToArray();
                // Colonnes
                _units[i + 9] = Enumerable.Range(i, 9).Select(x => x * 9 + i).ToArray();
                // Blocs
                int startRow = (i / 3) * 3;
                int startCol = (i % 3) * 3;
                _units[i + 18] = Enumerable.Range(0, 9).Select(x => (startRow + x / 3) * 9 + (startCol + x % 3)).ToArray();
            }

            // Initialisation des pairs pour chaque case
            for (int i = 0; i < 81; i++)
            {
                var peers = new HashSet<int>();
                foreach (var unit in _units)
                {
                    if (unit.Contains(i))
                    {
                        foreach (var cell in unit)
                        {
                            if (cell != i)
                            {
                                peers.Add(cell);
                            }
                        }
                    }
                }
                _peers[i] = peers.ToArray();
            }
        }

        public SudokuGrid Solve(SudokuGrid grid)
        {
            // Convertir la grille en tableau 1D
            _grid = grid.Cells.Cast<int>().ToArray();
            _candidates = new List<int>[81];

            // Initialiser les candidats pour chaque case
            for (int i = 0; i < 81; i++)
            {
                if (_grid[i] == 0)
                {
                    _candidates[i] = GetPossibleValues(i);
                }
                else
                {
                    _candidates[i] = new List<int> { _grid[i] };
                }
            }

            // Appliquer les techniques humaines jusqu'à ce que la grille soit résolue ou bloquée
            bool progress;
            do
            {
                progress = false;
                progress |= ApplyNakedSingles();
                progress |= ApplyHiddenSingles();
                progress |= ApplyNakedPairs();
                progress |= ApplyPointingPairs();
            } while (progress);

            // Convertir la grille résolue en SudokuGrid
            for (int i = 0; i < 81; i++)
            {
                grid.Cells[i / 9, i % 9] = _grid[i];
            }

            return grid;
        }

        private bool ApplyNakedSingles()
        {
            bool progress = false;
            for (int i = 0; i < 81; i++)
            {
                if (_grid[i] == 0 && _candidates[i].Count == 1)
                {
                    AssignValue(i, _candidates[i][0]);
                    progress = true;
                }
            }
            return progress;
        }

        private bool ApplyHiddenSingles()
        {
            bool progress = false;
            for (int i = 0; i < 81; i++)
            {
                if (_grid[i] == 0)
                {
                    foreach (var value in _candidates[i])
                    {
                        if (IsHiddenSingle(i, value))
                        {
                            AssignValue(i, value);
                            progress = true;
                            break;
                        }
                    }
                }
            }
            return progress;
        }

        private bool IsHiddenSingle(int cell, int value)
        {
            foreach (var peer in _peers[cell])
            {
                if (_grid[peer] == 0 && _candidates[peer].Contains(value))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ApplyNakedPairs()
        {
            bool progress = false;
            foreach (var unit in _units)
            {
                var candidatesCount = unit.Select(c => _grid[c] == 0 ? _candidates[c].Count : 0).ToArray();
                for (int i = 0; i < unit.Length; i++)
                {
                    if (candidatesCount[i] == 2)
                    {
                        for (int j = i + 1; j < unit.Length; j++)
                        {
                            if (candidatesCount[j] == 2 && _candidates[unit[i]].SequenceEqual(_candidates[unit[j]]))
                            {
                                // Éliminer ces candidats des autres cases de l'unité
                                foreach (var cell in unit)
                                {
                                    if (_grid[cell] == 0 && cell != unit[i] && cell != unit[j])
                                    {
                                        foreach (var value in _candidates[unit[i]])
                                        {
                                            if (_candidates[cell].Remove(value))
                                            {
                                                progress = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return progress;
        }

        private bool ApplyPointingPairs()
        {
            bool progress = false;
            foreach (var unit in _units)
            {
                for (int value = 1; value <= 9; value++)
                {
                    var cellsWithValue = unit.Where(c => _grid[c] == 0 && _candidates[c].Contains(value)).ToList();
                    if (cellsWithValue.Count == 2 || cellsWithValue.Count == 3)
                    {
                        // Vérifier si toutes les cases sont dans le même bloc
                        var block = cellsWithValue.Select(c => c / 27 * 3 + (c % 9) / 3).Distinct().ToList();
                        if (block.Count == 1)
                        {
                            // Éliminer la valeur des autres cases du bloc
                            var blockStart = (block[0] / 3) * 27 + (block[0] % 3) * 3;
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    var cell = blockStart + i * 9 + j;
                                    if (_grid[cell] == 0 && !cellsWithValue.Contains(cell))
                                    {
                                        if (_candidates[cell].Remove(value))
                                        {
                                            progress = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return progress;
        }

        private void AssignValue(int cell, int value)
        {
            _grid[cell] = value;
            _candidates[cell] = new List<int> { value };

            // Éliminer la valeur des pairs
            foreach (var peer in _peers[cell])
            {
                if (_grid[peer] == 0)
                {
                    _candidates[peer].Remove(value);
                }
            }
        }

        private List<int> GetPossibleValues(int cell)
        {
            var usedValues = new HashSet<int>();
            foreach (var peer in _peers[cell])
            {
                if (_grid[peer] != 0)
                {
                    usedValues.Add(_grid[peer]);
                }
            }
            return Enumerable.Range(1, 9).Except(usedValues).ToList();
        }
    }
}