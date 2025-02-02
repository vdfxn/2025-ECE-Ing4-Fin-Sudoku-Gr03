using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.GraphColoringSolver
{
    public class OptimizedGraphColoringSolverV2 : ISudokuSolver
    {
        private Dictionary<(int, int), HashSet<int>> _candidates;
         private Dictionary<(int, int), HashSet<int>> _initialCandidates;
        private int _recursionLevel = 0;

           private Dictionary<(int, int), int> _saturationDegrees;
        private Dictionary<(int, int), int> _dsaturOrder;
         private Dictionary<(int, int), int> _dsaturColors;


        public SudokuGrid Solve(SudokuGrid s)
        {
            var sudokuGrid = s.CloneSudoku();
             _candidates = new Dictionary<(int, int), HashSet<int>>();
             _initialCandidates = new Dictionary<(int, int), HashSet<int>>();

            var graph = BuildOptimizedSudokuGraph(sudokuGrid);
            InitializeCandidates(sudokuGrid, graph);
            _initialCandidates = _candidates.ToDictionary(entry => entry.Key, entry => new HashSet<int>(entry.Value));


            _saturationDegrees = new Dictionary<(int, int), int>();
            _dsaturOrder = new Dictionary<(int, int), int>();
             _dsaturColors = new Dictionary<(int, int), int>();

            bool dsaturSuccess = TrySolveWithDSatur(sudokuGrid, graph);


             if(dsaturSuccess)
             {
                if(IsGridValid(sudokuGrid))
                    return sudokuGrid;
            }
            else
             {
                if (SolveWithBacktracking(sudokuGrid, graph))
                {
                     return sudokuGrid;
                }
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

         private bool TrySolveWithDSatur(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
             //Initialiser
             _saturationDegrees = _candidates.Keys.ToDictionary(cell => cell, cell => 0);
             _dsaturOrder = new Dictionary<(int, int), int>();
             _dsaturColors = new Dictionary<(int, int), int>();

             List<(int,int)> cells = _candidates.Keys.ToList();
            int colorIndex = 0;

            while (_dsaturColors.Count < _candidates.Count)
            {
                (int, int)? cellToColor = FindBestCellToColorDSatur(_dsaturColors);

                if (!cellToColor.HasValue) // pas de sommet
                {
                      Console.WriteLine("DSatur : pas de sommet trouvé");
                      return false;
                }

                Console.WriteLine($"DSatur : Selection de la case  {cellToColor.Value}, Saturation : {_saturationDegrees[cellToColor.Value]}");
                int color = FindMinimumColor(cellToColor.Value, grid, graph, _dsaturColors);
                 if(color == 0)
                  {
                    Console.WriteLine($"DSatur : pas de couleur valide pour la case {cellToColor.Value}");
                    return false; //pas de couleur
                  }
                 Console.WriteLine($"DSatur : Ajout de la couleur {color} à la case {cellToColor.Value}");
                 grid.Cells[cellToColor.Value.Item1, cellToColor.Value.Item2] = color;
                 _dsaturColors[cellToColor.Value] = color;
                 _dsaturOrder[cellToColor.Value] =  colorIndex++;
                UpdateSaturationDegrees(cellToColor.Value, graph, _dsaturColors);


            }
           Console.WriteLine("DSatur : Succes!");
            return true;
        }


        private (int, int)? FindBestCellToColorDSatur(Dictionary<(int, int), int> colors)
        {
             (int, int)? bestCell = null;
           int maxSaturation = -1;
             int maxDegree = -1;

            foreach (var cell in _candidates.Keys)
            {
                 if(!colors.ContainsKey(cell))
                {
                    int saturation = _saturationDegrees[cell];
                    int degree = _candidates[cell].Count;


                   if(saturation > maxSaturation){
                        maxSaturation = saturation;
                         maxDegree = degree;
                        bestCell = cell;
                    } else if (saturation == maxSaturation && degree > maxDegree){
                       maxDegree = degree;
                        bestCell = cell;
                    }
                }
            }
              return bestCell;
        }

        private int FindMinimumColor((int,int) cell,  SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph,  Dictionary<(int, int), int> colors)
         {
              HashSet<int> usedColors = new HashSet<int>();
              Console.WriteLine($"FindMinimumColor : Case {cell}, Check les voisins :");
               foreach(var neighbor in graph[cell])
               {
                   if(colors.ContainsKey(neighbor)){
                        usedColors.Add(colors[neighbor]);
                        Console.WriteLine($"Voisin : {neighbor}, couleur : {colors[neighbor]}");
                    }

               }
                for (int color = 1; color <= 9; color++)
                {
                    Console.WriteLine($"FindMinimumColor : Test la couleur {color}");
                     if(!usedColors.Contains(color) && IsSafe(color, cell, grid, colors)){
                         Console.WriteLine($"FindMinimumColor : La couleur {color} est valide");
                       return color;
                    }
                }
                  Console.WriteLine($"FindMinimumColor : Pas de couleur valide");
                return 0; // no color found
         }

          private void UpdateSaturationDegrees((int, int) cell, Dictionary<(int, int), HashSet<(int, int)>> graph, Dictionary<(int, int), int> colors)
         {
            foreach (var neighbor in graph[cell])
            {
                 if(_saturationDegrees.ContainsKey(neighbor) && !colors.ContainsKey(neighbor)) {
                      _saturationDegrees[neighbor] =  _saturationDegrees[neighbor] + 1;

                 }

             }
         }
        private bool IsGridValid(SudokuGrid grid)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (!IsSafe(grid.Cells[row,col], (row, col), grid))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private bool SolveWithBacktracking(SudokuGrid grid, Dictionary<(int, int), HashSet<(int, int)>> graph)
        {
            _recursionLevel++;
           // Console.WriteLine($"[Recursion Level: {_recursionLevel}] Before SolveWithBacktracking:");

            var nextCell = FindBestCellToFill();
            if (nextCell == null)
            {
               // Console.WriteLine($"[Recursion Level: {_recursionLevel}] Solution found");
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
                    // Console.WriteLine($"[Recursion Level: {_recursionLevel}] Backtracking Cell ({row},{col}) Value {value} ");
                    UpdateCandidates((row,col), value, graph, true);
                     _candidates = _initialCandidates.ToDictionary(entry => entry.Key, entry => new HashSet<int>(entry.Value));


                } else {
                     // Console.WriteLine($"[Recursion Level: {_recursionLevel}] Value {value}  not safe for cell ({row},{col})");
                }
            }

              _recursionLevel--;
             //  Console.WriteLine($"[Recursion Level: {_recursionLevel}] No solution found for ({row},{col}), Returning...");
            return false;
        }



        private (int, int)? FindBestCellToFill()
        {
              var minCandidates = 10;
             (int, int)? bestCell = null;

            if(_dsaturOrder.Count > 0)
             {
                    foreach(var cell in _dsaturOrder.OrderBy(x => x.Value).Select(x => x.Key))
                    {
                         if(_candidates.ContainsKey(cell)){
                             bestCell = cell;
                             break;
                         }
                    }
                    return bestCell;
             }
            foreach (var cell in _candidates.Keys)
            {
                var count = _candidates[cell].Count;
                if (count > 0 && count < minCandidates)
                {
                    minCandidates = count;
                     bestCell = cell;
                     if(count == 1){
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

         private bool IsSafe(int value, (int row, int col) cell, SudokuGrid grid, Dictionary<(int, int), int> colors = null)
        {
           // Verification ligne
             for (int c = 0; c < 9; c++)
            {
                 if (c != cell.col)
                {
                   if(colors != null && colors.ContainsKey((cell.row, c)) && colors[(cell.row,c)] == value)
                        {
                            return false;
                        }
                    else if(colors == null && grid.Cells[cell.row, c] == value)
                        {
                             return false;
                        }

                }
            }
           // Verification colonne
              for (int r = 0; r < 9; r++)
            {
                if (r != cell.row)
                {
                      if(colors != null && colors.ContainsKey((r, cell.col)) && colors[(r, cell.col)] == value)
                        {
                            return false;
                        }
                     else  if(colors == null && grid.Cells[r, cell.col] == value)
                        {
                             return false;
                        }
                 }
            }
            // Verification carré
            int startRow = cell.row - cell.row % 3;
            int startCol = cell.col - cell.col % 3;
             for (int r = startRow; r < startRow + 3; r++)
            {
                 for (int c = startCol; c < startCol + 3; c++)
                {
                    if ((r != cell.row || c != cell.col))
                    {
                          if(colors != null && colors.ContainsKey((r, c)) && colors[(r,c)] == value)
                        {
                             return false;
                         }
                       else if(colors == null && grid.Cells[r, c] == value)
                        {
                            return false;
                        }
                    }
                }
            }
           return true;
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
                         if(_candidates[neighbor].Contains(value))
                              _candidates[neighbor].Remove(value);
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