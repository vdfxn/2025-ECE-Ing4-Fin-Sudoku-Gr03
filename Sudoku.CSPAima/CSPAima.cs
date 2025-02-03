using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.CSPAima
{
    public class CSPAimaSolver : ISudokuSolver
    {
        private const int Size = 9; // Taille du Sudoku 9x9

        public static bool Solve(int[,] grid)
        {
            int row, col;

            // Trouver une case vide
            if (!FindEmptyCell(grid, out row, out col))
                return true; // Si plus de cases vides, le Sudoku est résolu

            // Liste des valeurs possibles (Forward Checking)
            List<int> possibleValues = GetValidValues(grid, row, col);

            foreach (int num in possibleValues)
            {
                grid[row, col] = num;

                // Récursion avec Backtracking
                if (Solve(grid))
                    return true;

                // Si la solution ne marche pas, on backtrack
                grid[row, col] = 0;
            }

            return false;
        }

        private static bool FindEmptyCell(int[,] grid, out int row, out int col)
        {
            for (row = 0; row < Size; row++)
            {
                for (col = 0; col < Size; col++)
                {
                    if (grid[row, col] == 0)
                        return true;
                }
            }
            return false;
        }

        private static List<int> GetValidValues(int[,] grid, int row, int col)
        {
            HashSet<int> usedValues = new HashSet<int>();

            // Vérifier la ligne et la colonne
            for (int i = 0; i < Size; i++)
            {
                usedValues.Add(grid[row, i]);
                usedValues.Add(grid[i, col]);
            }

            // Vérifier la sous-grille 3x3
            int startRow = row / 3 * 3;
            int startCol = col / 3 * 3;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    usedValues.Add(grid[startRow + i, startCol + j]);
                }
            }

            // Renvoyer les valeurs possibles
            return Enumerable.Range(1, 9).Where(n => !usedValues.Contains(n)).ToList();
        }

        public static void PrintGrid(int[,] grid)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Console.Write(grid[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int[,] grid = {
                { 5, 3, 0, 0, 7, 0, 0, 0, 0 },
                { 6, 0, 0, 1, 9, 5, 0, 0, 0 },
                { 0, 9, 8, 0, 0, 0, 0, 6, 0 },
                { 8, 0, 0, 0, 6, 0, 0, 0, 3 },
                { 4, 0, 0, 8, 0, 3, 0, 0, 1 },
                { 7, 0, 0, 0, 2, 0, 0, 0, 6 },
                { 0, 6, 0, 0, 0, 0, 2, 8, 0 },
                { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
                { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
            };

            Console.WriteLine("Sudoku initial :\n");
            CSPAimaSolver.PrintGrid(grid);

            if (CSPAimaSolver.Solve(grid))
            {
                Console.WriteLine("\nSolution trouvée :\n");
                CSPAimaSolver.PrintGrid(grid);
            }
            else
            {
                Console.WriteLine("\nAucune solution trouvée.");
            }
        }
    }
}
