using System;
using System.Collections.Generic;
using Sudoku.Shared;

namespace Sudoku.ColorationGrapheRTsolvers
{
    public class ColorationGrapheSimpleSolverRT : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] grid = s.Cells;
            SolveSudokuWithGraphColoring(grid);
            s.Cells = grid;
            return s;
        }

        private void SolveSudokuWithGraphColoring(int[,] grid)
        {
            // Construire le graphe représentant la grille de Sudoku
            Dictionary<(int, int), HashSet<int>> graph = BuildGraph(grid);

            // Appliquer l'algorithme de coloration de graphe (DSatur)
            DSaturColoring(grid, graph);
        }

        private Dictionary<(int, int), HashSet<int>> BuildGraph(int[,] grid)
        {
            Dictionary<(int, int), HashSet<int>> graph = new();

            // Parcours de toutes les cellules de la grille
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid[row, col] == 0) // Si la cellule est vide, on la considère comme un nœud du graphe
                    {
                        // Initialisation des valeurs possibles (1 à 9)
                        HashSet<int> domain = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                        // Supprimer les valeurs déjà utilisées dans la ligne et la colonne
                        for (int i = 0; i < 9; i++)
                        {
                            domain.Remove(grid[row, i]); // Ligne
                            domain.Remove(grid[i, col]); // Colonne
                        }

                        // Supprimer les valeurs déjà présentes dans le bloc 3x3
                        int boxRow = (row / 3) * 3;
                        int boxCol = (col / 3) * 3;
                        for (int i = 0; i < 3; i++)
                            for (int j = 0; j < 3; j++)
                                domain.Remove(grid[boxRow + i, boxCol + j]);

                        // Ajouter la cellule et ses valeurs possibles au graphe
                        graph[(row, col)] = domain;
                    }
                }
            }
            return graph;
        }

        private void DSaturColoring(int[,] grid, Dictionary<(int, int), HashSet<int>> graph)
        {
            while (graph.Count > 0)
            {
                // Trouver la cellule avec le moins de choix possibles (la plus contrainte)
                var bestCell = GetMostConstrainedCell(graph);
                int row = bestCell.Item1, col = bestCell.Item2;

                // Trouver une valeur possible et l'assigner à la cellule
                int value = FindBestValue(graph, bestCell);
                grid[row, col] = value;

                // Mettre à jour le graphe en supprimant la cellule et en propageant les contraintes
                graph.Remove(bestCell);
                UpdateGraph(graph, row, col, value);
            }
        }

        private (int, int) GetMostConstrainedCell(Dictionary<(int, int), HashSet<int>> graph)
        {
            (int, int) bestCell = (0, 0);
            int minDomainSize = int.MaxValue;

            // Chercher la cellule avec le moins d'options possibles
            foreach (var cell in graph)
            {
                if (cell.Value.Count < minDomainSize)
                {
                    bestCell = cell.Key;
                    minDomainSize = cell.Value.Count;
                }
            }
            return bestCell;
        }

        private int FindBestValue(Dictionary<(int, int), HashSet<int>> graph, (int, int) cell)
        {
            // Prendre la plus petite valeur possible parmi celles disponibles
            return graph[cell].Count > 0 ? graph[cell].Min() : 1;
        }

        private void UpdateGraph(Dictionary<(int, int), HashSet<int>> graph, int row, int col, int value)
        {
            // Supprimer la valeur choisie des domaines des cellules voisines
            foreach (var cell in graph.Keys)
            {
                int r = cell.Item1, c = cell.Item2;
                if (r == row || c == col || (r / 3 == row / 3 && c / 3 == col / 3))
                {
                    graph[cell].Remove(value);
                }
            }
        }
    }
}
