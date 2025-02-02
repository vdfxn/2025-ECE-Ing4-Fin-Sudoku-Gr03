using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.GraphColoringSolver
{
    public class OptimizedGraphColoringSolver : ISudokuSolver
    {
        // Structure pour stocker les candidats possibles pour chaque cellule
        private Dictionary<(int, int), HashSet<int>> _candidates;

        public SudokuGrid Solve(SudokuGrid s)
        {
            var sudokuGrid = s.CloneSudoku();
            _candidates = new Dictionary<(int, int), HashSet<int>>();

            // 1. Construction du graphe optimisé du Sudoku
            var graph = BuildOptimizedSudokuGraph(sudokuGrid);

            // 2. Initialisation des candidats possibles pour chaque cellule vide
            InitializeCandidates(sudokuGrid, graph);

            // 3. Résolution avec backtracking et heuristiques
            if (SolveWithBacktracking(sudokuGrid, graph))
            {
                return sudokuGrid;
            }

            return s; // Retourne la grille originale si pas de solution
        }

        private Dictionary<(int, int), HashSet<(int, int)>> BuildOptimizedSudokuGraph(SudokuGrid grid)
        {
            var graph = new Dictionary<(int, int), HashSet<(int, int)>>();
            
            // Utilisation de HashSet pour des recherches O(1)
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    graph[(row, col)] = new HashSet<(int, int)>(
                        SudokuGrid.CellNeighbours[row][col]
                    );
                }
            }
            
            return graph;
        }

        private void InitializeCandidates(
            SudokuGrid grid,
            Dictionary<(int, int), HashSet<(int, int)>> graph
        )
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        // Pour chaque cellule vide, on détermine les valeurs possibles
                        var usedValues = new HashSet<int>();
                        foreach (var neighbor in graph[(row, col)])
                        {
                            if (grid.Cells[neighbor.Item1, neighbor.Item2] != 0)
                            {
                                usedValues.Add(grid.Cells[neighbor.Item1, neighbor.Item2]);
                            }
                        }

                        // Les candidats sont les valeurs non utilisées par les voisins
                        _candidates[(row, col)] = new HashSet<int>(
                            Enumerable.Range(1, 9).Except(usedValues)
                        );
                    }
                }
            }
        }

        private bool SolveWithBacktracking(
            SudokuGrid grid,
            Dictionary<(int, int), HashSet<(int, int)>> graph
        )
        {
            // Trouve la cellule avec le moins de candidats (MRV - Minimum Remaining Values)
            var nextCell = FindBestCellToFill();
            if (nextCell == null)
            {
                return true; // Toutes les cellules sont remplies
            }

            var (row, col) = nextCell.Value;
            var candidates = _candidates[(row, col)].OrderBy(v => 
                // LCV (Least Constraining Value) - Trie les valeurs qui contraignent le moins les voisins
                CountImpactOnNeighbors(v, (row, col), grid, graph)
            );

            foreach (var value in candidates)
            {
                if (IsSafe(value, (row, col), grid, graph))
                {
                    // Essaie une valeur
                    grid.Cells[row, col] = value;
                    UpdateCandidates((row, col), value, graph, false);

                    if (SolveWithBacktracking(grid, graph))
                    {
                        return true;
                    }

                    // Backtrack si nécessaire
                    grid.Cells[row, col] = 0;
                    UpdateCandidates((row, col), value, graph, true);
                }
            }

            return false;
        }

        private (int, int)? FindBestCellToFill()
        {
            var minCandidates = 10;
            (int, int)? bestCell = null;

            foreach (var cell in _candidates.Keys)
            {
                var count = _candidates[cell].Count;
                if (count > 0 && count < minCandidates)
                {
                    minCandidates = count;
                    bestCell = cell;
                    
                    // Optimisation : si on trouve une cellule avec un seul candidat,
                    // on peut l'utiliser immédiatement
                    if (count == 1)
                    {
                        break;
                    }
                }
            }

            return bestCell;
        }

        private int CountImpactOnNeighbors(
            int value,
            (int, int) cell,
            SudokuGrid grid,
            Dictionary<(int, int), HashSet<(int, int)>> graph
        )
        {
            int impact = 0;
            foreach (var neighbor in graph[cell])
            {
                if (grid.Cells[neighbor.Item1, neighbor.Item2] == 0 &&
                    _candidates.ContainsKey(neighbor) &&
                    _candidates[neighbor].Contains(value))
                {
                    impact++;
                }
            }
            return impact;
        }

        private bool IsSafe(
            int value,
            (int, int) cell,
            SudokuGrid grid,
            Dictionary<(int, int), HashSet<(int, int)>> graph
        )
        {
            foreach (var neighbor in graph[cell])
            {
                if (grid.Cells[neighbor.Item1, neighbor.Item2] == value)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateCandidates(
            (int, int) cell,
            int value,
            Dictionary<(int, int), HashSet<(int, int)>> graph,
            bool isBacktracking
        )
        {
            // Mise à jour des candidats pour les voisins
            foreach (var neighbor in graph[cell])
            {
                if (_candidates.ContainsKey(neighbor))
                {
                    if (isBacktracking)
                    {
                        _candidates[neighbor].Add(value);
                    }
                    else
                    {
                        _candidates[neighbor].Remove(value);
                    }
                }
            }
            if (isBacktracking)
            {
                _candidates[cell] = new HashSet<int>(_candidates[cell]);
            }
            else
            {
                _candidates.Remove(cell);
            }
        }
    }
}
