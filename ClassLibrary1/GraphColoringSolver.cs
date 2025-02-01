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


        private SudokuGrid ColorGraphDSatur(SudokuGrid s, Dictionary<(int, int), List<(int, int)>> graph)
        {
            var sudokuGrid = s.CloneSudoku();
            var colors = new Dictionary<(int, int), int>();
            var saturationDegrees = new Dictionary<(int, int), int>();


            // Initialisation des dictionnaires
            foreach (var vertex in graph.Keys)
            {
                colors[vertex] = 0; // 0 indique que le sommet n'est pas encore coloré
                saturationDegrees[vertex] = 0; // Initialisation du degré de saturation à 0 pour tous les sommets

                if (sudokuGrid.Cells[vertex.Item1, vertex.Item2] != 0)
                {
                    colors[vertex] = sudokuGrid.Cells[vertex.Item1, vertex.Item2];
                }
            }

            // Boucle principale de l'algorithme DSatur
            while (colors.Any(kvp => kvp.Value == 0))
            {
                // 1. Mise à jour des degrés de saturation
                UpdateSaturationDegrees(graph, colors, saturationDegrees);


                // 2. Selection du sommet non coloré avec le plus grand degré de saturation
                var vertexToColor = saturationDegrees
                    .Where(kvp => colors[kvp.Key] == 0)
                    .OrderByDescending(kvp => kvp.Value)
                    .ThenBy(kvp => graph[kvp.Key].Count) // En cas d'égalité on préfère le noeud de plus haut degré
                    .FirstOrDefault().Key;


                // 3. Coloration du sommet choisi avec la plus petite couleur possible
                if (vertexToColor != (0,0))
                {
                    var usedColors = new HashSet<int>();
                    foreach (var neighbor in graph[vertexToColor])
                    {
                        if (colors.ContainsKey(neighbor) && colors[neighbor] != 0)
                        {
                            usedColors.Add(colors[neighbor]);
                        }
                    }

                    for (int color = 1; color <= 9; color++)
                    {
                        if (!usedColors.Contains(color))
                        {
                            colors[vertexToColor] = color;
                            break;
                        }
                    }
                } else
                {
                   break; //Si tous les sommets non colorés sont de degré 0, il n'y a pas de sommet a colorer
                }


            }

            // Copie des couleurs dans la grille de Sudoku
            foreach (var vertex in graph.Keys)
            {
                sudokuGrid.Cells[vertex.Item1, vertex.Item2] = colors[vertex];
            }
            return sudokuGrid;
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
    }
}