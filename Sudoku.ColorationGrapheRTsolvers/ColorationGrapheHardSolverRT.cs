using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.ColorationGrapheRTsolvers
{
    public class ColorationGrapheHardSolverRT : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] grid = s.Cells;
            if (SolveSudokuWithGraphColoring(grid)) // On tente de résoudre avec la coloration de graphe
            {
                s.Cells = grid;
            }
            return s;
        }

        private bool SolveSudokuWithGraphColoring(int[,] grid)
        {
            // Création du graphe avec les contraintes du Sudoku
            Dictionary<(int, int), HashSet<int>> graph = BuildGraph(grid);
            return DSaturColoring(grid, graph); // Lancement de la coloration
        }

        private Dictionary<(int, int), HashSet<int>> BuildGraph(int[,] grid)
        {
            Dictionary<(int, int), HashSet<int>> graph = new();

            // On parcourt toute la grille
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid[row, col] == 0) // Si la case est vide, on doit la remplir
                    {
                        // Initialisation des valeurs possibles (1 à 9)
                        HashSet<int> domain = Enumerable.Range(1, 9).ToHashSet();

                        // On supprime les valeurs déjà utilisées dans la ligne et la colonne
                        for (int i = 0; i < 9; i++)
                        {
                            domain.Remove(grid[row, i]); // Ligne
                            domain.Remove(grid[i, col]); // Colonne
                        }

                        // On supprime aussi les valeurs présentes dans le bloc 3x3
                        int boxRow = (row / 3) * 3;
                        int boxCol = (col / 3) * 3;
                        for (int i = 0; i < 3; i++)
                            for (int j = 0; j < 3; j++)
                                domain.Remove(grid[boxRow + i, boxCol + j]);

                        // On stocke les valeurs possibles pour cette cellule
                        graph[(row, col)] = domain;
                    }
                }
            }
            return graph;
        }

        private bool DSaturColoring(int[,] grid, Dictionary<(int, int), HashSet<int>> graph)
        {
            if (graph.Count == 0)
                return true; // Plus rien à remplir, on a terminé

            // On cherche la case avec le plus de contraintes
            var bestCell = GetMostConstrainedCell(graph);
            int row = bestCell.Item1, col = bestCell.Item2;

            // On trie les valeurs possibles pour choisir la meilleure
            var sortedValues = GetSortedValues(graph, bestCell);
            foreach (var value in sortedValues)
            {
                grid[row, col] = value;
                var newGraph = CloneGraph(graph); // Copie du graphe pour éviter de tout casser
                newGraph.Remove(bestCell);
                UpdateGraph(newGraph, row, col, value); // Mise à jour des contraintes

                if (DSaturColoring(grid, newGraph)) // On continue la résolution
                    return true;

                grid[row, col] = 0; // Si ça coince, on annule et on teste une autre valeur
            }

            return false; // Aucune valeur ne marche, on remonte en arrière (backtracking)
        }

        private (int, int) GetMostConstrainedCell(Dictionary<(int, int), HashSet<int>> graph)
        {
            // On prend la cellule qui a le moins de choix possibles, et en cas d'égalité on regarde le nombre de voisins
            return graph.OrderBy(cell => cell.Value.Count)
                        .ThenByDescending(cell => GetDegree(cell.Key, graph))
                        .First().Key;
        }

        private IEnumerable<int> GetSortedValues(Dictionary<(int, int), HashSet<int>> graph, (int, int) cell)
        {
            // On trie les valeurs en fonction de leur impact sur le reste du graphe
            return graph[cell].OrderBy(val => GetImpact(graph, cell, val));
        }

        private int GetDegree((int, int) cell, Dictionary<(int, int), HashSet<int>> graph)
        {
            // Nombre de voisins dans le graphe (cases liées par une contrainte)
            return graph.Keys.Count(other => AreNeighbors(cell, other));
        }

        private int GetImpact(Dictionary<(int, int), HashSet<int>> graph, (int, int) cell, int value)
        {
            // Nombre de fois où une valeur est présente chez les voisins (on veut éviter celles qui bloquent trop)
            return graph.Keys.Count(other => AreNeighbors(cell, other) && graph[other].Contains(value));
        }

        private void UpdateGraph(Dictionary<(int, int), HashSet<int>> graph, int row, int col, int value)
        {
            // On supprime la valeur choisie des cellules voisines
            foreach (var cell in graph.Keys)
            {
                if (AreNeighbors((row, col), cell))
                {
                    graph[cell].Remove(value);
                }
            }
        }

        private bool AreNeighbors((int, int) cell1, (int, int) cell2)
        {
            // Deux cellules sont voisines si elles sont sur la même ligne, colonne ou dans le même bloc 3x3
            return cell1.Item1 == cell2.Item1 || cell1.Item2 == cell2.Item2 ||
                   (cell1.Item1 / 3 == cell2.Item1 / 3 && cell1.Item2 / 3 == cell2.Item2 / 3);
        }

        private Dictionary<(int, int), HashSet<int>> CloneGraph(Dictionary<(int, int), HashSet<int>> graph)
        {
            // Copie du graphe pour éviter les modifications accidentelles
            return graph.ToDictionary(entry => entry.Key, entry => new HashSet<int>(entry.Value));
        }
    }
}
