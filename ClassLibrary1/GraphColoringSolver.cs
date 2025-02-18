using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.GraphColoringSolver
{
    public class GraphColoringSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            // 1. Construction du graphe du Sudoku
            var graph = BuildSudokuGraph(s);

            // 2. Coloration du graphe (gloutonne pour commencer)
            //var coloredGrid = ColorGraphGreedy(s, graph);
            var coloredGrid = ColorGraphDSatur(s, graph);
          
             if (IsGridValid(coloredGrid))
             {
                return coloredGrid;
             }
             else
             {
                Console.WriteLine("Solution not valid");
                 return s;
             }
            
        }

        private Dictionary<(int, int), List<(int, int)>> BuildSudokuGraph(SudokuGrid grid)
        {
            var graph = new Dictionary<(int, int), List<(int, int)>>();
          for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    graph[(row, col)] = new List<(int, int)>();
                     foreach (var neighbor in SudokuGrid.CellNeighbours[row][col])
                     {
                        graph[(row, col)].Add(neighbor);
                     }
                   
                }
            }
           
            return graph;
        }
        
        private SudokuGrid ColorGraphGreedy(SudokuGrid s, Dictionary<(int, int), List<(int, int)>> graph)
        {
            var sudokuGrid = s.CloneSudoku();
            var colors = new Dictionary<(int, int), int>();

            // Initialisation du dictionnaire colors avec 0 pour toutes les cellules
            foreach (var vertex in graph.Keys)
            {
                colors[vertex] = 0;
            }
            // On met à jour les valeurs de départ dans colors
            foreach (var vertex in graph.Keys)
            {
                if (sudokuGrid.Cells[vertex.Item1, vertex.Item2] != 0)
                {
                    colors[vertex] = sudokuGrid.Cells[vertex.Item1, vertex.Item2];
                }
            }

            foreach (var vertex in graph.Keys)
            {
                if (colors[vertex] == 0) // Si la case est à 0 alors il faut la colorer
                {
                    var usedColors = new HashSet<int>();

                    foreach (var neighbor in graph[vertex])
                    {
                        if (colors.ContainsKey(neighbor))
                        {
                            usedColors.Add(colors[neighbor]);
                        }
                    }

                    for (int color = 1; color <= 9; color++)
                    {
                        if (!usedColors.Contains(color))
                        {
                            colors[vertex] = color;
                            break;
                        }
                    }
                }
            }

            foreach(var vertex in graph.Keys)
            {
                sudokuGrid.Cells[vertex.Item1,vertex.Item2] = colors[vertex];
            }
            return sudokuGrid;
        }

       private SudokuGrid ColorGraphDSatur(SudokuGrid s, Dictionary<(int, int), List<(int, int)>> graph)
        {
            var sudokuGrid = s.CloneSudoku();
             var colors = new Dictionary<(int, int), int>();
            var saturationDegrees = new Dictionary<(int, int), int>();

            // Initialisation des dictionnaires
             foreach (var vertex in graph.Keys)
            {
               colors[vertex] = 0;
               saturationDegrees[vertex] = 0;
                 if (sudokuGrid.Cells[vertex.Item1, vertex.Item2] != 0)
                {
                    colors[vertex] = sudokuGrid.Cells[vertex.Item1, vertex.Item2];
                }

            }
          
            while (colors.Any(kvp => kvp.Value == 0))
            {
                 UpdateSaturationDegrees(graph, colors, saturationDegrees);
                 var vertexToColor = FindBestCellToColorDSatur(graph, colors, saturationDegrees);

                 if (vertexToColor != (0,0))
                {
                    int color = FindMinimumColor(vertexToColor, graph, colors, sudokuGrid);
                     if (color > 0)
                    {
                       colors[vertexToColor] = color;
                    }
                     else {
                        return s; // pas de solution
                     }
                } else {
                   break; //Si tous les sommets non colorés sont de degré 0, il n'y a pas de sommet a colorer
                }
            }

           foreach (var vertex in graph.Keys)
           {
             sudokuGrid.Cells[vertex.Item1, vertex.Item2] = colors[vertex];
            }
           return sudokuGrid;
        }

      private  (int,int) FindBestCellToColorDSatur( Dictionary<(int, int), List<(int, int)>> graph, Dictionary<(int, int), int> colors,  Dictionary<(int, int), int> saturationDegrees)
         {
              (int, int) bestCell = (0,0);
             int maxSaturation = -1;
              int maxDegree = -1;

            foreach (var cell in graph.Keys)
            {
                 if(colors[cell] == 0)
                {
                   int saturation = saturationDegrees[cell];
                    int degree = graph[cell].Count;

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

      private int FindMinimumColor((int,int) cell,  Dictionary<(int, int), List<(int, int)>> graph,  Dictionary<(int, int), int> colors, SudokuGrid grid)
         {
              HashSet<int> usedColors = new HashSet<int>();
               foreach(var neighbor in graph[cell])
               {
                    if(colors.ContainsKey(neighbor) && colors[neighbor] != 0){
                        usedColors.Add(colors[neighbor]);
                    }

               }
                for (int color = 1; color <= 9; color++)
                {
                    if(!usedColors.Contains(color) && IsSafe(color, cell, grid, colors)){
                       return color;
                    }
                }

                return 0; // no color found
         }

        private void UpdateSaturationDegrees(Dictionary<(int, int), List<(int, int)>> graph, Dictionary<(int, int), int> colors, Dictionary<(int, int), int> saturationDegrees)
        {
            foreach (var vertex in graph.Keys)
            {
                saturationDegrees[vertex] = 0;
                if (colors[vertex] == 0) // On calcule le degré de saturation que pour les noeuds non colorés
                {
                    var usedColors = new HashSet<int>();
                    foreach (var neighbor in graph[vertex])
                    {
                        if (colors.ContainsKey(neighbor) && colors[neighbor] != 0)
                        {
                            usedColors.Add(colors[neighbor]);
                        }
                    }
                    saturationDegrees[vertex] = usedColors.Count;
                }
            }
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
                    else if(colors == null && grid.Cells[r, cell.col] == value)
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
                       else  if(colors == null && grid.Cells[r, c] == value)
                        {
                            return false;
                         }
                    }
                }
            }
           return true;
        }
        private bool IsGridValid(SudokuGrid grid,  Dictionary<(int, int), int> colors = null)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                     if (!IsSafe(grid.Cells[row,col], (row, col), grid, colors))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}