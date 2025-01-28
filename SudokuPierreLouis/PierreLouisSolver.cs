using System;

namespace Sudoku.Benchmark
{
    public class PierreLouisSolver
    {
        private int[,] _grid;

        public PierreLouisSolver(int[,] grid)
        {
            _grid = grid;
        }

        public bool Solve()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (_grid[row, col] == 0)
                    {
                        for (int num = 1; num <= 9; num++)
                        {
                            if (IsValid(row, col, num))
                            {
                                _grid[row, col] = num;

                                if (Solve())
                                {
                                    return true;
                                }

                                _grid[row, col] = 0; // Backtrack
                            }
                        }
                        return false; // Aucun chiffre ne fonctionne ici
                    }
                }
            }
            return true; // Résolu
        }

        private bool IsValid(int row, int col, int num)
        {
            // Vérifie la ligne
            for (int x = 0; x < 9; x++)
            {
                if (_grid[row, x] == num)
                {
                    return false;
                }
            }

            // Vérifie la colonne
            for (int x = 0; x < 9; x++)
            {
                if (_grid[x, col] == num)
                {
                    return false;
                }
            }

            // Vérifie la sous-grille 3x3
            int startRow = row / 3 * 3;
            int startCol = col / 3 * 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_grid[startRow + i, startCol + j] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int[,] GetGrid()
        {
            return _grid;
        }
    }
}