using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.GraphColoringSolver
{
    public class OptimizedGraphColoringSolver : ISudokuSolver
    {
        private Dictionary<(int, int), HashSet<int>> _candidates;
        private Dictionary<(int, int), HashSet<int>> _initialCandidates;
        private int _recursionLevel = 0;

        public SudokuGrid Solve(SudokuGrid s)
        {
            var sudokuGrid = s.CloneSudoku();
            _candidates = new Dictionary<(int, int), HashSet<int>>();
             _initialCandidates = new Dictionary<(int, int), HashSet<int>>();

            var graph = BuildOptimizedSudokuGraph(sudokuGrid);
            InitializeCandidates(sudokuGrid, graph);

            _initialCandidates = _candidates.ToDictionary(entry => entry.Key, entry => new HashSet<int>(entry.Value));


            if (SolveWithBacktracking(sudokuGrid, graph))
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
                        var usedValues = new HashSet<int>();
                        foreach (var neighbor in graph[(row, col)])
                        {
                            if (grid.Cells[neighbor.Item1, neighbor.Item2] != 0)
                            {
                                usedValues.Add(grid.Cells[neighbor.Item1, neighbor.Item2]);
                            }
                        }
                        _candidates[(row, col)] = new HashSet<int>(Enumerable.Range(1, 9).Except(usedValues));
                    }
                }
            }
        }

        private bool SolveWithBacktracking(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
              _recursionLevel++;
           // Console.WriteLine($"[Recursion Level: {_recursionLevel}] Before SolveWithBacktracking:");

            var nextCell = FindBestCellToFill();
            if (nextCell == null)
            {
                  //Console.WriteLine($"[Recursion Level: {_recursionLevel}] Solution found");
                  _recursionLevel--;
                return true;
            }


             var (row, col) = nextCell.Value;
           var candidates = _candidates[(row, col)].OrderBy(v => CountImpactOnNeighbors(v, (row, col), grid, graph));


            foreach (var value in candidates)
            {
                    //Console.WriteLine($"[Recursion Level: {_recursionLevel}] Trying Cell ({row},{col}) Value {value}, Candidates: {string.Join(",", _candidates[(row,col)])}");
                if (IsSafe(value, (row, col), grid))
                {

                    grid.Cells[row, col] = value;
                    UpdateCandidates((row, col), value, graph, false);

                    if (SolveWithBacktracking(grid, graph))
                    {
                         _recursionLevel--;
                        return true;
                    }

                    grid.Cells[row, col] = 0;
                     //  Console.WriteLine($"[Recursion Level: {_recursionLevel}] Backtracking Cell ({row},{col}) Value {value} ");
                    UpdateCandidates((row,col), value, graph, true);
                    _candidates = _initialCandidates.ToDictionary(entry => entry.Key, entry => new HashSet<int>(entry.Value));

                } else {
                       // Console.WriteLine($"[Recursion Level: {_recursionLevel}] Value {value}  not safe for cell ({row},{col})");
                }
            }

              _recursionLevel--;
              //Console.WriteLine($"[Recursion Level: {_recursionLevel}] No solution found for ({row},{col}), Returning...");
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
                    if (count == 1)
                    {
                        break;
                    }
                }
            }
            return bestCell;
        }

        private int CountImpactOnNeighbors(int value, (int, int) cell, SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            int impact = 0;

            foreach (var neighbor in graph[cell])
            {
                if (_candidates.ContainsKey(neighbor) && _candidates[neighbor].Contains(value))
                {
                    impact++;
                }
            }
            return impact;
        }

        private bool IsSafe(int value, (int row, int col) cell, SudokuGrid grid)
        {
            bool result = true;

             for (int c = 0; c < 9; c++)
            {
                if (c != cell.col && grid.Cells[cell.row, c] == value)
                {
                      result = false;
                       //Console.WriteLine($"IsSafe: ({cell.row},{cell.col}), value: {value} not safe because of ligne");
                     break;

                }
            }
            if(result){
                for (int r = 0; r < 9; r++)
                {
                     if (r != cell.row && grid.Cells[r, cell.col] == value)
                        {
                            result = false;
                           //  Console.WriteLine($"IsSafe: ({cell.row},{cell.col}), value: {value} not safe because of column");
                            break;

                        }
                }
            }
            if(result)
            {
                int startRow = cell.row - cell.row % 3;
                int startCol = cell.col - cell.col % 3;
                    for (int r = startRow; r < startRow + 3; r++)
                {
                     for (int c = startCol; c < startCol + 3; c++)
                        {
                             if ((r != cell.row || c != cell.col) && grid.Cells[r, c] == value)
                            {
                                result = false;
                              // Console.WriteLine($"IsSafe: ({cell.row},{cell.col}), value: {value} not safe because of block");
                                break;

                             }
                        }
                    if(!result){
                        break;
                    }
                }
            }


           return result;
        }



        private void UpdateCandidates((int row, int col) cell, int value, Dictionary<(int, int), HashSet<(int, int)>> graph, bool isBacktracking)
        {
                foreach (var neighbor in graph[cell])
                {
                    if (_candidates.ContainsKey(neighbor))
                    {
                        if(isBacktracking){
                            if(!_candidates[neighbor].Contains(value))
                                _candidates[neighbor].Add(value);
                        } else {
                            if(_candidates[neighbor].Contains(value)){
                                _candidates[neighbor].Remove(value);
                            }
                        }

                    }
                }
                 if (!isBacktracking)
                {
                     _candidates.Remove(cell);
                }
        }

        private void PrintGrid(SudokuGrid grid)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    Console.Write(grid.Cells[row, col] + " ");
                }
                Console.WriteLine();
            }
             Console.WriteLine("------------------------");
        }

         private void PrintCandidates(){
            foreach(var cell in _candidates)
            {
                 Console.WriteLine($"Cell {cell.Key} : [{string.Join(",", cell.Value)}]");
            }
        }

        private void PrintInitialCandidates(){
            foreach(var cell in _initialCandidates)
            {
                 Console.WriteLine($"Cell {cell.Key} : [{string.Join(",", cell.Value)}]");
            }
        }
    }
}