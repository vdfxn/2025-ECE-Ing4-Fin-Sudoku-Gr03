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
            var coloredGrid = ColorGraphGreedy(s, graph);
          
             if (coloredGrid.IsValid(s))
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

    }
}