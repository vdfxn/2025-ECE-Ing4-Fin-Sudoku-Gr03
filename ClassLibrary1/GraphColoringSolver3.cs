using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.GraphColoringSolver
{
    public class OptimizedGraphColoringSolverV2 : ISudokuSolver
    {
        private Dictionary<(int, int), HashSet<int>> _candidates;
        private Stack<(int, int, HashSet<int>)> _history;
        
        public SudokuGrid Solve(SudokuGrid s)
        {
            var sudokuGrid = s.CloneSudoku();
            _candidates = new Dictionary<(int, int), HashSet<int>>();
            _history = new Stack<(int, int, HashSet<int>)>();

            var graph = BuildOptimizedSudokuGraph(sudokuGrid);
            InitializeCandidates(sudokuGrid, graph);

            PreFillUniqueCandidates(sudokuGrid, graph);

            if (SolveWithDSATUR(sudokuGrid, graph))
            {
                return sudokuGrid;
            }
            return s;
        }

        private Dictionary<(int, int), HashSet<(int, int)>> BuildOptimizedSudokuGraph(SudokuGrid grid)
        {
            var graph = new Dictionary<(int, int), HashSet<(int, int)>>();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    graph[(row, col)] = new HashSet<(int, int)>(SudokuGrid.CellNeighbours[row][col]);
                }
            }
            return graph;
        }

        private void InitializeCandidates(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        var usedValues = graph[(row, col)].Select(n => grid.Cells[n.Item1, n.Item2]).Where(v => v != 0).ToHashSet();
                        _candidates[(row, col)] = new HashSet<int>(Enumerable.Range(1, 9).Except(usedValues));
                    }
                }
            }
        }

        private bool SolveWithDSATUR(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            while (_candidates.Count > 0)
            {
                var nextCell = FindMostSaturatedCell(grid, graph);
                if (nextCell == null)
                {
                    return true;
                }
                
                var (row, col) = nextCell.Value;
                var candidates = _candidates[(row, col)]
                    .OrderBy(v => CountImpactOnNeighbors(v, (row, col), grid, graph))
                    .ThenByDescending(v => GetFrequencyScore(v, (row, col), grid))
                    .ToList();
                
                foreach (var value in candidates)
                {
                    if (IsSafe(value, (row, col), grid, graph))
                    {
                        _history.Push((row, col, new HashSet<int>(_candidates[(row, col)])));
                        grid.Cells[row, col] = value;
                        UpdateCandidates((row, col), value, graph, false);
                        if (SolveWithDSATUR(grid, graph))
                        {
                            return true;
                        }
                        grid.Cells[row, col] = 0;
                        RestoreLastState();
                    }
                }
                return false;
            }
            return true;
        }

private (int, int)? FindMostSaturatedCell(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
{
    return _candidates.Keys
        .Select(cell => (cell, saturation: graph[cell].Count(n => grid.Cells[n.Item1, n.Item2] != 0), count: _candidates[cell].Count))
        .OrderByDescending(t => (t.saturation, -t.count))
        .Select(t => t.cell)
        .FirstOrDefault();
}


        private void UpdateCandidates((int row, int col) cell, int value, Dictionary<(int, int), HashSet<(int, int)>> graph, bool isBacktracking)
        {
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
            if (!isBacktracking)
            {
                _candidates.Remove(cell);
            }
        }

        private void RestoreLastState()
        {
            if (_history.Count > 0)
            {
                var (row, col, candidates) = _history.Pop();
                _candidates[(row, col)] = candidates;
            }
        }

        private int CountImpactOnNeighbors(int value, (int, int) cell, SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            return graph[cell].Count(n => _candidates.ContainsKey(n) && _candidates[n].Contains(value));
        }

        private int GetFrequencyScore(int value, (int, int) cell, SudokuGrid grid)
        {
            int row = cell.Item1, col = cell.Item2;
            return Enumerable.Range(0, 9).Count(i => grid.Cells[row, i] == value || grid.Cells[i, col] == value || grid.Cells[(row / 3) * 3 + i / 3, (col / 3) * 3 + i % 3] == value);
        }

        private bool IsSafe(int value, (int, int) cell, SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            return !graph[cell].Any(n => grid.Cells[n.Item1, n.Item2] == value);
        }

        private void PreFillUniqueCandidates(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            bool hasChanged;
            do
            {
                hasChanged = false;
                foreach (var cell in _candidates.Keys.ToList())
                {
                    if (_candidates[cell].Count == 1)
                    {
                        int value = _candidates[cell].First();
                        grid.Cells[cell.Item1, cell.Item2] = value;
                        UpdateCandidates(cell, value, graph, false);
                        _candidates.Remove(cell);
                        hasChanged = true;
                    }
                }
            } while (hasChanged);
        }
    }
}